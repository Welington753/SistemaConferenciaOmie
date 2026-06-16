using SistemaConferenciaPedidos.Helpers;
using SistemaConferenciaPedidos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SistemaConferenciaPedidos.Services
{
    public class VinculacaoEtiquetaService
    {
        private readonly EtiquetaService _etiquetaService;

        public VinculacaoEtiquetaService()
        {
            _etiquetaService = new EtiquetaService();
        }

        public void VincularEtiquetas(
            List<EtiquetaMarketplaceLote> etiquetasLote,
            List<EtiquetaShopeePdf> etiquetasShopeePdf,
            List<PedidoConferencia> pedidos)
        {
            if (etiquetasLote == null || pedidos == null)
                return;

            foreach (var pedido in pedidos)
            {
                pedido.EtiquetaMarketplaceZpl = null;
                pedido.CodigoEtiqueta = null;
                pedido.Status = "Sem etiqueta";
            }

            var etiquetasDisponiveis = new List<EtiquetaMarketplaceLote>(etiquetasLote);

            var pedidosOrdenados = pedidos
                .OrderBy(p => MarketplaceHelper.NormalizarMarketplace(p.Marketplace))
                .ThenBy(p => (p.NumeroPedidoCliente ?? "").Trim())
                .ThenBy(p => TextoHelper.NormalizarTexto(p.NomeCliente ?? ""))
                .ToList();

            foreach (var pedido in pedidosOrdenados)
            {
                string marketplace = MarketplaceHelper.NormalizarMarketplace(pedido.Marketplace);

                if (marketplace == "SHOPEE")
                {
                    var etiquetaPdf = etiquetasShopeePdf?.FirstOrDefault(e =>
                        TextoHelper.NormalizarTexto(e.PedidoShopee) ==
                        TextoHelper.NormalizarTexto(pedido.NumeroPedidoCliente));

                    if (etiquetaPdf != null)
                    {
                        pedido.EtiquetaMarketplaceZpl = $"PDF_SHOPEE|{etiquetaPdf.Pagina}";
                        pedido.CodigoEtiqueta = etiquetaPdf.CodigoRastreio ?? "";
                        pedido.Status = "Etiqueta vinculada";
                    }

                    continue;
                }

                var etiqueta = etiquetasDisponiveis.FirstOrDefault(e =>
                    EtiquetaContemCodigoDoCliente(e, pedido.NumeroPedidoCliente, pedido.Marketplace));

                if (etiqueta != null)
                {
                    pedido.EtiquetaMarketplaceZpl = etiqueta.ConteudoZpl;
                    pedido.CodigoEtiqueta = ExtrairCodigoEtiquetaDoZpl(etiqueta.ConteudoZpl, pedido.Marketplace);
                    pedido.Status = "Etiqueta vinculada";

                    etiquetasDisponiveis.Remove(etiqueta);
                }
            }
        }

        private bool EtiquetaContemCodigoDoCliente(
            EtiquetaMarketplaceLote etiqueta,
            string numeroPedidoCliente,
            string marketplace)
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

            string decodificado = etiqueta.ConteudoDecodificado ?? "";
            string decodificadoNormalizado = TextoHelper.NormalizarTexto(decodificado);
            string decodificadoSemCaracteres = TextoHelper.SomenteLetrasENumeros(decodificado);

            if (!string.IsNullOrWhiteSpace(numeroNormalizado))
            {
                if (zplNormalizado.Contains(numeroNormalizado) || decodificadoNormalizado.Contains(numeroNormalizado))
                    return true;
            }

            if (!string.IsNullOrWhiteSpace(numeroSemCaracteres))
            {
                if (zplSemCaracteres.Contains(numeroSemCaracteres) ||
                    decodificadoSemCaracteres.Contains(numeroSemCaracteres))
                    return true;
            }

            if (marketplaceNormalizado == "MERCADO LIVRE" &&
                numeroOriginal.StartsWith("20000") &&
                numeroOriginal.Length > 5)
            {
                string numeroSemPrefixo = numeroOriginal.Substring(5);
                string numeroSemPrefixoNormalizado = TextoHelper.NormalizarTexto(numeroSemPrefixo);
                string numeroSemPrefixoSemCaracteres = TextoHelper.SomenteLetrasENumeros(numeroSemPrefixo);

                if (!string.IsNullOrWhiteSpace(numeroSemPrefixoNormalizado))
                {
                    if (zplNormalizado.Contains(numeroSemPrefixoNormalizado) ||
                        decodificadoNormalizado.Contains(numeroSemPrefixoNormalizado))
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

        private string ExtrairCodigoEtiquetaDoZpl(string zpl, string marketplace)
        {
            if (string.IsNullOrWhiteSpace(zpl))
                return "";

            string texto = _etiquetaService.DecodificarHexAmazon(zpl).ToUpperInvariant();

            if ((marketplace ?? "").Trim().Equals("Shopee", StringComparison.OrdinalIgnoreCase))
            {
                var shopee = Regex.Match(texto, @"BR[A-Z0-9]{13}", RegexOptions.IgnoreCase);
                if (shopee.Success)
                    return shopee.Value;
            }

            if ((marketplace ?? "").Trim().Equals("Amazon", StringComparison.OrdinalIgnoreCase))
            {
                string textoAmazon = (texto + "\n" + zpl).ToUpperInvariant();

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

            return "";
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
    }
}