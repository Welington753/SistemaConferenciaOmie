using System.Drawing;
using System.IO;

namespace SistemaConferenciaPedidos.Services
{
    public interface IPdfRendererService
    {
        /// <summary>
        /// Renderiza uma página específica de um PDF para Bitmap.
        /// As páginas são 1-indexed (1, 2, 3...)
        /// </summary>
        Bitmap RenderizarPagina(string caminhoPdf, int numeroPagina, int dpi = 300);
        
        /// <summary>
        /// Renderiza uma página específica de um PDF em memória para Bitmap.
        /// As páginas são 1-indexed (1, 2, 3...)
        /// </summary>
        Bitmap RenderizarPagina(MemoryStream pdfStream, int numeroPagina, int dpi = 300);
    }
}
