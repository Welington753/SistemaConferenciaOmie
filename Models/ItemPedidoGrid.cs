using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaConferenciaPedidos.Models
{
    public class ItemPedidoGrid
    {
        public string Sku { get; set; } = "";
        public string Produto { get; set; } = "";
        public string Quantidade { get; set; } = "";
        public string Ean { get; set; } = "";
    }
}
