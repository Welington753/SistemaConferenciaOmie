using System;
using System.Drawing;
using Xunit;
using SistemaConferenciaPedidos.Services;
using SistemaConferenciaPedidos.Tests.Fakes;

namespace SistemaConferenciaPedidos.Tests.Services
{
    public class ImpressaoServiceTests
    {
        [Fact]
        public void Deve_Tratar_Fundo_Transparente_Como_Branco_No_Crop()
        {
            var fakePrinter = new FakePrinterEnvironment();
            var fakeRenderer = new FakePdfRendererService();
            var impressaoService = new TestableImpressaoService(fakePrinter, fakeRenderer);

            using var bitmap = new Bitmap(100, 100, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.FillRectangle(Brushes.Black, 40, 40, 20, 20);
            }

            using var bitmapCortado = impressaoService.CortarMargensBrancas(bitmap);

            Assert.NotNull(bitmapCortado);
            Assert.Equal(40, bitmapCortado.Width);
            Assert.Equal(40, bitmapCortado.Height);
        }

        [Fact]
        public void Deve_Cortar_Margem_Imagem_Limitada()
        {
            var fakePrinter = new FakePrinterEnvironment();
            var fakeRenderer = new FakePdfRendererService();
            var impressaoService = new TestableImpressaoService(fakePrinter, fakeRenderer);

            using var bitmap = new Bitmap(100, 100, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.FillRectangle(Brushes.Black, 40, 40, 20, 20);
            }

            using var bitmapCortado = impressaoService.CortarMargensBrancas(bitmap);

            Assert.NotNull(bitmapCortado);
            Assert.Equal(40, bitmapCortado.Width);
            Assert.Equal(40, bitmapCortado.Height);
        }

        [Fact]
        public void Deve_Retornar_Clone_Mesmo_Se_Imagem_Branca()
        {
            var fakePrinter = new FakePrinterEnvironment();
            var fakeRenderer = new FakePdfRendererService();
            var impressaoService = new TestableImpressaoService(fakePrinter, fakeRenderer);

            using var bitmap = new Bitmap(100, 100, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
            }

            using var bitmapCortado = impressaoService.CortarMargensBrancas(bitmap);

            Assert.NotNull(bitmapCortado);
            Assert.Equal(100, bitmapCortado.Width);
            Assert.Equal(100, bitmapCortado.Height);
        }

        [Fact]
        public void Deve_Aceitar_Imagem_Pequena()
        {
            var fakePrinter = new FakePrinterEnvironment();
            var fakeRenderer = new FakePdfRendererService();
            var impressaoService = new TestableImpressaoService(fakePrinter, fakeRenderer);

            using var bitmap = new Bitmap(5, 5, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.FillRectangle(Brushes.Black, 2, 2, 1, 1);
            }

            using var bitmapCortado = impressaoService.CortarMargensBrancas(bitmap);

            Assert.NotNull(bitmapCortado);
            // Com as margens de 10, ela tentará cobrir o limite da imagem
            Assert.Equal(5, bitmapCortado.Width);
            Assert.Equal(5, bitmapCortado.Height);
        }

        [Fact]
        public void Objeto_Retornado_Continua_Utilizavel()
        {
            var fakePrinter = new FakePrinterEnvironment();
            var fakeRenderer = new FakePdfRendererService();
            var impressaoService = new TestableImpressaoService(fakePrinter, fakeRenderer);

            using var bitmap = new Bitmap(50, 50, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.FillRectangle(Brushes.Red, 10, 10, 10, 10);
            }

            var bitmapCortado = impressaoService.CortarMargensBrancas(bitmap);

            // O bitmap original usado lá dentro é temporário. O retorno é um Clone independente.
            // Acessar propriedades não deve dar erro.
            Assert.Equal(30, bitmapCortado.Width);

            // Fazemos chamadas repetidas
            var outroCorte = impressaoService.CortarMargensBrancas(bitmap);
            Assert.Equal(30, outroCorte.Width);

            bitmapCortado.Dispose();
            outroCorte.Dispose();
        }
    }

    internal class TestableImpressaoService : ImpressaoService
    {
        public TestableImpressaoService(IPrinterEnvironment printerEnv, IPdfRendererService pdfRenderer)
            : base(printerEnv, pdfRenderer) { }

        public new Bitmap CortarMargensBrancas(Bitmap original)
        {
            var metodos = typeof(ImpressaoService).GetMethod("CortarMargensBrancas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Bitmap)metodos.Invoke(this, new object[] { original });
        }
    }
}
