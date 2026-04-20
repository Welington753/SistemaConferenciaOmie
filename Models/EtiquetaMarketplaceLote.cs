using System;

namespace SistemaConferenciaPedidos.Models
{
    public class EtiquetaMarketplaceLote
    {
        public string NomeArquivo { get; set; } = "";
        public string ConteudoZpl { get; set; } = "";
        public string ConteudoNormalizado { get; set; } = "";
        public string ConteudoDecodificado { get; set; } = "";
        public string PlataformaDetectada { get; set; } = "";
        public int OrdemNoArquivo { get; set; }
    }
}