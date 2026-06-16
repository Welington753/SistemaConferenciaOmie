using SistemaConferenciaPedidos.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaConferenciaPedidos.Services
{
    public class ValidacaoEanService
    {
        private readonly PedidoItemService _pedidoItemService;


        public const string CodigoLiberacaoManual = "0051000012517";

        public bool EhCodigoLiberacaoManual(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return false;

            return codigo.Trim().Equals(
                CodigoLiberacaoManual,
                StringComparison.OrdinalIgnoreCase);
        }
        public ValidacaoEanService()
        {
            _pedidoItemService = new PedidoItemService();
        }

        public List<ItemPedidoGrid> ObterItensQuePrecisamValidar(string jsonItens)
        {
            var itensComEan = _pedidoItemService.ObterItensComEanObrigatorio(jsonItens);

            if (itensComEan == null || itensComEan.Count == 0)
                return new List<ItemPedidoGrid>();

            return itensComEan
                .Where(i => !SkuIgnoraValidacaoEan(i.Sku))
                .ToList();
        }

        public bool PedidoPrecisaValidarEan(string jsonItens)
        {
            return ObterItensQuePrecisamValidar(jsonItens).Count > 0;
        }


        private bool SkuIgnoraValidacaoEan(string sku)
        {
            sku = (sku ?? "").Trim();

            if (string.IsNullOrWhiteSpace(sku))
                return false;

            if (sku.Contains("EZVAL6PST", StringComparison.OrdinalIgnoreCase))
                return true;

            if (sku.Contains("KIT", StringComparison.OrdinalIgnoreCase))
                return true;

            if (sku.Contains("Vari", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}