using SistemaConferenciaPedidos.Helpers;
using SistemaConferenciaPedidos.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SistemaConferenciaPedidos.Services
{
    public class EtiquetaService
    {
        public List<EtiquetaMarketplaceLote> ImportarEtiquetas(string caminhoZip)
        {
            var etiquetas = new List<EtiquetaMarketplaceLote>();

            using ZipArchive zip = ZipFile.OpenRead(caminhoZip);

            ZipArchiveEntry arquivoTxt = null;

            foreach (var entry in zip.Entries)
            {
                string nome = entry.FullName.ToLowerInvariant();

                if (nome.EndsWith(".txt") && nome.Contains("etiqueta_zpl_marketplace"))
                {
                    arquivoTxt = entry;
                    break;
                }
            }

            if (arquivoTxt == null)
            {
                foreach (var entry in zip.Entries)
                {
                    string nome = entry.FullName.ToLowerInvariant();

                    if (nome.EndsWith(".txt"))
                    {
                        arquivoTxt = entry;
                        break;
                    }
                }
            }

            if (arquivoTxt == null)
                throw new Exception("Não encontrei arquivo .txt de etiqueta dentro do ZIP.");

            string conteudoCompleto;

            using (var stream = arquivoTxt.Open())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                conteudoCompleto = reader.ReadToEnd();
            }

            var etiquetasSeparadas = SepararEtiquetasZpl(conteudoCompleto);
            var chaves = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int ordem = 0;

            foreach (var etiqueta in etiquetasSeparadas)
            {
                string zpl = (etiqueta ?? "").Trim();

                if (string.IsNullOrWhiteSpace(zpl))
                    continue;

                if (!EtiquetaEhValida(zpl))
                    continue;

                string decodificado = DecodificarHexAmazon(zpl);
                string plataformaDetectada = DetectarPlataformaEtiqueta(zpl, decodificado);

                string chave = TextoHelper.NormalizarTexto(decodificado);
                if (string.IsNullOrWhiteSpace(chave))
                    chave = TextoHelper.NormalizarTexto(zpl);

                if (string.IsNullOrWhiteSpace(chave))
                    continue;

                if (chaves.Contains(chave))
                    continue;

                chaves.Add(chave);
                ordem++;

                etiquetas.Add(new EtiquetaMarketplaceLote
                {
                    NomeArquivo = arquivoTxt.FullName,
                    ConteudoZpl = zpl,
                    ConteudoNormalizado = TextoHelper.NormalizarTexto(zpl),
                    ConteudoDecodificado = TextoHelper.NormalizarTexto(decodificado),
                    PlataformaDetectada = plataformaDetectada,
                    OrdemNoArquivo = ordem
                });
            }

            if (etiquetas.Count == 0)
                throw new Exception("Nenhuma etiqueta válida foi encontrada dentro do arquivo TXT.");

            return etiquetas;
        }

        public List<string> SepararEtiquetasZpl(string conteudoCompleto)
        {
            var lista = new List<string>();

            if (string.IsNullOrWhiteSpace(conteudoCompleto))
                return lista;

            const string marcadorInicio = "^XA";
            const string marcadorFim = "^XZ";

            int posicaoAtual = 0;

            while (true)
            {
                int inicio = conteudoCompleto.IndexOf(marcadorInicio, posicaoAtual, StringComparison.OrdinalIgnoreCase);
                if (inicio < 0)
                    break;

                string prefixoAntesDaEtiqueta = conteudoCompleto.Substring(posicaoAtual, inicio - posicaoAtual);

                int fim = conteudoCompleto.IndexOf(marcadorFim, inicio, StringComparison.OrdinalIgnoreCase);
                if (fim < 0)
                    break;

                fim += marcadorFim.Length;

                string etiqueta = conteudoCompleto.Substring(inicio, fim - inicio).Trim();
                string prefixoLimpo = (prefixoAntesDaEtiqueta ?? "").Trim();

                if (etiqueta.IndexOf("^XGR", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    prefixoLimpo.IndexOf("~DGR", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    etiqueta = prefixoLimpo + Environment.NewLine + etiqueta;
                }

                if (!string.IsNullOrWhiteSpace(etiqueta))
                    lista.Add(etiqueta);

                posicaoAtual = fim;
            }

            return lista;
        }

        public bool EtiquetaEhValida(string zpl)
        {
            if (string.IsNullOrWhiteSpace(zpl))
                return false;

            string texto = zpl.Trim();
            string textoLower = texto.ToLowerInvariant();

            if (!texto.Contains("^XA", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!texto.Contains("^XZ", StringComparison.OrdinalIgnoreCase))
                return false;

            if (textoLower == "^xa^mcy^xz")
                return false;

            if (textoLower.Contains("^idr:"))
                return false;

            if (texto.Contains("^XGR", StringComparison.OrdinalIgnoreCase) &&
                texto.IndexOf("~DGR", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            if (texto.Contains("^FO", StringComparison.OrdinalIgnoreCase))
                return true;

            if (texto.Contains("^FT", StringComparison.OrdinalIgnoreCase))
                return true;

            if (texto.Contains("^FD", StringComparison.OrdinalIgnoreCase))
                return true;

            if (texto.Contains("^BC", StringComparison.OrdinalIgnoreCase))
                return true;

            if (texto.Contains("^BQN", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public string DecodificarHexAmazon(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            string convertido = Regex.Replace(
                texto,
                @"(?:_[0-9A-Fa-f]{2})+",
                match =>
                {
                    string bloco = match.Value;
                    string[] partes = bloco.Split('_', StringSplitOptions.RemoveEmptyEntries);

                    try
                    {
                        byte[] bytes = partes.Select(p => Convert.ToByte(p, 16)).ToArray();
                        return Encoding.UTF8.GetString(bytes);
                    }
                    catch
                    {
                        return bloco;
                    }
                });

            convertido = convertido
                .Replace("\\&", " ")
                .Replace("_2c", ",")
                .Replace("_2C", ",")
                .Replace("_2e", ".")
                .Replace("_2E", ".")
                .Replace("_5f", "_")
                .Replace("_5F", "_");

            return convertido;
        }

    
       
        public string ExtrairTextosFdDoZpl(string zpl)
        {
            if (string.IsNullOrWhiteSpace(zpl))
                return "";

            var sb = new StringBuilder();

            var matches = Regex.Matches(
                zpl,
                @"\^FD(.*?)\^FS",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                string conteudo = match.Groups[1].Value ?? "";

                // remove marcador hexadecimal do ZPL
                conteudo = Regex.Replace(conteudo, @"\^FH\\", "", RegexOptions.IgnoreCase);

                // converte formato \xx
                conteudo = Regex.Replace(conteudo, @"\\([0-9A-Fa-f]{2})", m =>
                {
                    try
                    {
                        int ascii = Convert.ToInt32(m.Groups[1].Value, 16);
                        return ((char)ascii).ToString();
                    }
                    catch
                    {
                        return m.Value;
                    }
                });

                // converte formato _xx_xx_xx da Shopee
                conteudo = Regex.Replace(conteudo, @"(?:_[0-9A-Fa-f]{2})+", m =>
                {
                    try
                    {
                        string bloco = m.Value;
                        string[] partes = bloco.Split('_', StringSplitOptions.RemoveEmptyEntries);
                        byte[] bytes = partes.Select(p => Convert.ToByte(p, 16)).ToArray();
                        return Encoding.UTF8.GetString(bytes);
                    }
                    catch
                    {
                        return m.Value;
                    }
                });

                // alguns textos começam direto sem "_" no primeiro byte, ex: 43_6F_...
                if (Regex.IsMatch(conteudo, @"^(?:[0-9A-Fa-f]{2})(?:_[0-9A-Fa-f]{2})+$"))
                {
                    try
                    {
                        string[] partes = conteudo.Split('_', StringSplitOptions.RemoveEmptyEntries);
                        byte[] bytes = partes.Select(p => Convert.ToByte(p, 16)).ToArray();
                        conteudo = Encoding.UTF8.GetString(bytes);
                    }
                    catch
                    {
                    }
                }

                sb.AppendLine(conteudo);
            }

            return sb.ToString();
        }

     

   

        public string DetectarPlataformaEtiqueta(string zpl, string decodificado)
        {
            string bruto = TextoHelper.NormalizarTexto(zpl);
            string texto = TextoHelper.NormalizarTexto(decodificado);

            if (Regex.IsMatch(texto, @"\b\d{3}-\d{7}-\d{7}\b") ||
                Regex.IsMatch(texto, @"\btbr\d+\b") ||
                texto.Contains("order id:") ||
                texto.Contains("pickup id:") ||
                texto.Contains("amzl") ||
                texto.Contains("amazon") ||
                texto.Contains("tentativa de entrega"))
                return "Amazon";

            if (bruto.Contains("logo_meli") ||
                bruto.Contains("pack id:") ||
                texto.Contains("mercado livre") ||
                texto.Contains("pack id:") ||
                texto.Contains("venda:"))
                return "Mercado Livre";

            bool temShopeeTexto =
                bruto.Contains("shopee") ||
                texto.Contains("shopee") ||
                bruto.Contains("spx") ||
                texto.Contains("spx");

            if (temShopeeTexto)
                return "Shopee";

            return "";
        }
    }
}