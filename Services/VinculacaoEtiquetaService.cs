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
            List<PedidoConferencia> pedidos,
            string caminhoZip = "")
        {
            if (etiquetasLote == null || pedidos == null)
                return;


            var etiquetasDisponiveis = new List<EtiquetaMarketplaceLote>(etiquetasLote);

            var pedidosOrdenados = pedidos
                .OrderBy(p => MarketplaceHelper.NormalizarMarketplace(p.Marketplace))
                .ThenBy(p => (p.NumeroPedidoCliente ?? "").Trim())
                .ThenBy(p => TextoHelper.NormalizarTexto(p.NomeCliente ?? ""))
                .ToList();

            foreach (var pedido in pedidosOrdenados)
            {
                bool jaTemEtiqueta =
                    !string.IsNullOrWhiteSpace(pedido.EtiquetaMarketplaceZpl) &&
                    !string.IsNullOrWhiteSpace(pedido.CodigoEtiqueta);

                string marketplace = MarketplaceHelper.NormalizarMarketplace(pedido.Marketplace);

                if (jaTemEtiqueta)
                {
                    // Cura de registro da Shopee (reimportação do ZIP para corrigir arquivo ausente)
                    if (marketplace == "SHOPEE" && !string.IsNullOrWhiteSpace(caminhoZip))
                    {
                        var correspondencias = etiquetasShopeePdf?.Where(e =>
                            TextoHelper.NormalizarTexto(e.PedidoShopee) == TextoHelper.NormalizarTexto(pedido.NumeroPedidoCliente))?.ToList();

                        EtiquetaShopeePdf etiquetaPdf = null;
                        if (correspondencias != null && correspondencias.Count > 0)
                        {
                            // Validar estritamente pelo código de rastreio/etiqueta (sem fallback aproximado)
                            etiquetaPdf = correspondencias.FirstOrDefault(e => string.Equals(e.CodigoRastreio, pedido.CodigoEtiqueta, StringComparison.OrdinalIgnoreCase));
                        }

                        if (etiquetaPdf != null)
                        {
                            // Verifica se mais de um pedido não está usando a mesma página (unicidade)
                            bool paginaEmUso = pedidos.Any(p => p != pedido && p.Marketplace == "SHOPEE" && p.NomePdfNoZip == etiquetaPdf.NomeArquivoOrigem && p.PaginaPdf == etiquetaPdf.Pagina);
                            if (!paginaEmUso)
                            {
                                pedido.CaminhoZipImportacao = caminhoZip;
                                pedido.NomePdfNoZip = etiquetaPdf.NomeArquivoOrigem;
                                pedido.PaginaPdf = etiquetaPdf.Pagina;
                                pedido.EtiquetaMarketplaceZpl = $"PDF_SHOPEE|{etiquetaPdf.NomeArquivoOrigem}|{etiquetaPdf.Pagina}";
                            }
                        }
                    }
                    continue;
                }

                if (marketplace == "SHOPEE")
                {
                    var etiquetaPdf = etiquetasShopeePdf?.FirstOrDefault(e =>
                        TextoHelper.NormalizarTexto(e.PedidoShopee) ==
                        TextoHelper.NormalizarTexto(pedido.NumeroPedidoCliente));

                    if (etiquetaPdf != null)
                    {
                        pedido.EtiquetaMarketplaceZpl = $"PDF_SHOPEE|{etiquetaPdf.NomeArquivoOrigem}|{etiquetaPdf.Pagina}";
                        pedido.CodigoEtiqueta = etiquetaPdf.CodigoRastreio ?? "";
                        
                        if (string.IsNullOrWhiteSpace(pedido.CodigoEtiqueta))
                            pedido.Status = "Etiqueta encontrada sem código";
                        else
                            pedido.Status = "Etiqueta vinculada";
                            
                        if (!string.IsNullOrWhiteSpace(caminhoZip))
                            pedido.CaminhoZipImportacao = caminhoZip;

                        pedido.NomePdfNoZip = etiquetaPdf.NomeArquivoOrigem;
                        pedido.PaginaPdf = etiquetaPdf.Pagina;
                    }

                    continue;
                }

                var etiqueta = etiquetasDisponiveis.FirstOrDefault(e =>
                    EtiquetaContemCodigoDoCliente(e, pedido.NumeroPedidoCliente, pedido.Marketplace));

                if (etiqueta != null)
                {
                    pedido.EtiquetaMarketplaceZpl = etiqueta.ConteudoZpl;
                    pedido.CodigoEtiqueta = ExtrairCodigoEtiquetaDoZpl(etiqueta.ConteudoZpl, pedido.Marketplace);
                    
                    if (string.IsNullOrWhiteSpace(pedido.CodigoEtiqueta))
                        pedido.Status = "Etiqueta encontrada sem código";
                    else
                        pedido.Status = "Etiqueta vinculada";
                        
                    if (!string.IsNullOrWhiteSpace(caminhoZip))
                        pedido.CaminhoZipImportacao = caminhoZip;

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
            
            if (!string.IsNullOrWhiteSpace(etiqueta.PlataformaDetectada))
            {
                if (MarketplaceHelper.NormalizarMarketplace(etiqueta.PlataformaDetectada) != marketplaceNormalizado)
                    return false;
            }
            string numeroOriginal = (numeroPedidoCliente ?? "").Trim();

            string numeroNormalizado = TextoHelper.NormalizarTexto(numeroOriginal);
            string numeroSemCaracteres = TextoHelper.SomenteLetrasENumeros(numeroOriginal);

            string zpl = etiqueta.ConteudoZpl ?? "";
            string zplNormalizado = etiqueta.ConteudoNormalizado ?? TextoHelper.NormalizarTexto(zpl);
            string zplSemCaracteres = TextoHelper.SomenteLetrasENumeros(zpl);

            string decodificado = etiqueta.ConteudoDecodificado ?? "";
            string decodificadoNormalizado = TextoHelper.NormalizarTexto(decodificado);
            string decodificadoSemCaracteres = TextoHelper.SomenteLetrasENumeros(decodificado);

            if (marketplaceNormalizado != "MERCADO LIVRE")
            {
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
                // Algumas etiquetas Amazon guardam o código dentro dos campos ^FD
                // usando hexadecimal. Por isso juntamos o texto decodificado,
                // os campos ^FD decodificados e o ZPL bruto.
                string textosFd = _etiquetaService.ExtrairTextosFdDoZpl(zpl);
                string textoAmazon = (
                    texto + "\n" +
                    textosFd + "\n" +
                    zpl).ToUpperInvariant();

                string codigoAmazon = ExtrairCodigoAmazon(textoAmazon, textosFd);

                if (!string.IsNullOrWhiteSpace(codigoAmazon))
                    return codigoAmazon;
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

        private string ExtrairCodigoAmazon(string textoAmazon, string textosFd)
        {
            if (string.IsNullOrWhiteSpace(textoAmazon))
                return "";

            // Formato antigo utilizado nas etiquetas Amazon.
            var amazon = Regex.Match(
                textoAmazon,
                @"(?<![A-Z0-9])TBR[A-Z0-9]{6,24}(?![A-Z0-9])",
                RegexOptions.IgnoreCase);

            if (amazon.Success)
                return amazon.Value.Trim().ToUpperInvariant();

            // Formato atual. Antes o sistema aceitava somente AMZB.
            // Agora aceita qualquer código iniciado por AMZ, incluindo AMZB e AMZL.
            amazon = Regex.Match(
                textoAmazon,
                @"(?<![A-Z0-9])AMZ[A-Z0-9]{6,24}(?![A-Z0-9])",
                RegexOptions.IgnoreCase);

            if (amazon.Success)
                return amazon.Value.Trim().ToUpperInvariant();

            // Se houver separadores entre os caracteres, limpa cada campo ^FD
            // individualmente para não juntar informações diferentes da etiqueta.
            string[] camposFd = (textosFd ?? "")
                .Split(
                    new[] { "\r\n", "\n", "\r" },
                    StringSplitOptions.RemoveEmptyEntries);

            foreach (string campo in camposFd)
            {
                string campoLimpo = Regex.Replace(
                    (campo ?? "").ToUpperInvariant(),
                    @"[^A-Z0-9]",
                    "");

                amazon = Regex.Match(
                    campoLimpo,
                    @"TBR[A-Z0-9]{6,24}",
                    RegexOptions.IgnoreCase);

                if (amazon.Success)
                    return amazon.Value.Trim().ToUpperInvariant();

                amazon = Regex.Match(
                    campoLimpo,
                    @"AMZ[A-Z0-9]{6,24}",
                    RegexOptions.IgnoreCase);

                if (amazon.Success)
                    return amazon.Value.Trim().ToUpperInvariant();
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