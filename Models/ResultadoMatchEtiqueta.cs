using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaConferenciaPedidos.Models
{
    public class ResultadoMatchEtiqueta
    {
        public EtiquetaMarketplaceLote Etiqueta { get; set; }
        public int Pontuacao { get; set; }
        public bool MatchForte { get; set; }
        public string Motivo { get; set; } = "";
    }
}