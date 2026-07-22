using SistemaConferenciaPedidos.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using System.Windows.Forms;

namespace SistemaConferenciaPedidos.Services
{
    public class MercadoLivrePdfService
    {
        public List<EtiquetaMercadoLivrePdf> CarregarEtiquetas(
            string caminhoPdf)
        {
            ValidarArquivo(caminhoPdf);

            List<PaginaMercadoLivre> paginas =
                LerPaginas(caminhoPdf);

            /*
             * SEGUNDA PASSAGEM
             *
             * Monta:
             *
             * Pack ID -> Venda
             *
             * usando somente as páginas de identificação
             * de produtos.
             */
            Dictionary<string, string> mapaPackVenda =
                MontarMapaPackVenda(paginas);

            // Diagnóstico removido — era MessageBox.Show que bloqueava a thread

            /*
             * TERCEIRA PASSAGEM
             *
             * Cria as etiquetas prontas para serem vinculadas.
             */
            List<EtiquetaMercadoLivrePdf> etiquetas =
                MontarEtiquetas(paginas, mapaPackVenda);

            System.Diagnostics.Debug.WriteLine(
                $"MELI - páginas totais: {paginas.Count}");

            System.Diagnostics.Debug.WriteLine(
                $"MELI - páginas de etiquetas: {etiquetas.Count}");

            System.Diagnostics.Debug.WriteLine(
                $"MELI - pares Pack/Venda: {mapaPackVenda.Count}");

            System.Diagnostics.Debug.WriteLine(
                $"MELI - etiquetas com Venda: " +
                $"{etiquetas.Count(e => !string.IsNullOrWhiteSpace(e.NumeroVenda))}");

            System.Diagnostics.Debug.WriteLine(
                $"MELI - etiquetas com código: " +
                $"{etiquetas.Count(e => !string.IsNullOrWhiteSpace(e.CodigoEtiqueta))}");

            return etiquetas;
        }

        /*
         * PRIMEIRA PASSAGEM
         *
         * Lê todas as páginas sem tentar vincular nada.
         */
        private List<PaginaMercadoLivre> LerPaginas(
            string caminhoPdf)
        {
            var paginas = new List<PaginaMercadoLivre>();

            using var documento = PdfDocument.Open(caminhoPdf);

            foreach (var paginaPdf in documento.GetPages())
            {
                string textoOriginal =
                    paginaPdf.Text ?? string.Empty;

                string textoNormalizado =
                    NormalizarTexto(textoOriginal);

                paginas.Add(new PaginaMercadoLivre
                {
                    Numero = paginaPdf.Number,
                    TextoOriginal = textoOriginal,
                    TextoNormalizado = textoNormalizado,

                    PackId =
                        ExtrairPackId(textoNormalizado),

                    NumeroVenda =
                        ExtrairNumeroVenda(textoNormalizado),

                    CodigoEtiqueta =
                        ExtrairCodigoEtiqueta(textoNormalizado),

                    NomeCliente =
                        ExtrairNomeCliente(textoNormalizado),

                    EhPaginaResumo =
                        EhPaginaDeResumo(textoNormalizado)
                });
            }

            return paginas;
        }

        /*
         * Monta o mapa Pack ID -> Venda.
         *
         * É feito somente com páginas de identificação,
         * nunca com páginas de etiqueta.
         */
        private Dictionary<string, string> MontarMapaPackVenda(
    List<PaginaMercadoLivre> paginas)
        {
            var mapa = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            /*
             * Usa apenas páginas de resumo/identificação.
             */
            var paginasResumo = paginas
                .Where(p => p.EhPaginaResumo)
                .ToList();

            foreach (var pagina in paginasResumo)
            {
                string texto = pagina.TextoNormalizado;

                /*
                 * Localiza cada Pack ID no texto.
                 */
                MatchCollection packs = Regex.Matches(
                    texto,
                    @"PACK\s*ID\s*:\s*(\d{16})",
                    RegexOptions.IgnoreCase);

                foreach (Match packMatch in packs)
                {
                    string packId =
                        SomenteNumeros(packMatch.Groups[1].Value);

                    int inicioBusca =
                        packMatch.Index + packMatch.Length;

                    /*
                     * O fim do bloco é o próximo Pack ID,
                     * ou o fim da página.
                     */
                    Match proximoPack = Regex.Match(
                        texto.Substring(inicioBusca),
                        @"PACK\s*ID\s*:\s*\d{16}",
                        RegexOptions.IgnoreCase);

                    int tamanhoBloco = proximoPack.Success
                        ? proximoPack.Index
                        : texto.Length - inicioBusca;

                    string bloco = texto.Substring(
                        inicioBusca,
                        tamanhoBloco);

                    Match vendaMatch = Regex.Match(
                        bloco,
                        @"VENDA\s*:\s*(\d{16})",
                        RegexOptions.IgnoreCase);

                    if (!vendaMatch.Success)
                        continue;

                    string venda =
                        SomenteNumeros(vendaMatch.Groups[1].Value);

                    if (packId.Length == 16 &&
                        venda.Length == 16)
                    {
                        mapa[packId] = venda;
                    }
                }
            }

            return mapa;
        }

