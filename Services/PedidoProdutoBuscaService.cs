using SistemaConferenciaPedidos.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaConferenciaPedidos.Services
{
    public class PedidoProdutoBuscaService
    {
        private readonly PedidoItemService _pedidoItemService;

        public PedidoProdutoBuscaService()
        {
            _pedidoItemService = new PedidoItemService();
        }

        public List<PedidoConferencia> BuscarPedidosPorEanOuSku(
            List<PedidoConferencia> pedidos,
            string codigoBipado)
        {
            var encontrados = new List<PedidoConferencia>();

            if (pedidos == null || pedidos.Count == 0)
                return encontrados;

            codigoBipado = Normalizar(codigoBipado);

            if (string.IsNullOrWhiteSpace(codigoBipado))
                return encontrados;

            foreach (var pedido in pedidos)
            {
                if (pedido.Impresso)
                    continue;

                var itens = _pedidoItemService.MontarItensDoPedido(pedido.JsonItens);

                foreach (var item in itens)
                {
                    string ean = Normalizar(item.Ean);
                    string sku = Normalizar(item.Sku);

                    bool encontrouPorEan =
                        !string.IsNullOrWhiteSpace(ean) &&
                        ean == codigoBipado;

                    bool encontrouPorSku =
                        !string.IsNullOrWhiteSpace(sku) &&
                        sku == codigoBipado;

                    if (encontrouPorEan || encontrouPorSku)
                    {
                        encontrados.Add(pedido);
                        break;
                    }
                }
            }

            return encontrados
                .Distinct()
                .ToList();
        }

        private string Normalizar(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return "";

            return valor
                .Trim()
                .ToUpperInvariant()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace(".", "")
                .Replace("/", "");
        }
    }
}