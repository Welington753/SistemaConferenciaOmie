using System.Drawing;
using SistemaConferenciaPedidos.Services;

namespace SistemaConferenciaPedidos.Tests.Fakes
{
    internal sealed class FakePdfRendererService : IPdfRendererService
    {
        public Bitmap RenderizarPagina(string caminhoPdf, int numeroPagina, int dpi) => null;
        public Bitmap RenderizarPagina(System.IO.Stream streamPdf, int numeroPagina, int dpi) => null;
        public Bitmap RenderizarPagina(System.IO.MemoryStream streamPdf, int numeroPagina, int dpi) => null;
    }
}