        private List<EtiquetaMercadoLivrePdf> MontarEtiquetas(
     List<PaginaMercadoLivre> paginas,
     Dictionary<string, string> mapaPackVenda)
        {
            var etiquetas = new List<EtiquetaMercadoLivrePdf>();

            foreach (PaginaMercadoLivre pagina in paginas)
            {
                if (pagina.EhPaginaResumo)
                    continue;

                string venda = SomenteNumeros(pagina.NumeroVenda);
                string packId = SomenteNumeros(pagina.PackId);
                string codigo = SomenteNumeros(pagina.CodigoEtiqueta);

                bool temVenda = venda.Length == 16;
                bool temPack = packId.Length == 16;

                if (!temVenda && !temPack)
                    continue;

                if (!temVenda && temPack)
                {
                    mapaPackVenda.TryGetValue(
                        packId,
                        out venda);

                    venda = SomenteNumeros(venda);
                }

                /*
                 * Mesmo quando o Pack não tiver uma Venda correspondente,
                 * mantém a etiqueta na lista. O formulário poderá tentar
                 * vincular pelo nome único do cliente.
                 */
                etiquetas.Add(new EtiquetaMercadoLivrePdf
                {
                    NumeroVenda = venda,
                    PackId = packId,
                    CodigoEtiqueta = codigo,
                    NomeCliente = pagina.NomeCliente,
                    Pagina = pagina.Numero,
                    TextoPagina = pagina.TextoOriginal
                });
            }

            return etiquetas;
        }

        private bool EhPaginaDeResumo(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            int quantidadeSku = Regex.Matches(
                texto,
                @"SKU\s*:",
                RegexOptions.IgnoreCase).Count;

            int quantidadeCamposQuantidade = Regex.Matches(
                texto,
                @"QUANTIDADE\s*:",
                RegexOptions.IgnoreCase).Count;

            bool temIdentificacaoProdutos =
                Regex.IsMatch(
                    texto,
                    @"IDENTIFI[A-ZÇÃÁÉÍÓÚ]*\s*PRODUTOS",
                    RegexOptions.IgnoreCase);

            /*
             * As páginas finais possuem vários SKUs e quantidades.
             * Uma etiqueta normal não possui essa estrutura.
             */
            return temIdentificacaoProdutos ||
                   quantidadeSku >= 2 ||
                   quantidadeCamposQuantidade >= 2;
        }

        private string ExtrairNumeroVenda(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            Match match = Regex.Match(
                texto,
                @"VENDA\s*:\s*(\d{16})",
                RegexOptions.IgnoreCase);

            return match.Success
                ? SomenteNumeros(match.Groups[1].Value)
                : string.Empty;
        }

        private string ExtrairPackId(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            Match match = Regex.Match(
                texto,
                @"PACK\s*ID\s*:\s*(\d{16})",
                RegexOptions.IgnoreCase);

            return match.Success
                ? SomenteNumeros(match.Groups[1].Value)
                : string.Empty;
        }

        internal string ExtrairCodigoEtiqueta(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            /*
             * O código usado na conferência do Mercado Livre
             * possui 11 dígitos e começa com 47.
             *
             * Exemplo visual:
             *
             * 47519084004
             *
             * O PdfPig às vezes cola o código no horário:
             *
             * 475190840044110:00
             *
             * Nesse caso, os primeiros 11 dígitos continuam
             * sendo o código correto.
             */
            int indiceDespachar = texto.IndexOf(
                "Despachar",
                StringComparison.OrdinalIgnoreCase);

            if (indiceDespachar >= 0)
            {
                string trechoDepoisDespachar =
                    texto.Substring(indiceDespachar);

                Match matchCodigo = Regex.Match(
                    trechoDepoisDespachar,
                    @"47\d{9}");

                if (matchCodigo.Success)
                    return matchCodigo.Value;
            }

            /*
             * Fallback para layouts que não preservem
             * a palavra Despachar.
             */
            Match fallback = Regex.Match(
                texto,
                @"47\d{9}");

            return fallback.Success
                ? fallback.Value
                : string.Empty;
        }

        private string ExtrairNomeCliente(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            /*
             * Tenta capturar o nome entre NF e Endereço.
             */
            Match match = Regex.Match(
                texto,
                @"NF\s*:\s*\d+\s*(.*?)\s*ENDERE[CÇ]O\s*:",
                RegexOptions.IgnoreCase |
                RegexOptions.Singleline);

            if (!match.Success)
                return string.Empty;

            string nome = match.Groups[1].Value.Trim();

            /*
             * Remove usuário/apelido do Mercado Livre.
             */
            nome = Regex.Replace(
                nome,
                @"\([^)]+\)",
                string.Empty);

            nome = Regex.Replace(
                nome,
                @"^S\/Z",
                string.Empty,
                RegexOptions.IgnoreCase);

            return nome.Trim();
        }

        private string NormalizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            string normalizado = texto
                .Replace('\u00A0', ' ')
                .Replace('\u200B', ' ')
                .Replace('\u200C', ' ')
                .Replace('\u200D', ' ')
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");

            normalizado = Regex.Replace(
                normalizado,
                @"[ \t]+",
                " ");

            return normalizado.Trim();
        }

        private string SomenteNumeros(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            return Regex.Replace(
                valor,
                @"\D",
                string.Empty);
        }

        private void ValidarArquivo(string caminhoPdf)
        {
            if (string.IsNullOrWhiteSpace(caminhoPdf))
            {
                throw new ArgumentException(
                    "O caminho do PDF não foi informado.");
            }

            if (!File.Exists(caminhoPdf))
            {
                throw new FileNotFoundException(
                    "O PDF do Mercado Livre não foi encontrado.",
                    caminhoPdf);
            }
        }

        private sealed class PaginaMercadoLivre
        {
            public int Numero { get; set; }

            public string TextoOriginal { get; set; } =
                string.Empty;

            public string TextoNormalizado { get; set; } =
                string.Empty;

            public string PackId { get; set; } =
                string.Empty;

            public string NumeroVenda { get; set; } =
                string.Empty;

            public string CodigoEtiqueta { get; set; } =
                string.Empty;

            public string NomeCliente { get; set; } =
                string.Empty;

            public bool EhPaginaResumo { get; set; }
        }
    }
}