using System;

namespace SistemaConferenciaPedidos.Models
{
    public class PedidoConferencia
    {
        public string CodigoEtiqueta { get; set; } = "";
        public string NumeroPedidoCliente { get; set; } = "";
        public string NomeCliente { get; set; } = "";
        public string Marketplace { get; set; } = "";
        public string JsonItens { get; set; } = "";
        public string EtiquetaMarketplaceZpl { get; set; } = "";
        public string Status { get; set; } = "Pendente";
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataConferencia { get; set; }
        public bool Impresso { get; set; }
        public bool Conferido { get; set; }
      
    }
}