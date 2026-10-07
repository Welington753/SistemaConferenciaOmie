using System.Drawing;
using System.IO;
using PDFtoImage;
using SkiaSharp;

namespace SistemaConferenciaPedidos.Services
{
    public class PdfRendererService : IPdfRendererService
    {
        public Bitmap RenderizarPagina(string caminhoPdf, int numeroPagina, int dpi = 300)
        {
            using var inputStream = File.OpenRead(caminhoPdf);
            return RenderizarPaginaInterno(inputStream, numeroPagina, dpi);
        }

        public Bitmap RenderizarPagina(MemoryStream pdfStream, int numeroPagina, int dpi = 300)
        {
            return RenderizarPaginaInterno(pdfStream, numeroPagina, dpi);
        }

        private Bitmap RenderizarPaginaInterno(Stream stream, int numeroPagina, int dpi)
        {
            // PDFtoImage geralmente usa 0-indexed para o array, e as páginas na aplicação começam em 1
            if (numeroPagina < 1) return null;

            var options = new RenderOptions { Dpi = dpi };
            using var skBitmap = Conversion.ToImage(stream, page: numeroPagina - 1, options: options);
            if (skBitmap == null) return null;

            using var image = SKImage.FromBitmap(skBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            var ms = new MemoryStream();
            data.SaveTo(ms);
            ms.Position = 0;

            var originalBmp = new Bitmap(ms);
            var whiteBmp = new Bitmap(originalBmp.Width, originalBmp.Height);
            using (var g = Graphics.FromImage(whiteBmp))
            {
                g.Clear(Color.White);
                g.DrawImage(originalBmp, 0, 0);
            }
            originalBmp.Dispose();

            return whiteBmp;
        }
    }
}
