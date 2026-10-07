using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace SistemaConferenciaPedidos.Services
{
    public class PdfPigTextProvider : IPdfTextProvider
    {
        public string ExtrairTexto(string caminhoPdf, int pagina)
        {
            using var document = PdfDocument.Open(caminhoPdf);
            if (pagina < 1 || pagina > document.NumberOfPages)
                return null; // ou throw

            var page = document.GetPage(pagina);
            return ContentOrderTextExtractor.GetText(page, true) ?? "";
        }
    }
}
