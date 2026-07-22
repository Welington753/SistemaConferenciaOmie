namespace SistemaConferenciaPedidos.Models
{
    public class ResumoPreparacaoResult
    {
        public int Total { get; set; }
        public int Preparados { get; set; }
        public int Faltam { get; set; }
        public int Percentual { get; set; }

        public ResumoPreparacaoResult()
        {
        }

        public ResumoPreparacaoResult(int total, int preparados, int faltam, int percentual)
        {
            Total = total;
            Preparados = preparados;
            Faltam = faltam;
            Percentual = percentual;
        }
    }
}
