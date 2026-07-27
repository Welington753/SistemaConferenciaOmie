using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Repositories;
using SistemaConferenciaPedidos.Helpers;
using System.IO;
using System.IO.Compression;
using UglyToad.PdfPig;

namespace SistemaConferenciaPedidos.Services
{
    public class ValidacaoPreImpressaoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly EtiquetaService _etiquetaService;

        public ValidacaoPreImpressaoService(IPedidoRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
            _etiquetaService = new EtiquetaService();
        }

        public ResultadoValidacaoPreImpressao ValidarAntesDaImpressao(PedidoConferencia snapshotPedido, string caminhoZipShopee = null)
        {
            if (snapshotPedido == null)
                return ResultadoValidacaoPreImpressao.Falha("Pedido nulo recebido.", "Erro Interno");

            if (snapshotPedido.Status == "Cancelado")
                return ResultadoValidacaoPreImpressao.Falha("Pedido Cancelado ou Removido.", "Cancelado");

            // Recarregar do banco pelo Número para garantir que não foi removido nesse meio tempo
            var pedidoNoBanco = _pedidoRepository.ObterTodos().FirstOrDefault(p => p.NumeroPedidoCliente == snapshotPedido.NumeroPedidoCliente);

            if (pedidoNoBanco == null)
                return ResultadoValidacaoPreImpressao.Falha("Pedido não encontrado no banco.", "Não Encontrado");

            if (pedidoNoBanco.Status == "Cancelado")
                return ResultadoValidacaoPreImpressao.Falha("O pedido atual foi cancelado no banco.", "Cancelado no Banco");

            string etiqueta = snapshotPedido.EtiquetaMarketplaceZpl;
            
            if (string.IsNullOrWhiteSpace(etiqueta))
                return ResultadoValidacaoPreImpressao.Falha("Sem conteúdo ZPL ou PDF associado.", "Sem Etiqueta", "", StatusValidacaoPreImpressao.NaoConfirmado);

            string hash = CalcularHash(etiqueta);

            string marketplaceNormalizado = MarketplaceHelper.NormalizarMarketplace(snapshotPedido.Marketplace);

            if (marketplaceNormalizado == "SHOPEE" || etiqueta.StartsWith("PDF_SHOPEE|"))
            {
                if (!etiqueta.StartsWith("PDF_SHOPEE|"))
                    return ResultadoValidacaoPreImpressao.Falha("Conteúdo de etiqueta inválido para Shopee.", "Formato Inválido");

                var partes = etiqueta.Split('|');
                if (partes.Length < 3)
                    return ResultadoValidacaoPreImpressao.Falha("Referência de página PDF corrompida.", "Referência Quebrada", "", StatusValidacaoPreImpressao.NaoConfirmado);

                string nomePdfZpl = partes[1];
                if (!int.TryParse(partes[2], out int paginaAlvo))
                    return ResultadoValidacaoPreImpressao.Falha("Número da página PDF inválido.", "Referência Quebrada", "", StatusValidacaoPreImpressao.NaoConfirmado);

                string zipShopee = snapshotPedido.CaminhoZipImportacao;
                if (string.IsNullOrWhiteSpace(zipShopee))
                    zipShopee = caminhoZipShopee; // fallback legado

                if (string.IsNullOrWhiteSpace(zipShopee) || !File.Exists(zipShopee))
                    return ResultadoValidacaoPreImpressao.Falha("O arquivo ZIP original não foi encontrado para validar o PDF. Reimporte o arquivo desta etiqueta.", "Arquivo ZIP Ausente", "", StatusValidacaoPreImpressao.NaoConfirmado);

                string textoPagina = "";
                try
                {
                    using var archive = ZipFile.OpenRead(zipShopee);

                    // 1. Busca exata por FullName (registros novos)
                    var entry = archive.Entries.FirstOrDefault(e =>
                        e.FullName.Equals(nomePdfZpl, StringComparison.OrdinalIgnoreCase));

                    // 2. Fallback por nome base (registros antigos sem subpasta no NomePdfNoZip)
                    if (entry == null)
                    {
                        string nomeSemCaminho = Path.GetFileName(nomePdfZpl);
                        var candidatos = archive.Entries
                            .Where(e => e.FullName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) &&
                                        Path.GetFileName(e.FullName).Equals(nomeSemCaminho, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (candidatos.Count == 1)
                            entry = candidatos[0];
                        else if (candidatos.Count > 1)
                            return ResultadoValidacaoPreImpressao.Falha(
                                $"Nome de PDF '{nomeSemCaminho}' ambíguo no ZIP ({candidatos.Count} ocorrências). Reimporte o arquivo para corrigir o registro.",
                                "PDF Ambíguo");
                        // count == 0: entry permanece null → falha abaixo
                    }

                    if (entry == null)
                        return ResultadoValidacaoPreImpressao.Falha($"O PDF {nomePdfZpl} não existe dentro do ZIP.", "PDF Ausente");

                    using var stream = entry.Open();
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;

                    using var document = PdfDocument.Open(ms);
                    if (document.NumberOfPages < 1)
                        return ResultadoValidacaoPreImpressao.Falha("PDF sem páginas.", "Página Inexistente");

                    string rastreioNormalizado = TextoHelper.SomenteLetrasENumeros(snapshotPedido.CodigoEtiqueta ?? "");
                    if (string.IsNullOrWhiteSpace(rastreioNormalizado))
                        return ResultadoValidacaoPreImpressao.Falha("Pedido não possui código de etiqueta.", "Código Ausente", "", StatusValidacaoPreImpressao.NaoConfirmado);

                    int paginaCerta = -1;
                    string textoCerto = "";

                    bool CheckPage(int pageIdx, out string extractedText)
                    {
                        extractedText = "";
                        if (pageIdx < 1 || pageIdx > document.NumberOfPages) return false;
                        var pg = document.GetPage(pageIdx);
                        extractedText = pg.Text ?? "";
                        var matches = System.Text.RegularExpressions.Regex.Matches(extractedText, @"B\s*R\s*(?:[A-Z0-9]\s*){13}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        int countExatos = 0;
                        foreach (System.Text.RegularExpressions.Match m in matches)
                        {
                            if (TextoHelper.SomenteLetrasENumeros(m.Value) == rastreioNormalizado)
                                countExatos++;
                        }
                        if (countExatos == 1) return true;
                        if (countExatos > 1) return false;

                        try
                        {
                            using var msPdf = new MemoryStream();
                            ms.Position = 0;
                            ms.CopyTo(msPdf);
                            msPdf.Position = 0;
                            using var pdfDoc = PdfiumViewer.PdfDocument.Load(msPdf);
                            int zeroBased = pageIdx - 1;
                            if (zeroBased >= 0 && zeroBased < pdfDoc.PageCount)
                            {
                                using var image = pdfDoc.Render(zeroBased, 300, 300, PdfiumViewer.PdfRenderFlags.Annotations);
                                var reader = new ZXing.Windows.Compatibility.BarcodeReader();
                                reader.Options.PossibleFormats = new[] { ZXing.BarcodeFormat.CODE_128 };
                                reader.Options.TryHarder = true;
                                using var bmp = new System.Drawing.Bitmap(image);
                                var result = reader.Decode(bmp);
                                if (result != null && result.Text.Contains(rastreioNormalizado))
                                {
                                    return true;
                                }
                            }
                        }
                        catch { }

                        return false;
                    }

                    if (CheckPage(paginaAlvo, out textoCerto))
                    {
                        paginaCerta = paginaAlvo;
                    }
                    else
                    {
                        for (int i = 1; i <= document.NumberOfPages; i++)
                        {
                            if (i == paginaAlvo) continue;
                            if (CheckPage(i, out textoCerto))
                            {
                                paginaCerta = i;
                                break;
                            }
                        }
                    }

                    if (paginaCerta == -1)
                        return ResultadoValidacaoPreImpressao.Falha("Código não encontrado no ZIP importado", "Código PDF Inválido", "", StatusValidacaoPreImpressao.NaoConfirmado);

                    if (paginaCerta != paginaAlvo)
                    {
                        snapshotPedido.PaginaPdf = paginaCerta;
                        snapshotPedido.EtiquetaMarketplaceZpl = $"PDF_SHOPEE|{nomePdfZpl}|{paginaCerta}";
                        _pedidoRepository.SalvarOuAtualizar(snapshotPedido);
                    }

                    textoPagina = textoCerto;
                    if (string.IsNullOrWhiteSpace(textoPagina))
                        textoPagina = rastreioNormalizado; // fallback pro hash não falhar se só achou via código de barras
                }
                catch (Exception ex)
                {
                    return ResultadoValidacaoPreImpressao.Falha($"Erro ao extrair PDF da Shopee: {ex.Message}", "Erro de Extração PDF", "", StatusValidacaoPreImpressao.NaoConfirmado);
                }

                hash = CalcularHash(textoPagina);
            }
            else if (marketplaceNormalizado == "MERCADO LIVRE" && etiqueta.StartsWith("PDF_MELI|"))
            {
                // As páginas de ETIQUETA do Mercado Livre NÃO contêm o NumeroVenda —
                // apenas o código de barras (47XXXXXXXXX) e o Pack ID.
                // A prova de vínculo é: CodigoEtiqueta do pedido == código extraído da página.
                var partes = etiqueta.Split('|');
                if (partes.Length < 2 || !int.TryParse(partes[1], out int paginaAlvo))
                    return ResultadoValidacaoPreImpressao.Falha("Referência de página PDF corrompida.", "Referência Quebrada", "", StatusValidacaoPreImpressao.NaoConfirmado);

                string pdfCaminho = snapshotPedido.CaminhoZipImportacao;
                if (string.IsNullOrWhiteSpace(pdfCaminho) || !File.Exists(pdfCaminho))
                    return ResultadoValidacaoPreImpressao.Falha("O arquivo PDF original não foi encontrado para validar. Reimporte o arquivo desta etiqueta.", "Arquivo PDF Ausente", "", StatusValidacaoPreImpressao.NaoConfirmado);

                string textoPagina = "";
                try
                {
                    using var document = PdfDocument.Open(pdfCaminho);
                    if (paginaAlvo < 1 || paginaAlvo > document.NumberOfPages)
                        return ResultadoValidacaoPreImpressao.Falha($"Página {paginaAlvo} não existe no PDF.", "Página Inexistente");

                    var page = document.GetPage(paginaAlvo);
                    textoPagina = page.Text ?? "";
                }
                catch (Exception ex)
                {
                    return ResultadoValidacaoPreImpressao.Falha($"Erro ao extrair PDF do Mercado Livre: {ex.Message}", "Erro de Extração PDF", "", StatusValidacaoPreImpressao.NaoConfirmado);
                }

                if (string.IsNullOrWhiteSpace(textoPagina))
                    return ResultadoValidacaoPreImpressao.Falha("Não foi possível extrair o texto da página do Mercado Livre. Conteúdo não validável.", "PDF Sem Texto", "", StatusValidacaoPreImpressao.NaoConfirmado);

                hash = CalcularHash(textoPagina);

                // Extrair o código de rastreio da página usando a MESMA lógica do importador
                var servicoPdfMeli = new MercadoLivrePdfService();
                string codigoExtraido = TextoHelper.SomenteLetrasENumeros(
                    servicoPdfMeli.ExtrairCodigoEtiqueta(textoPagina) ?? "");

                string codigoPedido = TextoHelper.SomenteLetrasENumeros(snapshotPedido.CodigoEtiqueta ?? "");

                if (string.IsNullOrWhiteSpace(codigoExtraido))
                    return ResultadoValidacaoPreImpressao.Falha(
                        "Código de rastreio não encontrado na página do PDF. Conteúdo do PDF não validável.",
                        "Código PDF Inválido", "", StatusValidacaoPreImpressao.NaoConfirmado);

                if (string.IsNullOrWhiteSpace(codigoPedido))
                    return ResultadoValidacaoPreImpressao.Falha(
                        "O pedido não possui CodigoEtiqueta para validação.",
                        "Código Pedido Vazio", "", StatusValidacaoPreImpressao.NaoConfirmado);

                if (codigoExtraido != codigoPedido)
                    return ResultadoValidacaoPreImpressao.Falha(
                        $"O código extraído do PDF ({codigoExtraido}) não corresponde ao pedido ({codigoPedido}).",
                        "Código PDF Inválido");
            }

            else
            {
                // ZPL puro (Amazon ou ML)
                string numeroOriginal = (snapshotPedido.NumeroPedidoCliente ?? "").Trim();
                string codigoRastreioOriginal = (snapshotPedido.CodigoEtiqueta ?? "").Trim();
                
                string numeroNormalizado = TextoHelper.NormalizarTexto(numeroOriginal);
                string numeroSemCaracteres = TextoHelper.SomenteLetrasENumeros(numeroOriginal);
                
                string zplNormalizado = TextoHelper.NormalizarTexto(etiqueta);
                string zplSemCaracteres = TextoHelper.SomenteLetrasENumeros(etiqueta);

                // Prova de Rastreio (Deve existir de forma exata)
                string codigoRastreioSemCaracteres = TextoHelper.SomenteLetrasENumeros(codigoRastreioOriginal);
                bool rastreioLocalizado = false;

                if (!string.IsNullOrWhiteSpace(codigoRastreioSemCaracteres))
                {
                    if (zplSemCaracteres.Contains(codigoRastreioSemCaracteres))
                        rastreioLocalizado = true;
                    else
                    {
                        // Tentar usar extração especializada
                        string zplDecodificado = _etiquetaService.DecodificarHexAmazon(etiqueta);
                        string zplDecodSemCaracteres = TextoHelper.SomenteLetrasENumeros(zplDecodificado);
                        
                        if (zplDecodSemCaracteres.Contains(codigoRastreioSemCaracteres))
                            rastreioLocalizado = true;
                        else
                        {
                            string fdDecod = _etiquetaService.ExtrairTextosFdDoZpl(etiqueta);
                            string fdDecodSemCaracteres = TextoHelper.SomenteLetrasENumeros(fdDecod);
                            if (fdDecodSemCaracteres.Contains(codigoRastreioSemCaracteres))
                                rastreioLocalizado = true;
                        }
                    }
                }

                // Prova de Número
                bool numeroLocalizado = false;

                if (marketplaceNormalizado == "MERCADO LIVRE" && numeroOriginal.StartsWith("20000") && numeroOriginal.Length > 5)
                {
                    string numeroSemPrefixo = numeroOriginal.Substring(5);
                    string numeroSemPrefixoSemCaracteres = TextoHelper.SomenteLetrasENumeros(numeroSemPrefixo);
                    
                    if (zplSemCaracteres.Contains(numeroSemPrefixoSemCaracteres))
                        numeroLocalizado = true;
                    else
                    {
                        string zplDecodificado = _etiquetaService.DecodificarHexAmazon(etiqueta);
                        string zplDecodSemCaracteres = TextoHelper.SomenteLetrasENumeros(zplDecodificado);
                        if (zplDecodSemCaracteres.Contains(numeroSemPrefixoSemCaracteres))
                            numeroLocalizado = true;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(numeroSemCaracteres) && zplSemCaracteres.Contains(numeroSemCaracteres))
                        numeroLocalizado = true;
                    else
                    {
                        string zplDecodificado = _etiquetaService.DecodificarHexAmazon(etiqueta);
                        string zplDecodSemCaracteres = TextoHelper.SomenteLetrasENumeros(zplDecodificado);
                        if (!string.IsNullOrWhiteSpace(numeroSemCaracteres) && zplDecodSemCaracteres.Contains(numeroSemCaracteres))
                            numeroLocalizado = true;
                    }
                }

                if (!numeroLocalizado)
                    return ResultadoValidacaoPreImpressao.Falha("Número do pedido não consta no ZPL.", "Número ZPL Inválido", "", StatusValidacaoPreImpressao.NaoConfirmado);

                if (!rastreioLocalizado)
                {
                    // Se o rastreio não foi localizado mas o número do pedido FOI localizado de maneira confiável,
                    // validamos com aviso em modo fail-closed parcial (só deixamos passar se o número da venda bater exatamente).
                    // Temos de ter cuidado de garantir duplicidades
                    return ValidarDuplicidadesFinal(snapshotPedido, hash, dataAlvo: snapshotPedido.DataPrevisao ?? snapshotPedido.DataCriacao, statusEspecial: "Válido com rastreio não extraível do ZPL");
                }
            }

            return ValidarDuplicidadesFinal(snapshotPedido, hash, dataAlvo: snapshotPedido.DataPrevisao ?? snapshotPedido.DataCriacao, statusEspecial: null);
        }

        private ResultadoValidacaoPreImpressao ValidarDuplicidadesFinal(PedidoConferencia snapshotPedido, string hash, DateTime dataAlvo, string statusEspecial)
        {
            var ativosDoDia = _pedidoRepository.ObterTodos()
                .Where(p => p.Status != "Cancelado")
                .Where(p => (p.DataPrevisao.HasValue && p.DataPrevisao.Value.Date == dataAlvo.Date) || (!p.DataPrevisao.HasValue && p.DataCriacao.Date == dataAlvo.Date))
                .ToList();

            var duplicadoCodigo = ativosDoDia.FirstOrDefault(p => p.NumeroPedidoCliente != snapshotPedido.NumeroPedidoCliente && !string.IsNullOrWhiteSpace(p.CodigoEtiqueta) && p.CodigoEtiqueta == snapshotPedido.CodigoEtiqueta);
            if (duplicadoCodigo != null)
                return ResultadoValidacaoPreImpressao.Falha($"O Código de Rastreio está duplicado com o pedido: {duplicadoCodigo.NumeroPedidoCliente}", "Duplicidade de Código");

            var duplicadoEtiqueta = ativosDoDia.FirstOrDefault(p => p.NumeroPedidoCliente != snapshotPedido.NumeroPedidoCliente && !string.IsNullOrWhiteSpace(p.EtiquetaMarketplaceZpl) && p.EtiquetaMarketplaceZpl == snapshotPedido.EtiquetaMarketplaceZpl);
            if (duplicadoEtiqueta != null)
                return ResultadoValidacaoPreImpressao.Falha($"A Etiqueta ZPL/PDF está duplicada com o pedido: {duplicadoEtiqueta.NumeroPedidoCliente}", "Duplicidade de Etiqueta");

            if (!string.IsNullOrWhiteSpace(statusEspecial))
            {
                return ResultadoValidacaoPreImpressao.AprovadoComAviso(
                    snapshotPedido.NumeroPedidoCliente,
                    snapshotPedido.NumeroPedidoCliente,
                    snapshotPedido.Marketplace,
                    snapshotPedido.CodigoEtiqueta,
                    hash,
                    statusEspecial
                );
            }

            return ResultadoValidacaoPreImpressao.Aprovado(
                snapshotPedido.NumeroPedidoCliente,
                snapshotPedido.NumeroPedidoCliente,
                snapshotPedido.Marketplace,
                snapshotPedido.CodigoEtiqueta,
                hash
            );
        }

        private string CalcularHash(string texto)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                var sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
