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
                        if (!string.IsNullOrWhiteSpace(pedido.CodigoEtiqueta))
                            continue; // Regra: Não permitir bypass de etiqueta válida

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
            
            // Remove espaços, CR, LF, TAB e caracteres de controle
            valor = Regex.Replace(valor, @"[\s\p{C}]+", "");

            return valor;
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