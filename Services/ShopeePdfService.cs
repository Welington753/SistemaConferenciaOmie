using SistemaConferenciaPedidos.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace SistemaConferenciaPedidos.Services
{
    public class ShopeePdfService
    {
        public List<EtiquetaShopeePdf> CarregarEtiquetasDoZip(ZipArchive zip, out string nomePdfNoZip)
        {
            nomePdfNoZip = "";
            var etiquetas = new List<EtiquetaShopeePdf>();

            if (zip == null)
                return etiquetas;

            var pdfs = zip.Entries
                .Where(e => e.FullName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var entry in pdfs)
            {
                nomePdfNoZip = entry.FullName;

                using var stream = entry.Open();
                using var ms = new MemoryStream();

                stream.CopyTo(ms);
                ms.Position = 0;

                using var document = PdfDocument.Open(ms);

                foreach (var page in document.GetPages())
                {
                    string textoPagina = page.Text ?? "";

                    if (!PaginaPareceShopee(textoPagina))
                        continue;

                    string pedido = ExtrairPedidoShopeeDoPdf(textoPagina);
                    string rastreio = ExtrairCodigoRastreioShopeeDoPdf(textoPagina);
                    string nomeCliente = ExtrairNomeClienteShopeeDoPdf(textoPagina);

                    if (string.IsNullOrWhiteSpace(pedido) && string.IsNullOrWhiteSpace(rastreio))
                        continue;

                    etiquetas.Add(new EtiquetaShopeePdf
                    {
                        PedidoShopee = pedido,
                        CodigoRastreio = rastreio,
                        NomeCliente = nomeCliente,
                        Pagina = page.Number,
                        TextoPagina = textoPagina
                    });
                }
            }

            return etiquetas;
        }

        private bool PaginaPareceShopee(string textoPagina)
        {
            if (string.IsNullOrWhiteSpace(textoPagina))
                return false;

            string texto = textoPagina.ToUpperInvariant();

            bool temDanfe = texto.Contains("DANFE SIMPLIFICADO");
            bool temPedido = texto.Contains("PEDIDO");
            bool temRastreio = Regex.IsMatch(texto, @"BR[A-Z0-9]{13}", RegexOptions.IgnoreCase);
            bool temRemetente = texto.Contains("EZIE HOME") || texto.Contains("REMETENTE");
            bool temDestino = texto.Contains("DESTINAT");

            return (temPedido && temRastreio) ||
                   (temDanfe && temPedido) ||
                   (temRemetente && temDestino && temRastreio);
        }

        private string ExtrairPedidoShopeeDoPdf(string textoPagina)
        {
            if (string.IsNullOrWhiteSpace(textoPagina))
                return "";

            string texto = textoPagina.ToUpperInvariant();

            var matches = Regex.Matches(texto, @"26[A-Z0-9]{12}", RegexOptions.IgnoreCase);

            string melhor = "";

            foreach (Match match in matches)
            {
                string valor = match.Value.Trim().ToUpperInvariant();

                if (valor.Length == 14 && valor.Any(char.IsLetter))
                {
                    if (texto.Contains("PEDIDO") && texto.IndexOf(valor) > texto.IndexOf("PEDIDO"))
                        return valor;

                    melhor = valor;
                }
            }

            return melhor;
        }

        private string ExtrairCodigoRastreioShopeeDoPdf(string textoPagina)
        {
            if (string.IsNullOrWhiteSpace(textoPagina))
                return "";

            string texto = textoPagina.ToUpperInvariant();

            var match = Regex.Match(texto, @"BR[A-Z0-9]{13}", RegexOptions.IgnoreCase);

            if (match.Success)
                return match.Value.Trim().ToUpperInvariant();

            return "";
        }

        private string ExtrairNomeClienteShopeeDoPdf(string textoPagina)
        {
            if (string.IsNullOrWhiteSpace(textoPagina))
                return "";

            var linhas = textoPagina
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(l => (l ?? "").Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            for (int i = 0; i < linhas.Count; i++)
            {
                string linha = linhas[i].ToUpperInvariant();

                if (linha == "REMETENTE" && i + 3 < linhas.Count)
                    return linhas[i + 3].Trim();
            }

            return "";
        }
    }
}