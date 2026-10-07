using System.IO;

namespace SistemaConferenciaPedidos.Services
{
    public interface IPdfTextProvider
    {
        string ExtrairTexto(string caminhoPdf, int pagina);
    }
}
