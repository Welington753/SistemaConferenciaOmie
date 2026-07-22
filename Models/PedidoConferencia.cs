using System;

namespace SistemaConferenciaPedidos.Models
{
    public class PedidoConferencia
    {
        public int Id { get; set; }
        public string CodigoEtiqueta { get; set; } = "";
        public string NumeroPedidoCliente { get; set; } = "";
        public string NomeCliente { get; set; } = "";
        public string Marketplace { get; set; } = "";
        public string JsonItens { get; set; } = "";
        public string EtiquetaMarketplaceZpl { get; set; } = "";
        public string Status { get; set; } = "Pendente";
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }
        public DateTime? DataConferencia { get; set; }
        public DateTime? DataPrevisao { get; set; }
        public bool Impresso { get; set; }
        public bool Conferido { get; set; }
        public string CaminhoZipImportacao { get; set; } = "";
        public string NomePdfNoZip { get; set; } = "";
        public int? PaginaPdf { get; set; }

        public DateTime? DataPrimeiraImpressao { get; set; }
        public DateTime? DataReimpressao { get; set; }
        public string MotivoReimpressao { get; set; } = "";
        public bool Oculto { get; set; }
    }
}