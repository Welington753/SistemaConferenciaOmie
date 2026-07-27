using System;
using System.Collections.Generic;

namespace SistemaConferenciaPedidos.Models
{
    public class PedidoDescartadoOmie
    {
        public string NumeroPedidoCliente { get; set; }
        public string CodigoPedidoOmie { get; set; }
        public string DataPrevisao { get; set; }
        public string Etapa { get; set; }
        public string Status { get; set; }
        public string Origem { get; set; }
        public string MarketplaceDetectado { get; set; }
        public string MotivoExclusao { get; set; }
        public int PaginaDescartado { get; set; }
    }

    public enum ModoBuscaOmie
    {
        Rapida,
        Completa
    }

    public class ResultadoBuscaOmie
    {
        public List<PedidoConferencia> PedidosValidos { get; set; } = new List<PedidoConferencia>();
        public List<PedidoDescartadoOmie> Descartados { get; set; } = new List<PedidoDescartadoOmie>();
        
        public int PaginasConsultadas { get; set; }
        public int PedidosBrutos { get; set; }
        public bool LimiteAtingido { get; set; }
        public int UltimaPaginaConsultada { get; set; }
    }
}
