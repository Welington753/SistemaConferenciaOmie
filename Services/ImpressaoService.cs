using BinaryKits.Zpl.Viewer;
using SistemaConferenciaPedidos.Models;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SistemaConferenciaPedidos.Services
{
    public class ImpressaoService
    {
        private readonly IPrinterEnvironment _printerEnvironment;

        public ImpressaoService() : this(new WindowsPrinterEnvironment()) { }

        public ImpressaoService(IPrinterEnvironment printerEnvironment)
        {
            _printerEnvironment = printerEnvironment ?? new WindowsPrinterEnvironment();
        }

        public ResultadoImpressao ImprimirZplComoImagem(string zpl)
        {
            IPrinterStorage printerStorage = new PrinterStorage();
            var analyzer = new ZplAnalyzer(printerStorage);
            var drawer = new ZplElementDrawer(printerStorage);

            var analyzeInfo = analyzer.Analyze(zpl);

            if (analyzeInfo == null || analyzeInfo.LabelInfos == null || !analyzeInfo.LabelInfos.Any())
            {
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ConteudoInvalido, "Não foi possível converter a etiqueta ZPL.");
            }

            byte[] imageData = drawer.Draw(analyzeInfo.LabelInfos.First().ZplElements);

            if (imageData == null || imageData.Length == 0)
            {
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ConteudoInvalido, "A imagem da etiqueta ficou vazia.");
            }

            using var ms = new MemoryStream(imageData);
            using var imagemOriginal = Image.FromStream(ms);
            using var bitmapFinal = new Bitmap(1200, 1800);

            using (Graphics g = Graphics.FromImage(bitmapFinal))
            {
                g.Clear(Color.White);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                Rectangle areaDestino = new Rectangle(0, 0, bitmapFinal.Width, bitmapFinal.Height);

                float proporcaoImagem = (float)imagemOriginal.Width / imagemOriginal.Height;
                float proporcaoArea = (float)areaDestino.Width / areaDestino.Height;

                int larguraFinal;
                int alturaFinal;

                if (proporcaoImagem > proporcaoArea)
                {
                    larguraFinal = areaDestino.Width;
                    alturaFinal = (int)(areaDestino.Width / proporcaoImagem);
                }
                else
                {
                    alturaFinal = areaDestino.Height;
                    larguraFinal = (int)(areaDestino.Height * proporcaoImagem);
                }

                int x = (areaDestino.Width - larguraFinal) / 2;
                int y = (areaDestino.Height - alturaFinal) / 2;

                g.DrawImage(imagemOriginal, new Rectangle(x, y, larguraFinal, alturaFinal));
            }

            using PrintDocument pd = new PrintDocument();

            var impressoraTermica = ObterImpressoraTermicaPreferida();
            if (impressoraTermica == null)
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ImpressoraNaoEncontrada, "A impressora térmica configurada não foi encontrada. Verifique a conexão ou selecione novamente a impressora na Administração.");

            pd.PrinterSettings.PrinterName = impressoraTermica.PrinterName;

            if (!pd.PrinterSettings.IsValid)
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ImpressoraNaoEncontrada, "A impressora térmica encontrada não está válida para impressão.");

            pd.DefaultPageSettings.PrinterSettings.PrinterName = impressoraTermica.PrinterName;
            pd.PrinterSettings.DefaultPageSettings.PrinterSettings.PrinterName = impressoraTermica.PrinterName;

            pd.DefaultPageSettings.PaperSize = ObterPapelEtiqueta(pd);
            pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            pd.DefaultPageSettings.Landscape = false;
            pd.OriginAtMargins = false;
            pd.PrintController = new StandardPrintController();

            pd.PrintPage += (s, e) =>
            {
                Rectangle area = e.MarginBounds;
                e.Graphics.DrawImage(bitmapFinal, area);
                e.HasMorePages = false;
            };

            pd.Print();
            return ResultadoImpressao.Ok("ZPL", impressoraTermica.PrinterName);
        }

        public virtual ResultadoImpressao ImprimirPedido(
    PedidoConferencia pedido,
    string caminhoZip,
    string caminhoPdfMercadoLivre)
        {
            if (pedido == null)
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ConteudoInvalido, "Pedido não informado.");

            string etiqueta = (pedido.EtiquetaMarketplaceZpl ?? "").Trim();

            if (string.IsNullOrWhiteSpace(etiqueta))
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ConteudoInvalido, "O pedido não possui etiqueta vinculada.");

            string marketplace = (pedido.Marketplace ?? "").Trim();

            if (marketplace.Equals("Shopee", StringComparison.OrdinalIgnoreCase) &&
                etiqueta.StartsWith("PDF_SHOPEE|", StringComparison.OrdinalIgnoreCase))
            {
                return ImprimirEtiquetaShopeePdf(pedido, caminhoZip);
            }

            if (marketplace.Equals(
        "Mercado Livre",
        StringComparison.OrdinalIgnoreCase) &&
    etiqueta.StartsWith(
        "PDF_MELI|",
        StringComparison.OrdinalIgnoreCase))
            {
                return ImprimirEtiquetaMercadoLivrePdf(
                    pedido,
                    caminhoPdfMercadoLivre);
            }

            return ImprimirZplComoImagem(etiqueta);
        }

        public ResultadoImpressao ImprimirEtiquetaMercadoLivrePdf(
    PedidoConferencia pedido,
    string caminhoPdfMercadoLivre)
        {
            if (pedido == null)
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ConteudoInvalido, "Pedido não informado.");

            if (string.IsNullOrWhiteSpace(caminhoPdfMercadoLivre) ||
                !File.Exists(caminhoPdfMercadoLivre))
            {
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ArquivoAusente, "O PDF do Mercado Livre não está carregado. Importe novamente.");
            }

            string referencia =
                pedido.EtiquetaMarketplaceZpl ?? "";

            if (!referencia.StartsWith("PDF_MELI|", StringComparison.OrdinalIgnoreCase))
            {
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ConteudoInvalido, "O pedido não possui uma página de PDF do Mercado Livre vinculada.");
            }

            string paginaTexto = referencia
                .Replace(
                    "PDF_MELI|",
                    "",
                    StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (!int.TryParse(paginaTexto, out int pagina))
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ConteudoInvalido, "Página do PDF Mercado Livre inválida.");

            using var bitmapOriginal =
                RenderizarPaginaPdfComoBitmap(
                    caminhoPdfMercadoLivre,
                    pagina);

            if (bitmapOriginal == null)
                return ResultadoImpressao.Falha(StatusResultadoImpressao.FalhaAoPreparar, "Não foi possível renderizar a página do PDF.");

            using var bitmapCortado =
                CortarMargensBrancas(bitmapOriginal);

            if (bitmapCortado == null)
                return ResultadoImpressao.Falha(StatusResultadoImpressao.FalhaAoPreparar, "Não foi possível preparar a etiqueta para impressão.");

            var resultado = ImprimirBitmapNaTermica(bitmapCortado);
            if (!resultado.Sucesso) return resultado;
            
            return ResultadoImpressao.Ok("MercadoLivre_PDF", resultado.Impressora);
        }

        public ResultadoImpressao ImprimirEtiquetaShopeePdf(PedidoConferencia pedido, string caminhoZip)
        {
            try
            {
                if (pedido == null)
                    return ResultadoImpressao.Falha(StatusResultadoImpressao.ConteudoInvalido, "Pedido nulo.");

                if (string.IsNullOrWhiteSpace(caminhoZip) || !File.Exists(caminhoZip))
                    return ResultadoImpressao.Falha(StatusResultadoImpressao.ArquivoAusente, "ZIP não encontrado.");

                if (string.IsNullOrWhiteSpace(pedido.EtiquetaMarketplaceZpl) ||
                    !pedido.EtiquetaMarketplaceZpl.StartsWith("PDF_SHOPEE|", StringComparison.OrdinalIgnoreCase))
                    return ResultadoImpressao.Falha(StatusResultadoImpressao.ConteudoInvalido, "Referência da Shopee inválida.");

                string[] partesShopee = pedido.EtiquetaMarketplaceZpl.Split('|');
                
                if (partesShopee.Length < 3)
                    return ResultadoImpressao.Falha(StatusResultadoImpressao.ConteudoInvalido, "Formato da referência Shopee inválido.");

                string nomePdfCorreto = partesShopee[1];
                string paginaTexto = partesShopee[2];

                if (!int.TryParse(paginaTexto, out int pagina))
                    return ResultadoImpressao.Falha(StatusResultadoImpressao.ConteudoInvalido, "Página da Shopee inválida.");

                string pastaTemp = Path.Combine(Path.GetTempPath(), "SistemaConferenciaPedidos");
                Directory.CreateDirectory(pastaTemp);

                string caminhoPdfOriginal = Path.Combine(pastaTemp, "Shopee_Completo.pdf");

                using (var zip = System.IO.Compression.ZipFile.OpenRead(caminhoZip))
                {
                    var entryPdf = zip.Entries.FirstOrDefault(e =>
                        e.FullName.Equals(nomePdfCorreto, StringComparison.OrdinalIgnoreCase));

                    if (entryPdf == null)
                        return ResultadoImpressao.Falha(StatusResultadoImpressao.ArquivoAusente, "PDF não encontrado dentro do ZIP.");

                    using var stream = entryPdf.Open();
                    using var fs = new FileStream(caminhoPdfOriginal, FileMode.Create, FileAccess.Write);
                    stream.CopyTo(fs);
                }

                using var bitmapOriginal = RenderizarPaginaPdfComoBitmap(caminhoPdfOriginal, pagina);

                if (bitmapOriginal == null)
                    return ResultadoImpressao.Falha(StatusResultadoImpressao.FalhaAoPreparar, "Falha ao renderizar PDF Shopee.");

                using var bitmapCortado = CortarMargensBrancas(bitmapOriginal);

                if (bitmapCortado == null)
                    return ResultadoImpressao.Falha(StatusResultadoImpressao.FalhaAoPreparar, "Falha ao cortar margens do PDF Shopee.");

                var resultado = ImprimirBitmapNaTermica(bitmapCortado);
                if (!resultado.Sucesso) return resultado;
                
                return ResultadoImpressao.Ok("Shopee_PDF", resultado.Impressora);
            }
            catch (Exception ex)
            {
                return ResultadoImpressao.Desconhecido("Erro ao imprimir etiqueta Shopee: " + ex.Message, ex);
            }
        }
        private Bitmap RenderizarPaginaPdfComoBitmap(string caminhoPdf, int numeroPagina)
        {
            if (string.IsNullOrWhiteSpace(caminhoPdf) || !File.Exists(caminhoPdf))
                return null;

            if (numeroPagina <= 0)
                return null;

            using var document = PdfiumViewer.PdfDocument.Load(caminhoPdf);

            if (numeroPagina > document.PageCount)
                return null;

            int largura = 1200;
            int altura = 1800;

            using var imagem = document.Render(
                numeroPagina - 1,
                largura,
                altura,
                300,
                300,
                PdfiumViewer.PdfRenderFlags.ForPrinting);

            return new Bitmap(imagem);
        }

        private Bitmap CortarMargensBrancas(Bitmap original)
        {
            if (original == null)
                return null;

            int esquerda = original.Width;
            int topo = original.Height;
            int direita = 0;
            int baixo = 0;

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Color c = original.GetPixel(x, y);

                    bool ehBranco = c.R > 245 && c.G > 245 && c.B > 245;

                    if (!ehBranco)
                    {
                        if (x < esquerda) esquerda = x;
                        if (y < topo) topo = y;
                        if (x > direita) direita = x;
                        if (y > baixo) baixo = y;
                    }
                }
            }

            if (direita <= esquerda || baixo <= topo)
                return new Bitmap(original);

            int margem = 10;

            esquerda = Math.Max(0, esquerda - margem);
            topo = Math.Max(0, topo - margem);
            direita = Math.Min(original.Width - 1, direita + margem);
            baixo = Math.Min(original.Height - 1, baixo + margem);

            Rectangle area = new Rectangle(
                esquerda,
                topo,
                direita - esquerda + 1,
                baixo - topo + 1);

            return original.Clone(area, original.PixelFormat);
        }

        public virtual ResultadoImpressao ImprimirBitmapNaTermica(Bitmap bitmap)
        {
            if (bitmap == null)
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ConteudoInvalido, "Falha ao preparar: bitmap está nulo.");

            using PrintDocument pd = new PrintDocument();

            var impressoraTermica = ObterImpressoraTermicaPreferida();
            if (impressoraTermica == null)
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ImpressoraNaoEncontrada, "A impressora térmica configurada não foi encontrada. Verifique a conexão ou selecione novamente a impressora na Administração.");

            pd.PrinterSettings.PrinterName = impressoraTermica.PrinterName;

            if (!_printerEnvironment.ConfiguracaoValida(impressoraTermica.PrinterName))
                return ResultadoImpressao.Falha(StatusResultadoImpressao.ImpressoraNaoEncontrada, "A impressora térmica configurada não está válida.");

            pd.DefaultPageSettings.PrinterSettings.PrinterName = impressoraTermica.PrinterName;
            pd.DefaultPageSettings.PaperSize = new PaperSize("4x6", 400, 600);
            pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            pd.DefaultPageSettings.Landscape = false;
            pd.OriginAtMargins = false;
            pd.PrintController = new StandardPrintController();

            pd.PrintPage += (s, e) =>
            {
                Rectangle area = new Rectangle(0, 0, e.PageBounds.Width, e.PageBounds.Height);

                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                e.Graphics.DrawImage(bitmap, area);
                e.HasMorePages = false;
            };

            try
            {
                pd.Print();
                return ResultadoImpressao.Ok("Bitmap", impressoraTermica.PrinterName);
            }
            catch (Exception ex)
            {
                return ResultadoImpressao.Desconhecido("Erro desconhecido durante o envio à fila: " + ex.Message, ex);
            }
        }
        public PrinterSettings ObterImpressoraTermicaPreferida()
        {
            try
            {
                var nomes = _printerEnvironment.ObterImpressorasInstaladas().ToList();

            string[] nomesPreferidos =
            {
                "tomate",
                "tb",
                "MDK-007",
                "label",
                "etiqueta",
                "thermal"
            };

            foreach (string preferido in nomesPreferidos)
            {
                string encontrada = nomes.FirstOrDefault(x =>
                    x.IndexOf(preferido, StringComparison.OrdinalIgnoreCase) >= 0);

                if (!string.IsNullOrWhiteSpace(encontrada))
                {
                    if (_printerEnvironment.ConfiguracaoValida(encontrada))
                        return new PrinterSettings { PrinterName = encontrada };
                }
            }
            }
            catch
            {
                // Registra o erro ou ignora, retornando null para indicar falha
            }

            return null;
        }

        public PaperSize ObterPapelEtiqueta(PrintDocument pd)
        {
            foreach (PaperSize ps in pd.PrinterSettings.PaperSizes)
            {
                string nome = (ps.PaperName ?? "").ToLowerInvariant();

                bool nomeCompativel =
                    nome.Contains("10x15") ||
                    nome.Contains("100x150") ||
                    nome.Contains("4x6") ||
                    nome.Contains("4 x 6");

                bool tamanhoCompativel =
                    (Math.Abs(ps.Width - 400) <= 20 && Math.Abs(ps.Height - 600) <= 20) ||
                    (Math.Abs(ps.Width - 600) <= 20 && Math.Abs(ps.Height - 400) <= 20);

                if (nomeCompativel || tamanhoCompativel)
                    return ps;
            }

            return new PaperSize("Etiqueta 10x15", 400, 600);
        }
    }
}