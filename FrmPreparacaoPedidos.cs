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
    public enum OrigemSolicitacaoImpressao
    {
        Botao,
        F2,
        Enter,
        F4,
        Reimpressao
    }

    public partial class FrmPreparacaoPedidos : Form
    {


        private string _jsonPedidoSelecionado = "[]";
        private PedidoConferencia _pedidoSelecionado = null;
        private readonly List<EtiquetaMarketplaceLote> _etiquetasLote = new List<EtiquetaMarketplaceLote>();
        private bool _carregandoPedidos = false;
        private bool _suprimindoEventoData = false;
        private readonly Dictionary<string, string> _cachePedidoShopeeOcr = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<EtiquetaShopeePdf> _etiquetasShopeePdf = new List<EtiquetaShopeePdf>();
        private readonly PedidoItemService _pedidoItemService = new PedidoItemService();
        private readonly EtiquetaService _etiquetaService = new EtiquetaService();
        internal ImpressaoService ServicoImpressao = new ImpressaoService();
        private readonly PedidoOmieService _pedidoOmieService = new PedidoOmieService();
        private readonly ShopeePdfService _shopeePdfService = new ShopeePdfService();
        private readonly LeituraCodigoService _leituraCodigoService = new LeituraCodigoService();
        private readonly ValidacaoEanService _validacaoEanService = new ValidacaoEanService();
        private readonly VinculacaoEtiquetaService _vinculacaoEtiquetaService = new VinculacaoEtiquetaService();
        private readonly IPedidoRepository _pedidoRepository = new PedidoRepositorySqlite();
        internal Action<string, string, MessageBoxButtons, MessageBoxIcon> ExibirMensagem = (msg, title, btns, icon) => MessageBox.Show(msg, title, btns, icon);
        private readonly ValidacaoPreImpressaoService _validacaoPreImpressaoService;
        private readonly PedidoProdutoBuscaService _pedidoProdutoBuscaService = new PedidoProdutoBuscaService();
        private readonly System.Threading.SemaphoreSlim _controleImpressao = new System.Threading.SemaphoreSlim(1, 1);
        // private string _caminhoPdfShopee = ""; (removido)
        // private string _ultimoArquivoZipImportado = ""; (removido)
        private System.Windows.Forms.Timer _timerAtualizacaoPedidos;
        private bool _atualizandoPedidos = false;
        private readonly MercadoLivrePdfService _mercadoLivrePdfService =
    new MercadoLivrePdfService();

        private readonly List<EtiquetaMercadoLivrePdf> _etiquetasMercadoLivrePdf =
            new List<EtiquetaMercadoLivrePdf>();

        // private string _caminhoPdfMercadoLivre = ""; (removido)


        public FrmPreparacaoPedidos()
        {
            InitializeComponent();
            _validacaoPreImpressaoService = new ValidacaoPreImpressaoService(_pedidoRepository);

            dtpDataInicial.Value = DateTime.Today;

            KeyPreview = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            btnSalvarPedido.Text = "Importar Etiquetas do Lote";
            btnGerarEtiqueta.Text = "Conferir";

            _suprimindoEventoData = true;
            dtpDataInicial.Value = DateTime.Today;
            dtpDataFinal.Value = DateTime.Today;
            _suprimindoEventoData = false;

            ConfigurarGrids();
            CarregarPedidos();

            ConfigurarAtualizacaoAutomatica();
        }


        private void ConfigurarAtualizacaoAutomatica()
        {
            _timerAtualizacaoPedidos = new System.Windows.Forms.Timer();
            _timerAtualizacaoPedidos.Interval = 15 * 60 * 1000; // 15 minutos
            _timerAtualizacaoPedidos.Tick += async (s, e) =>
            {
                await AtualizarPedidosDoOmieAsync(exibirMensagem: false);
            };

            _timerAtualizacaoPedidos.Start();
        }

        /// <summary>
        /// Disparado quando o usuÃ¡rio altera DataInicial ou DataFinal.
        /// Recarrega a grade sem buscar novamente no Omie.
        /// </summary>
        private void dtpData_ValueChanged(object sender, EventArgs e)
        {
            if (_suprimindoEventoData || _carregandoPedidos)
                return;

            CarregarPedidos(_pedidoSelecionado?.NumeroPedidoCliente);
        }



        private string NormalizarCodigoPedidoShopee(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            string valor = texto.ToUpperInvariant();

            // mantÃ©m sÃ³ letras e nÃºmeros
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

        private string SomenteNumeros(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            return Regex.Replace(valor, @"\D", string.Empty);
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

            // CorreÃ§Ãµes comuns do OCR
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

            // ðŸ”¥ NOVO: compara por DISTÃ‚NCIA (tipo Levenshtein simplificado)
            int diferencas = 0;
            int tamanho = Math.Min(omie.Length, ocr.Length);

            for (int i = 0; i < tamanho; i++)
            {
                if (omie[i] != ocr[i])
                    diferencas++;
            }

            // penaliza diferenÃ§a de tamanho tambÃ©m
            diferencas += Math.Abs(omie.Length - ocr.Length);

            // ðŸ‘‰ regra mÃ¡gica:
            // aceita atÃ© 3 erros em 14 caracteres
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
            dgvPedidos.SelectionChanged += dgvPedidos_SelectionChanged;
            dgvPedidos.KeyDown += dgvPedidos_KeyDown;
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
                        Motivo = "CÃ³digo do cliente encontrado na etiqueta"
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
                        Motivo = $"Shopee vinculada pelo PDF. Pedido={etiquetaPdf.PedidoShopee}, Rastreio={etiquetaPdf.CodigoRastreio}, PÃ¡gina={etiquetaPdf.Pagina}"
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

                var dataInicio = dtpDataInicial.Value.Date;
                var dataFim = dtpDataFinal.Value.Date.AddDays(1);

                foreach (var outroPedido in _pedidoRepository.ObterPorPeriodo(dataInicio, dataFim))
                {
                    if (ReferenceEquals(outroPedido, pedido) || outroPedido.Id == pedido.Id)
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
            // Ã s vezes o pedido vem com prefixo e a etiqueta sem ele.
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

        private void btnAdministracao_Click(object sender, EventArgs e)
        {
            if (!FrmSenhaAdministrativa.SolicitarAutorizacao(this))
            {
                MessageBox.Show("Acesso negado.", "Segurança", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var frm = new FrmAdministracao();
            frm.ShowDialog();
            CarregarPedidos();
        }

        private void btnSalvarPedido_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog();

            ofd.Title = "Selecione o ZIP do Omie ou o PDF do Mercado Livre";
            ofd.Filter =
                "Arquivos de etiquetas (*.zip;*.pdf)|*.zip;*.pdf|" +
                "Arquivo ZIP (*.zip)|*.zip|" +
                "PDF Mercado Livre (*.pdf)|*.pdf";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                string extensao = Path.GetExtension(ofd.FileName);

                if (extensao.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ImportarPdfMercadoLivre(ofd.FileName);
                    return;
                }

                ImportarEtiquetasDoLote(ofd.FileName);

                var pedidos = _pedidoRepository.ObterTodos().ToList();

                _vinculacaoEtiquetaService.VincularEtiquetas(
                    _etiquetasLote,
                    _etiquetasShopeePdf,
                    pedidos,
                    ofd.FileName
                );

                _pedidoRepository.SalvarOuAtualizarVarios(pedidos);

                CarregarPedidos(_pedidoSelecionado?.NumeroPedidoCliente);

                int totalMl = _etiquetasLote.Count(x =>
                    x.PlataformaDetectada == "Mercado Livre");

                int totalAmazon = _etiquetasLote.Count(x =>
                    x.PlataformaDetectada == "Amazon");

                int totalShopee = _etiquetasLote.Count(x =>
                    x.PlataformaDetectada == "Shopee");

                int totalShopeePdf = _etiquetasShopeePdf.Count;

                var vinculados = ContarPedidosVinculados();

                MessageBox.Show(
                    $"Lote importado com sucesso!\n\n" +
                    $"Mercado Livre ZPL: {totalMl}\n" +
                    $"Amazon ZPL: {totalAmazon}\n" +
                    $"Shopee ZPL: {totalShopee}\n" +
                    $"Shopee PDF: {totalShopeePdf}\n\n" +
                    $"Pedidos vinculados:\n" +
                    $"Mercado Livre: {vinculados.ml}\n" +
                    $"Amazon: {vinculados.amazon}\n" +
                    $"Shopee: {vinculados.shopee}\n" +
                    $"Sem etiqueta: {vinculados.semEtiqueta}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao importar etiquetas: " + ex.Message,
                    "Importar etiquetas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ImportarPdfMercadoLivre(string caminhoPdf)
        {
            try
            {
                _etiquetasMercadoLivrePdf.Clear();
                // _caminhoPdfMercadoLivre = caminhoPdf;

                var etiquetas =
                    _mercadoLivrePdfService.CarregarEtiquetas(caminhoPdf);

                _etiquetasMercadoLivrePdf.AddRange(etiquetas);

                if (_etiquetasMercadoLivrePdf.Count == 0)
                {
                    MessageBox.Show(
                        "Nenhuma etiqueta vÃ¡lida do Mercado Livre foi encontrada.",
                        "PDF Mercado Livre",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                var pedidos =
                    _pedidoRepository.ObterTodos().ToList();

                var pedidosMercadoLivre = pedidos
                    .Where(p =>
                        MarketplaceHelper.NormalizarMarketplace(
                            p.Marketplace) == "MERCADO LIVRE")
                    .ToList();

                /*
                 * Ãndice por Venda.
                 */
                var etiquetasPorCodigo = _etiquetasMercadoLivrePdf
                    .Where(e => !string.IsNullOrWhiteSpace(e.CodigoEtiqueta) && SomenteNumeros(e.CodigoEtiqueta).Length == 11)
                    .GroupBy(e => SomenteNumeros(e.CodigoEtiqueta))
                    .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                var etiquetasPorVenda =
                    _etiquetasMercadoLivrePdf
                        .Where(e =>
                            SomenteNumeros(e.NumeroVenda).Length == 16)
                        .GroupBy(e =>
                            SomenteNumeros(e.NumeroVenda))
                        .ToDictionary(
                            grupo => grupo.Key,
                            grupo => grupo.First(),
                            StringComparer.OrdinalIgnoreCase);

                /*
                 * Ãndice por Pack ID.
                 */
                var etiquetasPorPackId =
                    _etiquetasMercadoLivrePdf
                        .Where(e =>
                            SomenteNumeros(e.PackId).Length == 16)
                        .GroupBy(e =>
                            SomenteNumeros(e.PackId))
                        .ToDictionary(
                            grupo => grupo.Key,
                            grupo => grupo.First(),
                            StringComparer.OrdinalIgnoreCase);

                int vinculadosPorVenda = 0;
                int vinculadosPorPackId = 0;
                int jaVinculados = 0;
                int semCodigo = 0;
                int semCorrespondencia = 0;
                int vinculadosPorCodigo = 0;

                foreach (var pedido in pedidosMercadoLivre)
                {
                    string referenciaAtual =
                        pedido.EtiquetaMarketplaceZpl ?? string.Empty;

                    string codigoAtual =
                        SomenteNumeros(pedido.CodigoEtiqueta);

                    bool temCodigoSalvo = codigoAtual.Length == 11;

                    // Se o pedido JÃ TEM cÃ³digo (11 dÃ­gitos), usamos ele para buscar a pÃ¡gina correta no PDF.
                    // Isso evita cruzamento de pÃ¡ginas por erro na extraÃ§Ã£o do NumeroVenda.
                    if (temCodigoSalvo)
                    {
                        if (etiquetasPorCodigo.TryGetValue(codigoAtual, out var listaPorCodigo))
                        {
                            if (listaPorCodigo.Count == 1)
                            {
                                var etiquetaUnica = listaPorCodigo[0];
                                bool paginaEmUso = pedidos.Any(p => p != pedido && p.Marketplace == pedido.Marketplace && p.PaginaPdf == etiquetaUnica.Pagina);
                                if (!paginaEmUso)
                                {
                                    pedido.CaminhoZipImportacao = caminhoPdf;
                                    pedido.NomePdfNoZip = "";
                                    pedido.PaginaPdf = etiquetaUnica.Pagina;
                                    pedido.EtiquetaMarketplaceZpl = $"PDF_MELI|{etiquetaUnica.Pagina}";
                                    pedido.Status = "Etiqueta vinculada";
                                    vinculadosPorCodigo++;
                                }
                            }
                        }
                        
                        jaVinculados++;
                        continue;
                    }

                    // LÃ³gica para pedidos SEM CodigoEtiqueta (novos ou importados do Omie sem ZPL)
                    string numeroPedido =
                        SomenteNumeros(pedido.NumeroPedidoCliente);

                    EtiquetaMercadoLivrePdf etiqueta = null;
                    bool encontrouPorVenda = false;
                    bool encontrouPorPackId = false;

                    /*
                     * Primeira tentativa:
                     * NÃºmero do pedido do Omie igual Ã  Venda.
                     */
                    if (numeroPedido.Length == 16 &&
                        etiquetasPorVenda.TryGetValue(
                            numeroPedido,
                            out EtiquetaMercadoLivrePdf etiquetaVenda))
                    {
                        etiqueta = etiquetaVenda;
                        encontrouPorVenda = true;
                    }

                    /*
                     * Segunda tentativa:
                     * NÃºmero do pedido do Omie igual ao Pack ID.
                     */
                    if (etiqueta == null &&
                        numeroPedido.Length == 16 &&
                        etiquetasPorPackId.TryGetValue(
                            numeroPedido,
                            out EtiquetaMercadoLivrePdf etiquetaPack))
                    {
                        etiqueta = etiquetaPack;
                        encontrouPorPackId = true;
                    }

                    if (etiqueta == null)
                    {
                        semCorrespondencia++;
                        continue;
                    }

                    pedido.EtiquetaMarketplaceZpl =
                        $"PDF_MELI|{etiqueta.Pagina}";
                    pedido.CaminhoZipImportacao = caminhoPdf;
                    pedido.NomePdfNoZip = "";
                    pedido.PaginaPdf = etiqueta.Pagina;

                    string codigoEncontrado =
                        SomenteNumeros(etiqueta.CodigoEtiqueta);

                    if (codigoEncontrado.Length != 11)
                    {
                        pedido.CodigoEtiqueta = string.Empty;

                        pedido.Status =
                            "PDF encontrado - cÃ³digo nÃ£o identificado";

                        semCodigo++;
                        continue;
                    }

                    pedido.CodigoEtiqueta =
                        codigoEncontrado;

                    pedido.Status =
                        "Etiqueta vinculada";

                    if (encontrouPorVenda)
                        vinculadosPorVenda++;
                    else if (encontrouPorPackId)
                        vinculadosPorPackId++;
                }

                _pedidoRepository
                    .SalvarOuAtualizarVarios(pedidos);

                CarregarPedidos(
                    _pedidoSelecionado?.NumeroPedidoCliente);

                int etiquetasComVenda =
                    _etiquetasMercadoLivrePdf.Count(e =>
                        SomenteNumeros(e.NumeroVenda).Length == 16);

                int etiquetasComPackId =
                    _etiquetasMercadoLivrePdf.Count(e =>
                        SomenteNumeros(e.PackId).Length == 16);

                int etiquetasComCodigo =
                    _etiquetasMercadoLivrePdf.Count(e =>
                        SomenteNumeros(e.CodigoEtiqueta).Length == 11);

                MessageBox.Show(
                    $"PDF do Mercado Livre importado!\n\n" +
                    $"PÃ¡ginas de etiqueta encontradas: " +
                    $"{_etiquetasMercadoLivrePdf.Count}\n" +
                    $"Etiquetas com Venda: {etiquetasComVenda}\n" +
                    $"Etiquetas com Pack ID: {etiquetasComPackId}\n" +
                    $"Etiquetas com cÃ³digo vÃ¡lido: {etiquetasComCodigo}\n\n" +
                    $"Vinculados pela Venda: {vinculadosPorVenda}\n" +
                    $"Vinculados pelo Pack ID: {vinculadosPorPackId}\n" +
                    $"JÃ¡ tinham etiqueta completa: {jaVinculados}\n" +
                    $"PDF encontrado, mas sem cÃ³digo: {semCodigo}\n" +
                    $"Pedidos sem correspondÃªncia: {semCorrespondencia}",
                    "PDF Mercado Livre",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao importar o PDF do Mercado Livre:\n\n" +
                    ex.Message,
                    "PDF Mercado Livre",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }



        private void ImportarEtiquetasDoLote(string caminhoZip)
        {
            _etiquetasLote.Clear();
            _etiquetasShopeePdf.Clear();
            _cachePedidoShopeeOcr.Clear();
            // _ultimoArquivoZipImportado = caminhoZip;

            using (ZipArchive zip = ZipFile.OpenRead(caminhoZip))
            {
                var etiquetasShopee = _shopeePdfService.CarregarEtiquetasDoZip(zip);

                _etiquetasShopeePdf.AddRange(etiquetasShopee);
            }

            var etiquetas = _etiquetaService.ImportarEtiquetas(caminhoZip);

            _etiquetasLote.AddRange(etiquetas);

            if (_etiquetasLote.Count == 0)
                throw new Exception("Nenhuma etiqueta vÃ¡lida foi encontrada dentro do arquivo TXT.");
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
                sb.AppendLine("TEXTO EXTRAÃDO DOS ^FD:");
                sb.AppendLine(_etiquetaService.ExtrairTextosFdDoZpl(etiqueta.ConteudoZpl));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        internal async Task ExecutarImpressaoSeguraAsync(PedidoConferencia pedidoInicial, OrigemSolicitacaoImpressao origem)
        {
            if (pedidoInicial == null || string.IsNullOrWhiteSpace(pedidoInicial.NumeroPedidoCliente))
            {
                ExibirMensagem("Nenhum pedido especificado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool acquired = await _controleImpressao.WaitAsync(0);
            if (!acquired)
            {
                ExibirMensagem("Uma impressÃ£o jÃ¡ estÃ¡ em andamento. Aguarde.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                btnImprimirEtiqueta.Enabled = false;
                Cursor = Cursors.WaitCursor;

                // 1. Recarregue o pedido pelo identificador persistente.
                // Se o Id estiver preenchido (caso tenha vindo do BD apÃ³s as atualizaÃ§Ãµes recentes), usamos ele, senÃ£o pelo nÃºmero.
                var pedidoAtual = pedidoInicial.Id > 0
                    ? _pedidoRepository.ObterPorId(pedidoInicial.Id)
                    : _pedidoRepository.ObterPorNumero(pedidoInicial.NumeroPedidoCliente);
                
                if (pedidoAtual == null)
                {
                    ExibirMensagem("Pedido não encontrado no banco de dados.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!ValidarEansAntesDaImpressao(pedidoAtual))
                {
                    ExibirMensagem("Impressão cancelada. Os EANs do pedido não foram conferidos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(pedidoAtual.EtiquetaMarketplaceZpl))
                {
                    ExibirMensagem("Importe primeiro o lote de etiquetas ou garanta que o pedido possui uma etiqueta vinculada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string motivoReimpressao = null;
                if (pedidoAtual.Impresso)
                {
                    if (!FrmSenhaAdministrativa.SolicitarAutorizacao(this))
                    {
                        ExibirMensagem("Impressão bloqueada (autorização negada ou cancelada).", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    motivoReimpressao = FrmMotivoReimpressao.SolicitarMotivo(this);
                    if (string.IsNullOrWhiteSpace(motivoReimpressao))
                    {
                        ExibirMensagem("Impressão bloqueada (motivo não informado).", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // 2. O snapshot deve copiar obrigatoriamente:
                var snapshot = new PedidoConferencia
                {
                    Id = pedidoAtual.Id,
                    NumeroPedidoCliente = pedidoAtual.NumeroPedidoCliente,
                    Marketplace = pedidoAtual.Marketplace,
                    CodigoEtiqueta = pedidoAtual.CodigoEtiqueta,
                    EtiquetaMarketplaceZpl = pedidoAtual.EtiquetaMarketplaceZpl,
                    CaminhoZipImportacao = pedidoAtual.CaminhoZipImportacao,
                    NomePdfNoZip = pedidoAtual.NomePdfNoZip,
                    PaginaPdf = pedidoAtual.PaginaPdf,
                    DataPrevisao = pedidoAtual.DataPrevisao,
                    Impresso = pedidoAtual.Impresso,
                    Status = pedidoAtual.Status,
                    DataCriacao = pedidoAtual.DataCriacao
                };

                // 3. ValidaÃ§Ã£o e ImpressÃ£o apenas com snapshot
                var validacao = _validacaoPreImpressaoService.ValidarAntesDaImpressao(snapshot);

                if (!validacao.Valido)
                {
                    ExibirMensagem($"IMPRESSÃƒO BLOQUEADA!\n\nMotivo: {validacao.MotivoBloqueio}\nDetalhes: {validacao.Mensagem}", "Falha de SeguranÃ§a", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var resultadoImpressao = ServicoImpressao.ImprimirPedido(
                    snapshot,
                    snapshot.CaminhoZipImportacao,
                    snapshot.CaminhoZipImportacao
                );

                if (resultadoImpressao.Sucesso && resultadoImpressao.Status == StatusResultadoImpressao.EnviadoParaFila)
                {
                    var pedidoOriginalParaAtualizar = _pedidoRepository.ObterPorId(snapshot.Id);
                    if (pedidoOriginalParaAtualizar != null)
                    {
                        if (pedidoOriginalParaAtualizar.Impresso)
                        {
                            pedidoOriginalParaAtualizar.DataReimpressao = DateTime.Now;
                            pedidoOriginalParaAtualizar.MotivoReimpressao = motivoReimpressao;
                            pedidoOriginalParaAtualizar.Conferido = false; // Reset de segurança
                            
                            var authService = new SistemaConferenciaPedidos.Services.AdminAuthService();
                            authService.RegistrarAcao("REIMPRESSAO_PEDIDO", pedidoOriginalParaAtualizar.NumeroPedidoCliente, $"Motivo: {motivoReimpressao}");
                        }
                        else
                        {
                            pedidoOriginalParaAtualizar.DataPrimeiraImpressao = DateTime.Now;
                        }
                        
                        pedidoOriginalParaAtualizar.Impresso = true;
                        _pedidoRepository.SalvarOuAtualizar(pedidoOriginalParaAtualizar);
                    }
                    
                    CarregarPedidos(snapshot.NumeroPedidoCliente);
                    SelecionarProximoPedidoNaoImpresso();
                }
                else if (resultadoImpressao.Status == StatusResultadoImpressao.EstadoDesconhecido)
                {
                    ExibirMensagem("Não foi possível confirmar se a etiqueta foi enviada. Verifique a fila de impressão antes de tentar novamente.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    ExibirMensagem("Falha na impressão: " + resultadoImpressao.Mensagem, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                ExibirMensagem("Erro inesperado ao imprimir: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnImprimirEtiqueta.Enabled = true;
                Cursor = Cursors.Default;
                _controleImpressao.Release();
            }
        }

        private async void btnImprimirEtiqueta_Click(object sender, EventArgs e)
        {
            if (_pedidoSelecionado == null)
            {
                MessageBox.Show("Selecione um pedido na grade.");
                return;
            }
            
            await ExecutarImpressaoSeguraAsync(_pedidoSelecionado, OrigemSolicitacaoImpressao.Botao);
        }

        private void CarregarPedidos(string numeroPedidoParaRestaurar = null)
        {
            _carregandoPedidos = true;

            try
            {
                DateTime inicio = dtpDataInicial.Value.Date;
                DateTime fimExclusivo = dtpDataFinal.Value.Date.AddDays(1);

                if (inicio > dtpDataFinal.Value.Date)
                    fimExclusivo = inicio.AddDays(1); // protege inversão silenciosa

                var lista = _pedidoRepository.ObterPorPeriodo(inicio, fimExclusivo)
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

                AtualizarResumoPreparacao(lista);

                if (!string.IsNullOrWhiteSpace(numeroPedidoParaRestaurar))
                    RestaurarSelecaoPedido(numeroPedidoParaRestaurar);
            }
            finally
            {
                _carregandoPedidos = false;
            }
        }

        public ResumoPreparacaoResult ResumoAtual { get; private set; } = new ResumoPreparacaoResult();

        public ResumoPreparacaoResult CalcularResumoPreparacao(IEnumerable<PedidoConferencia> pedidos)
        {
            if (pedidos == null)
            {
                return new ResumoPreparacaoResult(0, 0, 0, 0);
            }

            var pedidosValidos = pedidos
                .Where(p => p != null)
                .Where(p => !p.Oculto)
                .Where(p => !string.Equals((p.Status ?? "").Trim(), "Cancelado", StringComparison.OrdinalIgnoreCase))
                .Where(PedidoEhDeMarketplaceValido)
                .ToList();

            int total = pedidosValidos.Count;
            int preparados = pedidosValidos.Count(p => p.Impresso);
            int faltam = Math.Max(0, total - preparados);
            int percentual = total == 0 ? 0 : (int)Math.Round((double)preparados / total * 100);

            return new ResumoPreparacaoResult(total, preparados, faltam, percentual);
        }

        public ResumoPreparacaoResult AtualizarResumoPreparacao(IEnumerable<PedidoConferencia> pedidos = null)
        {
            if (pedidos == null)
            {
                if (dgvPedidos?.DataSource is IEnumerable<PedidoConferencia> listaGrid)
                    pedidos = listaGrid;
                else
                    pedidos = new List<PedidoConferencia>();
            }

            var resumo = CalcularResumoPreparacao(pedidos);
            ResumoAtual = resumo;

            if (lblResumoTotal != null)
                lblResumoTotal.Text = $"Total: {resumo.Total}";

            if (lblResumoPreparados != null)
                lblResumoPreparados.Text = $"Preparados: {resumo.Preparados}";

            if (lblResumoFaltam != null)
                lblResumoFaltam.Text = $"Faltam: {resumo.Faltam}";

            if (lblResumoPercentual != null)
                lblResumoPercentual.Text = $"Progresso: {resumo.Percentual}%";

            if (pbProgressoResumo != null)
            {
                int valProgress = Math.Min(100, Math.Max(0, resumo.Percentual));
                pbProgressoResumo.Value = valProgress;
            }

            return resumo;
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
        private async Task SelecionarPedidoAsync(PedidoConferencia pedido)
        {
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
        private async void dgvPedidos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (_carregandoPedidos)
                return;

            if (e.RowIndex < 0)
                return;

            if (dgvPedidos.Rows[e.RowIndex].DataBoundItem is not PedidoConferencia pedido)
                return;

            await SelecionarPedidoAsync(pedido);
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
            var confirmacao = MessageBox.Show(
                "Deseja procurar novamente os pedidos?",
                "Confirmar nova busca",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (confirmacao != DialogResult.Yes)
                return;

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

                var pedidosMarketplace = pedidosImportados
                    .Where(PedidoEhDeMarketplaceValido)
                    .ToList();

                var pedidosOutrosCanais = pedidosImportados
                    .Where(p => !PedidoEhDeMarketplaceValido(p))
                    .ToList();

                // Sincronização segura através de serviço dedicado
                var syncService = new SistemaConferenciaPedidos.Services.PedidoSincronizacaoService(_pedidoRepository);
                await syncService.SincronizarAsync(pedidosMarketplace);

                _pedidoSelecionado = null;
                _jsonPedidoSelecionado = "[]";
                txtCliente.Text = "";
                txtPedidoCliente.Text = "";
                txtMarketplace.Text = "";
                txtCodigoEtiqueta.Text = "";
                dgvItensPedido.DataSource = null;

                CarregarPedidos();

                MessageBox.Show(
                    $"Busca concluída.\n\n" +
                    $"Período: {dataInicial:dd/MM/yyyy} até {dataFinal:dd/MM/yyyy}\n\n" +
                    $"Pedidos para preparação:\n" +
                    $"Amazon / Shopee / Mercado Livre: {pedidosMarketplace.Count}\n\n" +
                    $"Pedidos de outros canais ignorados: {pedidosOutrosCanais.Count}",
                    "Buscar pedidos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
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


        private void btnExcluirPedido_Click(object sender, EventArgs e)
        {
            if (_pedidoSelecionado == null)
            {
                MessageBox.Show(
                    "Selecione um pedido para remover.",
                    "Remover pedido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string numeroPedido = _pedidoSelecionado.NumeroPedidoCliente ?? "";
            string cliente = _pedidoSelecionado.NomeCliente ?? "";

            var confirmacao = MessageBox.Show(
                $"Deseja realmente remover este pedido do painel de preparação?\n\n" +
                $"Pedido: {numeroPedido}\n" +
                $"Cliente: {cliente}\n\n" +
                "Ele também deixará de aparecer na conferência e não voltará nas próximas buscas.",
                "Confirmar remoção",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirmacao != DialogResult.Yes)
                return;

            if (!FrmSenhaAdministrativa.SolicitarAutorizacao(this))
                return;

            try
            {
                bool removido = _pedidoRepository.OcultarPedido(numeroPedido);

                if (!removido)
                {
                    MessageBox.Show(
                        "Não foi possível localizar o pedido para remoção.",
                        "Remover pedido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                _pedidoSelecionado = null;
                _jsonPedidoSelecionado = "[]";
                txtCliente.Clear();
                txtPedidoCliente.Clear();
                txtMarketplace.Clear();
                txtCodigoEtiqueta.Clear();
                dgvItensPedido.DataSource = null;

                CarregarPedidos();

                MessageBox.Show(
                    "Pedido removido do painel de preparação e da conferência.",
                    "Pedido removido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao remover pedido: " + ex.Message,
                    "Remover pedido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void btnAtualizarPedidos_Click(object sender, EventArgs e)
        {
            await AtualizarPedidosDoOmieAsync(exibirMensagem: true);
        }
        private void btnGerarEtiqueta_Click(object sender, EventArgs e)
        {
            using var frm = new FrmConferencia(dtpDataInicial.Value);
            frm.ShowDialog();
        }

        private async Task AtualizarPedidosDoOmieAsync(bool exibirMensagem)
        {
            if (_atualizandoPedidos)
                return;

            try
            {
                _atualizandoPedidos = true;

                if (exibirMensagem)
                    btnAtualizarPedidos.Enabled = false;

                var dataInicial = dtpDataInicial.Value.Date;
                var dataFinal = dtpDataFinal.Value.Date;

                var pedidosOmie = await _pedidoOmieService.BuscarPedidosAsync(dataInicial, dataFinal);

                var pedidosMarketplace = pedidosOmie
                    .Where(PedidoEhDeMarketplaceValido)
                    .ToList();

                var pedidosIgnorados = pedidosOmie
                    .Where(p => !PedidoEhDeMarketplaceValido(p))
                    .ToList();

                // Sincronização segura através de serviço dedicado (idempotente e imune a corrida)
                var syncService = new SistemaConferenciaPedidos.Services.PedidoSincronizacaoService(_pedidoRepository);
                await syncService.SincronizarAsync(pedidosMarketplace);

                CarregarPedidos(_pedidoSelecionado?.NumeroPedidoCliente);

                if (exibirMensagem)
                {
                    MessageBox.Show(
                        $"Atualização concluída!\n\n" +
                        $"Pedidos de marketplace processados: {pedidosMarketplace.Count}\n" +
                        $"Ignorados por não serem Amazon/Shopee/Mercado Livre: {pedidosIgnorados.Count}",
                        "Atualizar pedidos",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                if (exibirMensagem)
                    MessageBox.Show("Erro ao atualizar pedidos:\n" + ex.Message);
            }
            finally
            {
                _atualizandoPedidos = false;

                if (exibirMensagem)
                    btnAtualizarPedidos.Enabled = true;
            }
        }
        private void btnImprimirPorProduto_Click(object sender, EventArgs e)
        {
            using var frm = new FrmBuscarPedidoPorProduto(dtpDataInicial.Value.Date, dtpDataFinal.Value.Date, ImprimirPedidoEncontradoPorProduto);
            frm.ShowDialog();

            CarregarPedidos(_pedidoSelecionado?.NumeroPedidoCliente);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F2:

                    if (btnImprimirEtiqueta.Enabled)
                        btnImprimirEtiqueta.PerformClick();

                    return true;

                case Keys.F4:

                    if (btnImprimirPorProduto.Enabled)
                        btnImprimirPorProduto.PerformClick();

                    return true;

                case Keys.F5:

                    if (btnAtualizarPedidos.Enabled)
                        btnAtualizarPedidos.PerformClick();

                    return true;

                case Keys.F8:

                    if (btnGerarEtiqueta.Enabled)
                        btnGerarEtiqueta.PerformClick();

                    return true;

                case Keys.Delete:

                    if (btnExcluirPedido.Enabled)
                        btnExcluirPedido.PerformClick();

                    return true;

                case Keys.Escape:

                    dgvPedidos.ClearSelection();

                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
        private async void dgvPedidos_SelectionChanged(object sender, EventArgs e)
        {
            if (_carregandoPedidos)
                return;

            if (dgvPedidos.CurrentRow == null)
                return;

            if (dgvPedidos.CurrentRow.DataBoundItem is not PedidoConferencia pedido)
                return;

            await SelecionarPedidoAsync(pedido);
        }
        private async void dgvPedidos_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                if (_pedidoSelecionado != null)
                {
                    await ExecutarImpressaoSeguraAsync(_pedidoSelecionado, OrigemSolicitacaoImpressao.Enter);
                }
            }
        }
        private async void SelecionarProximoPedidoNaoImpresso()
        {
            if (dgvPedidos.Rows.Count == 0)
                return;

            foreach (DataGridViewRow row in dgvPedidos.Rows)
            {
                if (row.DataBoundItem is not PedidoConferencia pedido)
                    continue;

                if (pedido.Impresso)
                    continue;

                dgvPedidos.ClearSelection();

                row.Selected = true;
                dgvPedidos.CurrentCell = row.Cells[1];

                await SelecionarPedidoAsync(pedido);

                return;
            }
        }
        private async void ImprimirPedidoEncontradoPorProduto(PedidoConferencia pedido)
        {
            if (pedido == null)
                return;

            var dataInicial = dtpDataInicial.Value.Date;
            var dataFinal = dtpDataFinal.Value.Date.AddDays(1);
            var dataComparar = pedido.DataPrevisao ?? pedido.DataCriacao;

            if (dataComparar < dataInicial || dataComparar >= dataFinal)
            {
                MessageBox.Show("Este pedido não pertence à data selecionada.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _pedidoSelecionado = pedido;

            await ExecutarImpressaoSeguraAsync(pedido, OrigemSolicitacaoImpressao.F4);
        }

        private void btnValidarVinculos_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var dataFiltro = dtpDataInicial.Value.Date;
                var pedidosFiltro = _pedidoRepository.ObterPorPeriodo(dataFiltro, dataFiltro.AddDays(1))
                    .Where(p => p.Status != "Cancelado")
                    .ToList();

                int total = pedidosFiltro.Count;
                int validos = 0;
                int falhas = 0;
                var errosBuilder = new StringBuilder();

                foreach (var pedido in pedidosFiltro)
                {
                    var validacao = _validacaoPreImpressaoService.ValidarAntesDaImpressao(pedido);
                    if (validacao.Valido)
                    {
                        validos++;
                    }
                    else
                    {
                        falhas++;
                        errosBuilder.AppendLine($"[Nº {pedido.NumeroPedidoCliente} | {pedido.Marketplace}]");
                        errosBuilder.AppendLine($"   Motivo: {validacao.MotivoBloqueio}");
                        errosBuilder.AppendLine($"   Detalhe: {validacao.Mensagem}");
                        errosBuilder.AppendLine(new string('-', 50));
                    }
                }

                if (falhas == 0)
                {
                    MessageBox.Show($"Resumo do dia {dataFiltro:dd/MM/yyyy}:\n\nTotal: {total}\nVálidos: {validos}\n\nTodos os vínculos estão corretos!", "Validação de Vínculos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var msg = $"Resumo do dia {dataFiltro:dd/MM/yyyy}:\n\nTotal: {total}\nVálidos: {validos}\nInconsistentes: {falhas}\n\nDetalhes:\n{errosBuilder.ToString()}";
                    MessageBox.Show(msg, "Validação de Vínculos - INCONSISTÊNCIAS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao validar vínculos: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}

