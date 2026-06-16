using BinaryKits.Zpl.Viewer;
using PdfiumViewer;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Helpers;
using SistemaConferenciaPedidos.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using UglyToad.PdfPig;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using SistemaConferenciaPedidos.Repositories;
using SistemaOmie.Shared.Services;




namespace SistemaConferenciaPedidos
{
    public partial class FrmPreparacaoPedidos : Form
    {


        private string _jsonPedidoSelecionado = "[]";
        private PedidoConferencia _pedidoSelecionado = null;
        private readonly List<EtiquetaMarketplaceLote> _etiquetasLote = new List<EtiquetaMarketplaceLote>();
        private bool _carregandoPedidos = false;
        private readonly Dictionary<string, string> _cachePedidoShopeeOcr = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<EtiquetaShopeePdf> _etiquetasShopeePdf = new List<EtiquetaShopeePdf>();
        private readonly PedidoItemService _pedidoItemService = new PedidoItemService();
        private readonly EtiquetaService _etiquetaService = new EtiquetaService();
        private readonly ImpressaoService _impressaoService = new ImpressaoService();
        private readonly PedidoOmieService _pedidoOmieService = new PedidoOmieService();
        private readonly ShopeePdfService _shopeePdfService = new ShopeePdfService();
        private readonly LeituraCodigoService _leituraCodigoService = new LeituraCodigoService();
        private readonly ValidacaoEanService _validacaoEanService = new ValidacaoEanService();
        private readonly VinculacaoEtiquetaService _vinculacaoEtiquetaService = new VinculacaoEtiquetaService();
        private readonly IPedidoRepository _pedidoRepository = new PedidoRepositorySqlite();
        private readonly PedidoProdutoBuscaService _pedidoProdutoBuscaService = new PedidoProdutoBuscaService();
        private string _caminhoPdfShopee = "";
        private string _ultimoArquivoZipImportado = "";
        private string _nomePdfShopeeNoZip = "";


        public FrmPreparacaoPedidos()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            btnSalvarPedido.Text = "Importar Etiquetas do Lote";
            btnGerarEtiqueta.Text = "Conferir";

            dtpDataInicial.Value = DateTime.Today;
            dtpDataFinal.Value = DateTime.Today;

            ConfigurarGrids();
            CarregarPedidos();
        }






        private string NormalizarCodigoPedidoShopee(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            string valor = texto.ToUpperInvariant();

            // mantém só letras e números
            valor = Regex.Replace(valor, @"[^A-Z0-9]", "");

            return valor.Trim();
        }

        private string CanonicalizarCodigoShopee(string valor)
        {
            valor = NormalizarCodigoPedidoShopee(valor);

            if (string.IsNullOrWhiteSpace(valor))
                return "";

            return valor
                .Replace("0", "O")
                .Replace("1", "I")
                .Replace("5", "S")
                .Replace("8", "B")
                .Replace("2", "Z");
        }

        private int DistanciaLevenshtein(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
                return string.IsNullOrEmpty(b) ? 0 : b.Length;

            if (string.IsNullOrEmpty(b))
                return a.Length;

            int[,] dp = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++)
                dp[i, 0] = i;

