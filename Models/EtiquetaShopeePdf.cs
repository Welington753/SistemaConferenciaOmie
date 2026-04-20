using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaConferenciaPedidos.Models
{
    public class EtiquetaShopeePdf
    {
        public string PedidoShopee { get; set; } = "";
        public string CodigoRastreio { get; set; } = "";
        public string NomeCliente { get; set; } = "";
        public int Pagina { get; set; }
        public string TextoPagina { get; set; } = "";
    }
}
