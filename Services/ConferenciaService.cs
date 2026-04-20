using SistemaConferenciaPedidos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SistemaConferenciaPedidos.Services
{
    public class ConferenciaService
    {
        public PedidoConferencia BuscarPedidoPorCodigoOuNumero(
            IEnumerable<PedidoConferencia> pedidos,
            string textoLido,
            out bool encontradoPorNumeroPedido)
        {
            encontradoPorNumeroPedido = false;

            if (pedidos == null)
                return null;

            string codigoBuscado = NormalizarCodigoConferencia(textoLido);

            if (!string.IsNullOrWhiteSpace(codigoBuscado))
            {
                foreach (var pedido in pedidos)
                {
                    string codigoEtiqueta = NormalizarCodigoConferencia(pedido.CodigoEtiqueta ?? "");

                    if (string.IsNullOrWhiteSpace(codigoEtiqueta))
                        continue;

                    if (codigoEtiqueta.Equals(codigoBuscado, StringComparison.OrdinalIgnoreCase))
                        return pedido;
                }
            }

            string numeroBuscado = NormalizarNumeroPedido(textoLido);

            if (!string.IsNullOrWhiteSpace(numeroBuscado))
            {
                foreach (var pedido in pedidos)
                {
                    string numeroPedido = NormalizarNumeroPedido(pedido.NumeroPedidoCliente ?? "");

                    if (string.IsNullOrWhiteSpace(numeroPedido))
                        continue;

                    if (numeroPedido.Equals(numeroBuscado, StringComparison.OrdinalIgnoreCase))
                    {
                        encontradoPorNumeroPedido = true;
                        return pedido;
                    }
                }
            }

            return null;
        }

        public string NormalizarCodigoConferencia(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return "";

            string valor = codigo.Trim().ToUpperInvariant();
            valor = Regex.Replace(valor, @"\s+", "");

            if (Regex.IsMatch(valor, @"^BR[A-Z0-9]{13}$", RegexOptions.IgnoreCase))
                return valor;

            if (Regex.IsMatch(valor, @"^TBR\d+$", RegexOptions.IgnoreCase))
                return valor;

            string somenteNumeros = Regex.Replace(valor, @"\D", "");
            if (!string.IsNullOrWhiteSpace(somenteNumeros))
                return somenteNumeros;

            return Regex.Replace(valor, @"[^A-Z0-9]", "");
        }

        public string NormalizarNumeroPedido(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return "";

            return Regex.Replace(valor.Trim().ToUpperInvariant(), @"[^A-Z0-9]", "");
        }

        public string NormalizarMarketplaceResumo(string marketplace)
        {
            string valor = (marketplace ?? "").Trim();

            if (valor.Equals("Mercado Livre", StringComparison.OrdinalIgnoreCase) ||
                valor.Equals("MLV", StringComparison.OrdinalIgnoreCase) ||
                valor.Equals("MLB", StringComparison.OrdinalIgnoreCase) ||
                valor.Equals("MELI", StringComparison.OrdinalIgnoreCase))
                return "MERCADO LIVRE";

            if (valor.Equals("Amazon", StringComparison.OrdinalIgnoreCase) ||
                valor.Equals("AMZ", StringComparison.OrdinalIgnoreCase))
                return "AMAZON";

            if (valor.Equals("Shopee", StringComparison.OrdinalIgnoreCase) ||
                valor.Equals("SHP", StringComparison.OrdinalIgnoreCase))
                return "SHOPEE";

            return valor.ToUpperInvariant();
        }
    }
}