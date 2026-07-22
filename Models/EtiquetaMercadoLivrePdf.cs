namespace SistemaConferenciaPedidos.Models
{
    public class EtiquetaMercadoLivrePdf
    {
        public string NumeroVenda { get; set; } = "";
        public string PackId { get; set; } = "";
        public string CodigoEtiqueta { get; set; } = "";
        public string NomeCliente { get; set; } = "";
        public int Pagina { get; set; }
        public string TextoPagina { get; set; } = "";
    }
}