            for (int j = 0; j <= b.Length; j++)
                dp[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int custo = a[i - 1] == b[j - 1] ? 0 : 1;

                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + custo);
                }
            }

            return dp[a.Length, b.Length];
        }

        private int CalcularDistanciaShopee(string pedidoOmie, string pedidoOcr)
        {
            string omie = CanonicalizarCodigoShopee(pedidoOmie);
            string ocr = CanonicalizarCodigoShopee(pedidoOcr);

            if (string.IsNullOrWhiteSpace(omie) || string.IsNullOrWhiteSpace(ocr))
                return int.MaxValue;

            return DistanciaLevenshtein(omie, ocr);
        }

        private string ExtrairCodigo14Shopee(string texto)
        {
            string valor = NormalizarCodigoPedidoShopee(texto);

            if (string.IsNullOrWhiteSpace(valor))
                return "";

            var match = Regex.Match(valor, @"\b[0-9]{6}[A-Z0-9]{8}\b", RegexOptions.IgnoreCase);

            if (match.Success)
                return match.Value.ToUpperInvariant();

            return "";
        }










        private bool PedidoShopeeConfere(string pedidoOmie, string pedidoOcr)
        {
            if (string.IsNullOrWhiteSpace(pedidoOmie) || string.IsNullOrWhiteSpace(pedidoOcr))
                return false;

            string omie = NormalizarCodigoPedidoShopee(pedidoOmie);
            string ocr = NormalizarCodigoPedidoShopee(pedidoOcr);

            if (omie.Length < 10 || ocr.Length < 10)
                return false;

            // Correções comuns do OCR
            string Corrigir(string s) => s
                .Replace("0", "O")
                .Replace("1", "I")
                .Replace("5", "S")
                .Replace("8", "B")
                .Replace("2", "Z"); // <-- NOVO

            omie = Corrigir(omie);
            ocr = Corrigir(ocr);

            if (omie == ocr)
                return true;

            // 🔥 NOVO: compara por DISTÂNCIA (tipo Levenshtein simplificado)
            int diferencas = 0;
            int tamanho = Math.Min(omie.Length, ocr.Length);

            for (int i = 0; i < tamanho; i++)
            {
                if (omie[i] != ocr[i])
                    diferencas++;
            }

            // penaliza diferença de tamanho também
            diferencas += Math.Abs(omie.Length - ocr.Length);

            // 👉 regra mágica:
            // aceita até 3 erros em 14 caracteres
            if (diferencas <= 3)
                return true;

            // fallback leve
            if (omie.Contains(ocr) || ocr.Contains(omie))
                return true;

            return false;
        }
        private void ConfigurarGrids()
        {
            dgvPedidos.AutoGenerateColumns = false;
            dgvPedidos.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgvPedidos.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvPedidos.RowTemplate.Height = 28;
            dgvPedidos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPedidos.MultiSelect = false;
            dgvPedidos.ReadOnly = true;
            dgvPedidos.AllowUserToAddRows = false;
            dgvPedidos.AllowUserToDeleteRows = false;
            dgvPedidos.AllowUserToResizeRows = false;

            dgvItensPedido.AutoGenerateColumns = true;
            dgvItensPedido.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvItensPedido.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvItensPedido.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItensPedido.MultiSelect = false;
            dgvItensPedido.ReadOnly = true;
            dgvItensPedido.AllowUserToAddRows = false;
            dgvItensPedido.AllowUserToDeleteRows = false;
            dgvItensPedido.AllowUserToResizeRows = false;
        }





        private string NormalizarPedidoShopee(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return "";

            valor = valor.ToUpperInvariant();

            valor = new string(valor.Where(char.IsLetterOrDigit).ToArray());

            return valor;
        }

        private List<string> ExtrairPossiveisPedidosShopee(string texto)
        {
            var lista = new List<string>();

            if (string.IsNullOrWhiteSpace(texto))
                return lista;

            string normalizado = NormalizarPedidoShopee(texto);

            var matches = Regex.Matches(normalizado, @"26[A-Z0-9]{12}", RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                string valor = match.Value.Trim().ToUpperInvariant();

                if (valor.Length == 14 && valor.Any(char.IsLetter))
                    lista.Add(valor);
            }

            return lista
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private string CanonicalizarPedidoShopeeParaComparacao(string valor)
        {
            valor = NormalizarPedidoShopee(valor);

            if (string.IsNullOrWhiteSpace(valor))
                return "";

            return valor
                .Replace("O", "0")
                .Replace("I", "1")
                .Replace("L", "1")
                .Replace("S", "5")
                .Replace("B", "8");
        }

        private EtiquetaShopeePdf BuscarEtiquetaShopeePorPdf(PedidoConferencia pedido)
        {
            if (pedido == null || _etiquetasShopeePdf.Count == 0)
                return null;

            string numeroPedido = NormalizarPedidoShopee(pedido.NumeroPedidoCliente);

            if (string.IsNullOrWhiteSpace(numeroPedido))
                return null;

            string numeroPedidoCanonical = CanonicalizarPedidoShopeeParaComparacao(numeroPedido);

            foreach (var etiqueta in _etiquetasShopeePdf)
            {
                var candidatos = new List<string>();

                if (!string.IsNullOrWhiteSpace(etiqueta.PedidoShopee))
                    candidatos.Add(etiqueta.PedidoShopee);

                candidatos.AddRange(ExtrairPossiveisPedidosShopee(etiqueta.TextoPagina));

                candidatos = candidatos
                    .Select(NormalizarPedidoShopee)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var candidato in candidatos)
                {
                    if (candidato.Equals(numeroPedido, StringComparison.OrdinalIgnoreCase))
                        return etiqueta;

                    string candidatoCanonical = CanonicalizarPedidoShopeeParaComparacao(candidato);

                    if (!string.IsNullOrWhiteSpace(candidatoCanonical) &&
                        candidatoCanonical.Equals(numeroPedidoCanonical, StringComparison.OrdinalIgnoreCase))
                        return etiqueta;
                }

                string textoPagina = NormalizarPedidoShopee(etiqueta.TextoPagina);

                if (!string.IsNullOrWhiteSpace(textoPagina) &&
                    textoPagina.Contains(numeroPedido))
                {
                    return etiqueta;
                }
            }

            return null;
        }

        private ResultadoMatchEtiqueta BuscarMelhorEtiquetaParaPedido(
            PedidoConferencia pedido,
            List<EtiquetaMarketplaceLote> etiquetasDisponiveis,
            bool permitirFallbackShopeePorPosicao)
        {
            if (pedido == null || etiquetasDisponiveis == null || etiquetasDisponiveis.Count == 0)
                return null;

            string marketplacePedido = MarketplaceHelper.NormalizarMarketplace(pedido.Marketplace);

            if (marketplacePedido == "AMAZON" || marketplacePedido == "MERCADO LIVRE")
            {
                var etiquetaExata = BuscarEtiquetaPorCodigoDoCliente(pedido, etiquetasDisponiveis);

                if (etiquetaExata != null)
                {
                    return new ResultadoMatchEtiqueta
                    {
                        Etiqueta = etiquetaExata,
                        Pontuacao = 1000,
                        MatchForte = true,
                        Motivo = "Código do cliente encontrado na etiqueta"
                    };
                }

                return null;
            }

            if (marketplacePedido == "SHOPEE")
            {
                var etiquetaPdf = BuscarEtiquetaShopeePorPdf(pedido);

                if (etiquetaPdf != null)
                {
                    return new ResultadoMatchEtiqueta
                    {
                        Etiqueta = null,
                        Pontuacao = 1000,
                        MatchForte = true,
                        Motivo = $"Shopee vinculada pelo PDF. Pedido={etiquetaPdf.PedidoShopee}, Rastreio={etiquetaPdf.CodigoRastreio}, Página={etiquetaPdf.Pagina}"
                    };
                }

                return null;
            }

            return null;
        }


        private string BuscarEtiquetaDoPedido(PedidoConferencia pedido, bool impedirDuplicidade = true)
        {
            if (pedido == null || _etiquetasLote.Count == 0)
                return null;

            List<EtiquetaMarketplaceLote> disponiveis;

            if (impedirDuplicidade)
            {
                var etiquetasJaUsadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var outroPedido in PedidoRepository.ObterTodos())
                {
                    if (ReferenceEquals(outroPedido, pedido))
                        continue;

                    if (!string.IsNullOrWhiteSpace(outroPedido.EtiquetaMarketplaceZpl))
                        etiquetasJaUsadas.Add((outroPedido.EtiquetaMarketplaceZpl ?? "").Trim());
                }

                disponiveis = _etiquetasLote
                    .Where(e => !etiquetasJaUsadas.Contains((e.ConteudoZpl ?? "").Trim()))
                    .ToList();
            }
            else
            {
                disponiveis = new List<EtiquetaMarketplaceLote>(_etiquetasLote);
            }

            var resultado = BuscarMelhorEtiquetaParaPedido(pedido, disponiveis, true);
            return resultado?.Etiqueta?.ConteudoZpl;
        }





        private (int ml, int amazon, int shopee, int semEtiqueta) ContarPedidosVinculados()
        {
            int ml = 0;
            int amazon = 0;
            int shopee = 0;
            int semEtiqueta = 0;

            foreach (var pedido in _pedidoRepository.ObterTodos())
            {
                bool temEtiqueta = !string.IsNullOrWhiteSpace(pedido.EtiquetaMarketplaceZpl);

                if (!temEtiqueta)
                {
                    semEtiqueta++;
                    continue;
                }

                string marketplace = MarketplaceHelper.NormalizarMarketplace(pedido.Marketplace);

                if (marketplace == "MERCADO LIVRE")
                    ml++;
                else if (marketplace == "AMAZON")
                    amazon++;
                else if (marketplace == "SHOPEE")
                    shopee++;
            }

            return (ml, amazon, shopee, semEtiqueta);
        }



        private bool EtiquetaContemCodigoDoCliente(EtiquetaMarketplaceLote etiqueta, string numeroPedidoCliente, string marketplace)
        {
            if (etiqueta == null || string.IsNullOrWhiteSpace(numeroPedidoCliente))
                return false;

            string marketplaceNormalizado = MarketplaceHelper.NormalizarMarketplace(marketplace);
            string numeroOriginal = (numeroPedidoCliente ?? "").Trim();

            string numeroNormalizado = TextoHelper.NormalizarTexto(numeroOriginal);
            string numeroSemCaracteres = TextoHelper.SomenteLetrasENumeros(numeroOriginal);

            string zpl = etiqueta.ConteudoZpl ?? "";
            string zplNormalizado = etiqueta.ConteudoNormalizado ?? TextoHelper.NormalizarTexto(zpl);
            string zplSemCaracteres = TextoHelper.SomenteLetrasENumeros(zpl);

            string decodificado = etiqueta.ConteudoDecodificado ?? TextoHelper.NormalizarTexto(_etiquetaService.DecodificarHexAmazon(zpl));
            string decodificadoSemCaracteres = TextoHelper.SomenteLetrasENumeros(decodificado);

            if (!string.IsNullOrWhiteSpace(numeroNormalizado))
            {
                if (zplNormalizado.Contains(numeroNormalizado) || decodificado.Contains(numeroNormalizado))
                    return true;
            }

            if (!string.IsNullOrWhiteSpace(numeroSemCaracteres))
            {
                if (zplSemCaracteres.Contains(numeroSemCaracteres) ||
                    decodificadoSemCaracteres.Contains(numeroSemCaracteres))
                    return true;
            }

            // Regra extra para Mercado Livre:
            // às vezes o pedido vem com prefixo e a etiqueta sem ele.
            if (marketplaceNormalizado == "MERCADO LIVRE" &&
                numeroOriginal.StartsWith("20000") &&
                numeroOriginal.Length > 5)
            {
                string numeroSemPrefixo = numeroOriginal.Substring(5);
                string numeroSemPrefixoNormalizado = TextoHelper.NormalizarTexto(numeroSemPrefixo);
                string numeroSemPrefixoSemCaracteres = TextoHelper.SomenteLetrasENumeros(numeroSemPrefixo);

                if (!string.IsNullOrWhiteSpace(numeroSemPrefixoNormalizado))
                {
                    if (zplNormalizado.Contains(numeroSemPrefixoNormalizado) || decodificado.Contains(numeroSemPrefixoNormalizado))
                        return true;
                }

                if (!string.IsNullOrWhiteSpace(numeroSemPrefixoSemCaracteres))
                {
                    if (zplSemCaracteres.Contains(numeroSemPrefixoSemCaracteres) ||
                        decodificadoSemCaracteres.Contains(numeroSemPrefixoSemCaracteres))
                        return true;
                }
            }

            return false;
        }

        private EtiquetaMarketplaceLote BuscarEtiquetaPorCodigoDoCliente(PedidoConferencia pedido, List<EtiquetaMarketplaceLote> etiquetasDisponiveis)
        {
            if (pedido == null || etiquetasDisponiveis == null || etiquetasDisponiveis.Count == 0)
                return null;

            string marketplacePedido = MarketplaceHelper.NormalizarMarketplace(pedido.Marketplace);
            string numeroPedidoCliente = (pedido.NumeroPedidoCliente ?? "").Trim();

            if (string.IsNullOrWhiteSpace(numeroPedidoCliente))
                return null;

            var candidatas = etiquetasDisponiveis
                .Where(e =>
                {
                    string marketplaceEtiqueta = MarketplaceHelper.NormalizarMarketplace(e.PlataformaDetectada);
                    return string.IsNullOrWhiteSpace(marketplaceEtiqueta) || marketplaceEtiqueta == marketplacePedido;
                })
                .OrderBy(e => e.OrdemNoArquivo)
                .ToList();

            foreach (var etiqueta in candidatas)
            {
                if (EtiquetaContemCodigoDoCliente(etiqueta, numeroPedidoCliente, pedido.Marketplace))
                    return etiqueta;
            }

            return null;
        }




        private string ExtrairCodigoEtiquetaDoZpl(string zpl, string marketplace)
        {
            if (string.IsNullOrWhiteSpace(zpl))
                return "";

            string texto = _etiquetaService.DecodificarHexAmazon(zpl).ToUpperInvariant();

            if ((marketplace ?? "").Trim().Equals("Amazon", StringComparison.OrdinalIgnoreCase))
            {
                string textoAmazon = (texto + "\n" + _etiquetaService.ExtrairTextosFdDoZpl(zpl) + "\n" + zpl).ToUpperInvariant();

                var amazon = Regex.Match(textoAmazon, @"AMZB[A-Z0-9]+", RegexOptions.IgnoreCase);
                if (amazon.Success)
                    return amazon.Value.Trim().ToUpperInvariant();

                amazon = Regex.Match(textoAmazon, @"TBR[A-Z0-9]+", RegexOptions.IgnoreCase);
                if (amazon.Success)
                    return amazon.Value.Trim().ToUpperInvariant();

                string limpo = Regex.Replace(textoAmazon, @"[^A-Z0-9]", "");
                amazon = Regex.Match(limpo, @"AMZB[A-Z0-9]+", RegexOptions.IgnoreCase);
                if (amazon.Success)
                    return amazon.Value.Trim().ToUpperInvariant();
            }

            if ((marketplace ?? "").Trim().Equals("Shopee", StringComparison.OrdinalIgnoreCase))
            {
                var shopee = Regex.Match(texto, @"BR[A-Z0-9]{13}", RegexOptions.IgnoreCase);
                if (shopee.Success)
                    return shopee.Value;
            }



            if ((marketplace ?? "").Trim().Equals("Mercado Livre", StringComparison.OrdinalIgnoreCase))
            {
                var barcodeTexto = ExtrairBarcodeDoZpl(zpl);
                if (!string.IsNullOrWhiteSpace(barcodeTexto))
                {
                    string numeros = Regex.Replace(barcodeTexto, @"\D", "");
                    if (!string.IsNullOrWhiteSpace(numeros))
                        return numeros;
                }
            }

            try
            {
                using var bitmap = _leituraCodigoService.GerarBitmapViaLabelary(zpl);
                string lido = _leituraCodigoService.LerCodigoDaImagem(bitmap);

                if (!string.IsNullOrWhiteSpace(lido))
                    return lido.Trim().ToUpperInvariant();
            }
            catch
            {
            }

            return "";
        }

        private string NormalizarCodigoLido(string codigo, string marketplace)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return "";

            codigo = codigo.Trim().ToUpperInvariant();

            if ((marketplace ?? "").Trim().Equals("Shopee", StringComparison.OrdinalIgnoreCase))
            {
                var matchShopee = Regex.Match(codigo, @"BR[A-Z0-9]{13}", RegexOptions.IgnoreCase);
                if (matchShopee.Success)
                    return matchShopee.Value;
            }

            if ((marketplace ?? "").Trim().Equals("Amazon", StringComparison.OrdinalIgnoreCase))
            {
                var matchAmazon = Regex.Match(codigo, @"TBR[A-Z0-9]+", RegexOptions.IgnoreCase);

                if (matchAmazon.Success)
                    return matchAmazon.Value.Trim().ToUpperInvariant();
            }


            if ((marketplace ?? "").Trim().Equals("Mercado Livre", StringComparison.OrdinalIgnoreCase))
            {
                string numeros = Regex.Replace(codigo, @"\D", "");
                if (!string.IsNullOrWhiteSpace(numeros))
                    return numeros;
            }

            return Regex.Replace(codigo, @"[^A-Z0-9]", "");
        }


        private string ExtrairBarcodeDoZpl(string zpl)
        {
            if (string.IsNullOrWhiteSpace(zpl))
                return "";

            var regex = new Regex(@"\^(?:BC|B3|B7|BX|BQN)[^^]*\^FD([^\\^]+)\^FS", RegexOptions.IgnoreCase);
            var matches = regex.Matches(zpl);

            foreach (Match match in matches)
            {
                if (match.Success && match.Groups.Count > 1)
                {
                    string valor = match.Groups[1].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(valor) && valor.Length >= 8)
                        return valor;
                }
            }

            return "";
        }


        private bool ValidarEansAntesDaImpressao(PedidoConferencia pedido)
        {
            if (pedido == null)
                return false;

            var itensParaValidar = _validacaoEanService.ObterItensQuePrecisamValidar(pedido.JsonItens);

            if (itensParaValidar.Count == 0)
                return true;

            using var frm = new FrmValidacaoEan(
                itensParaValidar.Cast<object>().ToList(),
                pedido.NumeroPedidoCliente,
                pedido.NomeCliente
            );

            return frm.ShowDialog() == DialogResult.OK;
        }

        private class DadosDestinatarioShopee
        {
            public string Documento { get; set; } = "";
            public string Cep { get; set; } = "";
            public string Logradouro { get; set; } = "";
            public string Numero { get; set; } = "";
            public string Nome { get; set; } = "";
        }

        private void btnSalvarPedido_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Selecione o ZIP baixado do Omie";
            ofd.Filter = "Arquivo ZIP (*.zip)|*.zip";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                ImportarEtiquetasDoLote(ofd.FileName);

                var pedidos = _pedidoRepository.ObterTodos().ToList();

                _vinculacaoEtiquetaService.VincularEtiquetas(
                    _etiquetasLote,
                    _etiquetasShopeePdf,
                    pedidos
                );

                _pedidoRepository.SalvarOuAtualizarVarios(pedidos);

                CarregarPedidos(_pedidoSelecionado?.NumeroPedidoCliente);

                int totalMl = _etiquetasLote.Count(x => x.PlataformaDetectada == "Mercado Livre");
                int totalAmazon = _etiquetasLote.Count(x => x.PlataformaDetectada == "Amazon");
                int totalShopee = _etiquetasLote.Count(x => x.PlataformaDetectada == "Shopee");
                int totalDesconhecido = _etiquetasLote.Count(x => string.IsNullOrWhiteSpace(x.PlataformaDetectada));
                int totalShopeePdf = _etiquetasShopeePdf.Count;

                var vinculados = ContarPedidosVinculados();

                MessageBox.Show(
                    $"Lote importado com sucesso!\n\n" +
                    $"ETIQUETAS IMPORTADAS NO LOTE:\n" +
                    $"Total de etiquetas válidas: {_etiquetasLote.Count}\n" +
                    $"Mercado Livre: {totalMl}\n" +
                    $"Amazon: {totalAmazon}\n" +
                    $"Shopee (TXT/ZPL): {totalShopee}\n" +
                    $"Shopee (PDF): {totalShopeePdf}\n" +
                    $"Não identificadas: {totalDesconhecido}\n\n" +
                    $"PEDIDOS REALMENTE VINCULADOS:\n" +
                    $"Mercado Livre: {vinculados.ml}\n" +
                    $"Amazon: {vinculados.amazon}\n" +
                    $"Shopee: {vinculados.shopee}\n" +
                    $"Sem etiqueta: {vinculados.semEtiqueta}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao importar lote de etiquetas: " + ex.Message);
            }
        }

        private void ImportarEtiquetasDoLote(string caminhoZip)
        {
            _etiquetasLote.Clear();
            _etiquetasShopeePdf.Clear();
            _cachePedidoShopeeOcr.Clear();
            _ultimoArquivoZipImportado = caminhoZip;
            _nomePdfShopeeNoZip = "";

            using (ZipArchive zip = ZipFile.OpenRead(caminhoZip))
            {
                var etiquetasShopee = _shopeePdfService.CarregarEtiquetasDoZip(zip, out string nomePdfNoZip);

                _etiquetasShopeePdf.AddRange(etiquetasShopee);
                _nomePdfShopeeNoZip = nomePdfNoZip;
            }

            var etiquetas = _etiquetaService.ImportarEtiquetas(caminhoZip);

            _etiquetasLote.AddRange(etiquetas);

            if (_etiquetasLote.Count == 0)
                throw new Exception("Nenhuma etiqueta válida foi encontrada dentro do arquivo TXT.");
        }



        private string MostrarTextosFdShopeeDoLote()
        {
            var etiquetasShopee = _etiquetasLote
                .Where(e => string.Equals(e.PlataformaDetectada, "Shopee", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.OrdemNoArquivo)
                .Take(3)
                .ToList();

            if (etiquetasShopee.Count == 0)
                return "Nenhuma etiqueta Shopee encontrada no lote.";

            var sb = new StringBuilder();

            foreach (var etiqueta in etiquetasShopee)
            {
                sb.AppendLine("======================================");
                sb.AppendLine("ORDEM NO ARQUIVO: " + etiqueta.OrdemNoArquivo);
                sb.AppendLine("PLATAFORMA: " + etiqueta.PlataformaDetectada);
                sb.AppendLine("TEXTO EXTRAÍDO DOS ^FD:");
                sb.AppendLine(_etiquetaService.ExtrairTextosFdDoZpl(etiqueta.ConteudoZpl));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private void ImprimirPedidoSelecionado()
        {
            if (_pedidoSelecionado == null)
            {
                MessageBox.Show("Selecione um pedido.");
                return;
            }

            if (!ValidarEansAntesDaImpressao(_pedidoSelecionado))
            {
                MessageBox.Show("Impressão cancelada. Os EANs do pedido não foram conferidos.");
                return;
            }

            if (_etiquetasLote.Count == 0)
            {
                MessageBox.Show("Importe primeiro o lote de etiquetas.");
                return;
            }

            string zplEncontrado = _pedidoSelecionado.EtiquetaMarketplaceZpl;

            if (string.IsNullOrWhiteSpace(zplEncontrado))
            {
                MessageBox.Show(
                    "Não encontrei a etiqueta deste pedido dentro do lote importado.\n\n" +
                    "Confira se esse pedido realmente está no ZIP carregado.");
                return;
            }

            string numeroPedidoAtual = _pedidoSelecionado.NumeroPedidoCliente ?? "";

            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                _impressaoService.ImprimirPedido(
                    _pedidoSelecionado,
                    _ultimoArquivoZipImportado,
                    _nomePdfShopeeNoZip
                );

                _pedidoSelecionado.Impresso = true;

                _pedidoRepository.SalvarOuAtualizar(_pedidoSelecionado);

                CarregarPedidos(numeroPedidoAtual);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao imprimir: " + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnImprimirEtiqueta_Click(object sender, EventArgs e)
        {
            if (_pedidoSelecionado == null)
            {
                MessageBox.Show("Selecione um pedido.");
                return;
            }

            if (!ValidarEansAntesDaImpressao(_pedidoSelecionado))
            {
                MessageBox.Show("Impressão cancelada. Os EANs do pedido não foram conferidos.");
                return;
            }

            if (_etiquetasLote.Count == 0)
            {
                MessageBox.Show("Importe primeiro o lote de etiquetas.");
                return;
            }

            string zplEncontrado = _pedidoSelecionado.EtiquetaMarketplaceZpl;

            if (string.IsNullOrWhiteSpace(zplEncontrado))
            {
                MessageBox.Show(
                    "Não encontrei a etiqueta deste pedido dentro do lote importado.\n\n" +
                    "Confira se esse pedido realmente está no ZIP carregado.");
                return;
            }

            string numeroPedidoAtual = _pedidoSelecionado.NumeroPedidoCliente ?? "";

            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                _impressaoService.ImprimirPedido(
                    _pedidoSelecionado,
                    _ultimoArquivoZipImportado,
                    _nomePdfShopeeNoZip
                );

                _pedidoSelecionado.Impresso = true;

                _pedidoRepository.SalvarOuAtualizar(_pedidoSelecionado);

                CarregarPedidos(numeroPedidoAtual);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao imprimir: " + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void CarregarPedidos(string numeroPedidoParaRestaurar = null)
        {
            _carregandoPedidos = true;

            try
            {
                var lista = _pedidoRepository.ObterTodos()
    .Where(PedidoEhDeMarketplaceValido)
    .OrderBy(p => MarketplaceHelper.ObterOrdemMarketplace(p.Marketplace))
                    .ThenBy(p => (p.NumeroPedidoCliente ?? "").Trim())
                    .ThenBy(p => (p.NomeCliente ?? "").Trim())
                    .ToList();

                dgvPedidos.DataSource = null;
                dgvPedidos.Columns.Clear();
                dgvPedidos.AutoGenerateColumns = false;

                dgvPedidos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvPedidos.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                dgvPedidos.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
                dgvPedidos.RowTemplate.Height = 28;

                dgvPedidos.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "CodigoEtiqueta",
                    DataPropertyName = "CodigoEtiqueta",
                    HeaderText = "Código Etiqueta",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                });

                dgvPedidos.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "OK",
                    HeaderText = "OK",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                    Width = 45
                });

                dgvPedidos.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "NumeroPedidoCliente",
                    DataPropertyName = "NumeroPedidoCliente",
                    HeaderText = "Pedido Cliente",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                });

                dgvPedidos.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "NomeCliente",
                    DataPropertyName = "NomeCliente",
                    HeaderText = "Cliente",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                });

                dgvPedidos.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Marketplace",
                    DataPropertyName = "Marketplace",
                    HeaderText = "Marketplace",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                });

                dgvPedidos.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    DataPropertyName = "Status",
                    HeaderText = "Status",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                });

                dgvPedidos.DataSource = lista;

                dgvPedidos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvPedidos.MultiSelect = false;
                dgvPedidos.ReadOnly = true;
                dgvPedidos.AllowUserToAddRows = false;
                dgvPedidos.AllowUserToDeleteRows = false;

                FormatarColunaImpresso();
                FormatarColunaMarketplace();
                FormatarLinhasSemEtiqueta();

                if (!string.IsNullOrWhiteSpace(numeroPedidoParaRestaurar))
                    RestaurarSelecaoPedido(numeroPedidoParaRestaurar);
            }
            finally
            {
                _carregandoPedidos = false;
            }
        }

        private void FormatarColunaImpresso()
        {
            if (dgvPedidos.Rows.Count == 0 || dgvPedidos.Columns["OK"] == null)
                return;

            foreach (DataGridViewRow row in dgvPedidos.Rows)
            {
                if (row.DataBoundItem is PedidoConferencia pedido)
                {
                    row.Cells["OK"].Value = pedido.Impresso ? "✓" : "";
                }
            }
        }

        private void FormatarColunaMarketplace()
        {
            if (dgvPedidos.Rows.Count == 0 || dgvPedidos.Columns["Marketplace"] == null)
                return;

            foreach (DataGridViewRow row in dgvPedidos.Rows)
            {
                if (row.DataBoundItem is not PedidoConferencia pedido)
                    continue;

                string mp = MarketplaceHelper.NormalizarMarketplace(pedido.Marketplace);

                var cell = row.Cells["Marketplace"];

                cell.Style.ForeColor = Color.Black;
                cell.Style.SelectionForeColor = Color.Black;

                if (mp == "AMAZON")
                {
                    cell.Style.BackColor = Color.Moccasin;
                    cell.Style.SelectionBackColor = Color.Moccasin;
                }
                else if (mp == "MERCADO LIVRE")
                {
                    cell.Style.BackColor = Color.LightCyan;
                    cell.Style.SelectionBackColor = Color.LightCyan;
                }
                else if (mp == "SHOPEE")
                {
                    cell.Style.BackColor = Color.MistyRose;
                    cell.Style.SelectionBackColor = Color.MistyRose;
                }
                else
                {
                    cell.Style.BackColor = Color.White;
                    cell.Style.SelectionBackColor = Color.White;
                }
            }
        }

        private void FormatarLinhasSemEtiqueta()
        {
            if (dgvPedidos.Rows.Count == 0)
                return;

            foreach (DataGridViewRow row in dgvPedidos.Rows)
            {
                if (row.DataBoundItem is not PedidoConferencia pedido)
                    continue;

                bool semEtiqueta = string.IsNullOrWhiteSpace((pedido.EtiquetaMarketplaceZpl ?? "").Trim()) ||
                                   string.IsNullOrWhiteSpace((pedido.CodigoEtiqueta ?? "").Trim()) ||
                                   string.Equals((pedido.Status ?? "").Trim(), "Sem etiqueta", StringComparison.OrdinalIgnoreCase);

                if (semEtiqueta)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 230);
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 236, 179);
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    row.DefaultCellStyle.SelectionForeColor = Color.Black;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    row.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
                }
            }
        }

        private void RestaurarSelecaoPedido(string numeroPedidoCliente)
        {
            if (string.IsNullOrWhiteSpace(numeroPedidoCliente) || dgvPedidos.Rows.Count == 0)
                return;

            foreach (DataGridViewRow row in dgvPedidos.Rows)
            {
                if (row.DataBoundItem is not PedidoConferencia pedido)
                    continue;

                if (string.Equals(
                        (pedido.NumeroPedidoCliente ?? "").Trim(),
                        numeroPedidoCliente.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    row.Selected = true;
                    dgvPedidos.CurrentCell = row.Cells["NumeroPedidoCliente"];

                    if (row.Index >= 0)
                        dgvPedidos.FirstDisplayedScrollingRowIndex = Math.Max(0, row.Index - 2);

                    _pedidoSelecionado = pedido;
                    _jsonPedidoSelecionado = pedido.JsonItens ?? "[]";

                    txtCliente.Text = pedido.NomeCliente;
                    txtPedidoCliente.Text = pedido.NumeroPedidoCliente;
                    txtMarketplace.Text = pedido.Marketplace;
                    txtCodigoEtiqueta.Text = pedido.CodigoEtiqueta ?? "";

                    _ = CarregarItensDoPedidoAsync(pedido.JsonItens);
                    break;
                }
            }
        }

        private async void dgvPedidos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_carregandoPedidos)
                return;

            if (e.RowIndex < 0)
                return;

            var pedido = dgvPedidos.Rows[e.RowIndex].DataBoundItem as PedidoConferencia;
            if (pedido == null)
                return;

            _pedidoSelecionado = pedido;
            _jsonPedidoSelecionado = pedido.JsonItens ?? "[]";


            txtCliente.Text = pedido.NomeCliente;
            txtPedidoCliente.Text = pedido.NumeroPedidoCliente;
            txtMarketplace.Text = pedido.Marketplace;
            txtCodigoEtiqueta.Text = pedido.CodigoEtiqueta ?? "";

            await CarregarItensDoPedidoAsync(pedido.JsonItens);

        }

        private async Task CarregarItensDoPedidoAsync(string jsonPedido)
        {
            if (string.IsNullOrWhiteSpace(jsonPedido))
            {
                dgvItensPedido.DataSource = null;
                return;
            }

            try
            {
                dgvItensPedido.DataSource = null;

                var itens = await Task.Run(() => _pedidoItemService.MontarItensDoPedido(jsonPedido));

                dgvItensPedido.DataSource = itens;

                if (dgvItensPedido.Columns["Sku"] != null)
                {
                    dgvItensPedido.Columns["Sku"].DisplayIndex = 0;
                    dgvItensPedido.Columns["Sku"].HeaderText = "SKU";
                    dgvItensPedido.Columns["Sku"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }

                if (dgvItensPedido.Columns["Produto"] != null)
                {
                    dgvItensPedido.Columns["Produto"].DisplayIndex = 1;
                    dgvItensPedido.Columns["Produto"].HeaderText = "Produto";
                    dgvItensPedido.Columns["Produto"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }

                if (dgvItensPedido.Columns["Quantidade"] != null)
                {
                    dgvItensPedido.Columns["Quantidade"].DisplayIndex = 2;
                    dgvItensPedido.Columns["Quantidade"].HeaderText = "Qtd";
                    dgvItensPedido.Columns["Quantidade"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }

                if (dgvItensPedido.Columns["Ean"] != null)
                {
                    dgvItensPedido.Columns["Ean"].DisplayIndex = 3;
                    dgvItensPedido.Columns["Ean"].HeaderText = "EAN";
                    dgvItensPedido.Columns["Ean"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }

                dgvItensPedido.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvItensPedido.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                dgvItensPedido.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                dgvItensPedido.ReadOnly = true;
                dgvItensPedido.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvItensPedido.MultiSelect = false;
            }
            catch
            {
                dgvItensPedido.DataSource = null;
            }
        }

        private bool PedidoEhDeMarketplaceValido(PedidoConferencia pedido)
        {
            if (pedido == null)
                return false;

            string marketplace = MarketplaceHelper.NormalizarMarketplace(pedido.Marketplace);

            return marketplace == "AMAZON" ||
                   marketplace == "MERCADO LIVRE" ||
                   marketplace == "SHOPEE";
        }

        private async void btnBuscarPedidos_Click(object sender, EventArgs e)
        {
            btnBuscarPedidos.Enabled = false;

            try
            {
                DateTime dataInicial = dtpDataInicial.Value.Date;
                DateTime dataFinal = dtpDataFinal.Value.Date;

                if (dataInicial > dataFinal)
                {
                    MessageBox.Show("A data inicial não pode ser maior que a data final.");
                    return;
                }

                var pedidosImportados = await _pedidoOmieService.BuscarPedidosAsync(dataInicial, dataFinal);

                _pedidoRepository.Limpar();

                _pedidoRepository.SalvarOuAtualizarVarios(pedidosImportados);

                _pedidoSelecionado = null;
                _jsonPedidoSelecionado = "[]";
                txtCliente.Text = "";
                txtPedidoCliente.Text = "";
                txtMarketplace.Text = "";
                txtCodigoEtiqueta.Text = "";
                dgvItensPedido.DataSource = null;

                CarregarPedidos();

                MessageBox.Show(
                    $"Busca concluída.\n" +
                    $"Período: {dataInicial:dd/MM/yyyy} até {dataFinal:dd/MM/yyyy}\n" +
                    $"Pedidos encontrados: {pedidosImportados.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao buscar pedidos: " + ex.Message);
            }
            finally
            {
                btnBuscarPedidos.Enabled = true;
            }
        }

        private async void btnAtualizarPedidos_Click(object sender, EventArgs e)
        {
            try
            {
                btnAtualizarPedidos.Enabled = false;

                var dataInicial = dtpDataInicial.Value.Date;
                var dataFinal = dtpDataFinal.Value.Date;

                var pedidosOmie = await _pedidoOmieService.BuscarPedidosAsync(dataInicial, dataFinal);

                int novos = 0;
                int atualizados = 0;

                foreach (var pedido in pedidosOmie)
                {
                    var existente = _pedidoRepository.ObterTodos().FirstOrDefault(p =>
                        string.Equals(
                            (p.NumeroPedidoCliente ?? "").Trim(),
                            (pedido.NumeroPedidoCliente ?? "").Trim(),
                            StringComparison.OrdinalIgnoreCase));

                    if (existente == null)
                        novos++;
                    else
                        atualizados++;

                    _pedidoRepository.SalvarOuAtualizarPreservandoStatus(pedido);
                }

                CarregarPedidos(); // atualiza a grid

                MessageBox.Show(
                    $"Atualização concluída!\n\nNovos pedidos: {novos}\nAtualizados: {atualizados}",
                    "Atualizar pedidos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar pedidos:\n" + ex.Message);
            }
            finally
            {
                btnAtualizarPedidos.Enabled = true;
            }
        }
        private void btnGerarEtiqueta_Click(object sender, EventArgs e)
        {
            using var frm = new FrmConferencia();
            frm.ShowDialog();
        }

        private void btnImprimirPorProduto_Click(object sender, EventArgs e)
        {
            using var frm = new FrmBuscarPedidoPorProduto();

            if (frm.ShowDialog() != DialogResult.OK)
                return;

            var pedido = frm.PedidoSelecionado;

            if (pedido == null)
                return;

            _pedidoSelecionado = pedido;

            ImprimirPedidoSelecionado();
        }
    }
}

