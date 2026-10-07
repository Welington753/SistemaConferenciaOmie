using SistemaConferenciaPedidos.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using System.Windows.Forms;

namespace SistemaConferenciaPedidos.Services
{
    public class MercadoLivrePdfService
    {
        private readonly string _diretorioGerenciadoOverride;

        public MercadoLivrePdfService()
        {
        }

        internal MercadoLivrePdfService(string diretorioGerenciadoParaTestes)
        {
            _diretorioGerenciadoOverride = ValidarDiretorioTemporarioDeTeste(diretorioGerenciadoParaTestes);
        }

        private string ValidarDiretorioTemporarioDeTeste(string diretorio)
        {
            if (string.IsNullOrWhiteSpace(diretorio))
                throw new ArgumentException("O diretório de teste não pode ser vazio.");

            string tempPath = Path.GetFullPath(Path.GetTempPath());
            string fullDir = Path.GetFullPath(diretorio);

            if (!fullDir.StartsWith(tempPath, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("O diretório de teste deve estar abaixo de Path.GetTempPath().");

            if (!fullDir.Contains("SistemaConferenciaPedidos.Tests", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("O diretório de teste deve conter o segmento 'SistemaConferenciaPedidos.Tests'.");

            string appData = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            string realEtiquetasDir = Path.Combine(appData, "SistemaConferenciaPedidos", "Etiquetas");

            if (fullDir.StartsWith(realEtiquetasDir, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("O diretório de teste não pode estar dentro do LocalApplicationData real operacional (SistemaConferenciaPedidos/Etiquetas).");

            return fullDir;
        }

        private string ObterDiretorioGerenciado()
        {
            if (!string.IsNullOrWhiteSpace(_diretorioGerenciadoOverride))
                return _diretorioGerenciadoOverride;

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "SistemaConferenciaPedidos", "Etiquetas", "MercadoLivre");
        }
        public virtual (List<EtiquetaMercadoLivrePdf> Etiquetas, string CaminhoGerenciado) CarregarEtiquetas(
            string caminhoPdf)
        {
            string caminhoGerenciado = ValidarECopiarArquivo(caminhoPdf);

            List<PaginaMercadoLivre> paginas =
                LerPaginas(caminhoGerenciado);

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

            return (etiquetas, caminhoGerenciado);
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

            for (int i = 1; i <= documento.NumberOfPages; i++)
            {
                var paginaPdf = documento.GetPage(i);

                string textoExtraido = ContentOrderTextExtractor.GetText(paginaPdf, true);

                string textoNormalizado =
                    NormalizarTexto(textoExtraido);

                paginas.Add(new PaginaMercadoLivre
                {
                    Numero = paginaPdf.Number,
                    TextoOriginal = textoExtraido,
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

            bool temIdentificadorResumo = Regex.IsMatch(
                texto,
                @"\bL[A-Z0-9]{25}\b",
                RegexOptions.IgnoreCase);

            /*
             * As páginas finais possuem vários SKUs e quantidades.
             * Uma etiqueta normal não possui essa estrutura.
             */
            return temIdentificacaoProdutos ||
                   quantidadeSku >= 2 ||
                   quantidadeCamposQuantidade >= 2 ||
                   temIdentificadorResumo;
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

            // 1. Normalize apenas CRLF/CR para LF, preservando linhas
            texto = texto.Replace("\r\n", "\n").Replace("\r", "\n");
            var linhas = texto.Split('\n');
            var candidatos = new HashSet<string>();

            // Fallback: acumula linhas consecutivas estritamente numéricas
            var blocoNumericoAtual = new System.Text.StringBuilder();

            void ProcessarAgrupamento(string agrupamento)
            {
                if (string.IsNullOrWhiteSpace(agrupamento)) return;

                string limpo = System.Text.RegularExpressions.Regex.Replace(agrupamento, @"[\s\t\u00A0]+", "");
                if (string.IsNullOrEmpty(limpo)) return;

                // Regra 8: Agrupamentos com tamanhos não múltiplos de 11 devem ser rejeitados, etc.
                // Mas a regra diz: "Agrupamentos com 12, 13, 16 ou outros tamanhos devem ser rejeitados, nunca parcialmente aproveitados."
                // "Se um agrupamento tiver múltiplos exatos de 11..."
                if (limpo.Length % 11 != 0) return;

                int partes = limpo.Length / 11;
                var blocos = new List<string>();
                for (int i = 0; i < partes; i++)
                {
                    blocos.Add(limpo.Substring(i * 11, 11));
                }

                // Regra 6 e 7
                if (blocos.All(b => b.StartsWith("47")))
                {
                    if (blocos.Distinct().Count() == 1)
                    {
                        // Regra 7: todos idênticos, adiciona apenas o rastreio (HashSet cuidará de deduplicar globalmente)
                        candidatos.Add(blocos.First());
                    }
                    else
                    {
                        // Diferentes no mesmo agrupamento = ambíguo, força falha adicionando dois diferentes no HashSet
                        candidatos.Add(blocos[0]);
                        candidatos.Add(blocos[1]);
                    }
                }
            }

            foreach (var linha in linhas)
            {
                // Regra 2: Examine cada linha separadamente
                // Regra 3: Considere como espaço interno somente: espaço comum, tabulação e NBSP

                // Primeiro extrai agrupamentos numéricos máximos da linha isolada
                // Usamos Regex para achar grupos de dígitos separados por espaços válidos.
                // Mas atenção: se a linha tem "477987 92847", isso é UM agrupamento.
                // Se a linha tem "47798792847 02:30", há o agrupamento "47798792847" e o "02", e o "30" (já que o ':' quebra o agrupamento numérico).
                // Mas wait, a regra diz "Extraia agrupamentos numéricos máximos". O que é um agrupamento numérico?
                // Sequência de dígitos, possivelmente contendo " ", "\t" ou "\u00A0" entre eles, mas delimitada por qualquer outro caractere.
                var matches = System.Text.RegularExpressions.Regex.Matches(linha, @"[\d\s\t\u00A0]+");
                foreach (System.Text.RegularExpressions.Match m in matches)
                {
                    // A match pode ter capturado espaços antes/depois, vamos trimar os espaços
                    string ag = m.Value.Trim(new[] { ' ', '\t', '\u00A0' });
                    if (!string.IsNullOrEmpty(ag))
                    {
                        // Um agrupamento isolado na linha
                        ProcessarAgrupamento(ag);
                    }
                }

                // Fallback entre linhas
                // - nunca agrupe uma linha que contenha dois-pontos, letras ou horário
                // - somente agrupe linhas consecutivas compostas EXCLUSIVAMENTE por dígitos, espaço, tabulação ou NBSP
                bool eEstritamenteNumerica = !string.IsNullOrWhiteSpace(linha) &&
                                             linha.All(c => char.IsDigit(c) || c == ' ' || c == '\t' || c == '\u00A0');

                if (eEstritamenteNumerica)
                {
                    blocoNumericoAtual.Append(linha).Append(" ");
                }
                else
                {
                    // Se quebrou a sequência, processa o bloco numérico acumulado
                    if (blocoNumericoAtual.Length > 0)
                    {
                        ProcessarAgrupamento(blocoNumericoAtual.ToString());
                        blocoNumericoAtual.Clear();
                    }
                }
            }

            // Processa resto
            if (blocoNumericoAtual.Length > 0)
            {
                ProcessarAgrupamento(blocoNumericoAtual.ToString());
            }

            // Regra 10: Resultado
            if (candidatos.Count == 1)
                return candidatos.First();

            return string.Empty;
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

        private string ValidarECopiarArquivo(string caminhoPdf)
        {
            if (string.IsNullOrWhiteSpace(caminhoPdf))
                throw new ArgumentException("O caminho do PDF não foi informado.");

            if (!File.Exists(caminhoPdf))
                throw new FileNotFoundException("O PDF do Mercado Livre não foi encontrado.", caminhoPdf);

            var info = new FileInfo(caminhoPdf);
            if (info.Length > 20 * 1024 * 1024)
                throw new InvalidOperationException("O PDF excedeu o limite máximo de 20MB.");

            // Calcula hash e valida estrutura do arquivo de origem
            string hashString = CalcularSha256(caminhoPdf);
            int paginasOrigem = ContarPaginasPdfPig(caminhoPdf);
            if (paginasOrigem < 1)
                throw new InvalidOperationException("O PDF não contém páginas válidas.");

            string managedDir = ObterDiretorioGerenciado();
            Directory.CreateDirectory(managedDir);

            string managedPath = Path.Combine(managedDir, $"{hashString}.pdf");

            // Se o definitivo já existe, confirma integridade antes de reutilizar
            if (File.Exists(managedPath))
            {
                string hashExistente = CalcularSha256(managedPath);
                if (!string.Equals(hashExistente, hashString, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        "O arquivo gerenciado existente está corrompido (hash divergente). Exclua-o manualmente e tente novamente.");

                int paginasExistente = ContarPaginasPdfPig(managedPath);
                if (paginasExistente != paginasOrigem)
                    throw new InvalidOperationException(
                        $"O arquivo gerenciado existente tem {paginasExistente} página(s), mas o original tem {paginasOrigem}. Exclua-o manualmente e tente novamente.");

                return managedPath;
            }

            // Cópia atômica: temporário exclusivo usando Guid, FileStream, Flush(true), depois Move
            string tempFile = managedPath + "_" + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var sourceFs = new FileStream(caminhoPdf, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var destFs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    sourceFs.CopyTo(destFs);
                    destFs.Flush(true); // Garante gravação física no disco
                }

                // Verifica hash do temporário antes de promover
                string hashTemp = CalcularSha256(tempFile);
                if (!string.Equals(hashTemp, hashString, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        $"Falha de integridade na cópia: hash do temporário ({hashTemp}) difere do original ({hashString}).");

                // Valida estrutura do PDF copiado
                int paginasTemp = ContarPaginasPdfPig(tempFile);
                if (paginasTemp != paginasOrigem)
                    throw new InvalidOperationException(
                        $"Falha de integridade: PDF copiado tem {paginasTemp} página(s), original tem {paginasOrigem}.");

                try
                {
                    File.Move(tempFile, managedPath);
                }
                catch (IOException ex) when (EhColisaoDePromocao(ex))
                {
                    if (File.Exists(managedPath) &&
                        ArquivoGerenciadoEhValido(
                            managedPath,
                            hashString,
                            paginasOrigem))
                    {
                        return managedPath;
                    }

                    throw;
                }
            }
            finally
            {
                // Exclui apenas o temporário criado por esta operação; nunca toca no definitivo
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { /* ignora erro de limpeza */ }
                }
            }

            return managedPath;
        }

        private bool EhColisaoDePromocao(IOException ex)
        {
            int hResult = ex.HResult;
            return hResult == unchecked((int)0x80070020) ||
                   hResult == unchecked((int)0x800700B7) ||
                   hResult == unchecked((int)0x80070050);
        }

        private bool ArquivoGerenciadoEhValido(string managedPath, string hashEsperado, int paginasEsperadas)
        {
            try
            {
                if (!File.Exists(managedPath)) return false;

                string hashExistente = CalcularSha256(managedPath);
                if (!string.Equals(hashExistente, hashEsperado, StringComparison.OrdinalIgnoreCase)) return false;

                int paginas = ContarPaginasPdfPig(managedPath);
                if (paginas != paginasEsperadas) return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Calcula SHA-256 de um arquivo usando FileStream (sem carregar em memória).</summary>
        private static string CalcularSha256(string caminhoArquivo)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            using var fs = new FileStream(caminhoArquivo, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] hashBytes = sha256.ComputeHash(fs);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>Abre PDF com PdfPig e retorna número de páginas; 0 em caso de falha.</summary>
        private static int ContarPaginasPdfPig(string caminhoPdf)
        {
            try
            {
                using var fs = new FileStream(caminhoPdf, FileMode.Open, FileAccess.Read, FileShare.Read);
                // Valida cabeçalho %PDF
                byte[] header = new byte[4];
                int lidos = 0;
                while (lidos < 4)
                {
                    int n = fs.Read(header, lidos, 4 - lidos);
                    if (n == 0) break;
                    lidos += n;
                }
                if (lidos < 4 || System.Text.Encoding.ASCII.GetString(header, 0, 4) != "%PDF")
                    return 0;

                fs.Position = 0;
                using var doc = UglyToad.PdfPig.PdfDocument.Open(fs);
                return doc.NumberOfPages;
            }
            catch
            {
                return 0;
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
