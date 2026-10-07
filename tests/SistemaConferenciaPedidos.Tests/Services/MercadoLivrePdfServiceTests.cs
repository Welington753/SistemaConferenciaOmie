using System;
using System.Reflection;
using Xunit;
using SistemaConferenciaPedidos.Services;

namespace SistemaConferenciaPedidos.Tests.Services
{
    public class MercadoLivrePdfServiceTests
    {
        [Theory]
        [InlineData("Resumo L32XFMJDJ5JKJM5EVRB6MOBFTI", true)] // identificador alfanumérico do resumo
        [InlineData("SKU: \n SKU: \n", true)]
        [InlineData("IDENTIFICAÇÃO PRODUTOS", true)]
        [InlineData("Texto aleatorio 47798792847", false)]
        public void EhPaginaDeResumo_DeveReconhecerCorretamente(string texto, bool esperado)
        {
            var service = new MercadoLivrePdfService();
            // Invocando metodo privado/internal
            var methodInfo = typeof(MercadoLivrePdfService).GetMethod("EhPaginaDeResumo", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null)
                methodInfo = typeof(MercadoLivrePdfService).GetMethod("EhPaginaDeResumo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            bool resultado = (bool)methodInfo.Invoke(service, new object[] { texto });
            Assert.Equal(esperado, resultado);
        }

        /// <summary>
        /// Etapa 5: PDF com estrutura inválida deve ser rejeitado antes de copiar.
        /// Verifica que ValidarECopiarArquivo (via CarregarEtiquetas) rejeita PDF sem páginas válidas.
        /// </summary>
        [Fact]
        public void ValidarECopiarArquivo_PdfInvalido_Rejeita()
        {
            string pdfOrigem = System.IO.Path.GetTempFileName() + ".pdf";
            // Arquivo com cabeçalho %PDF mas conteúdo corrompido (sem páginas válidas)
            System.IO.File.WriteAllText(pdfOrigem, "%PDF-1.4\ncorrupto sem paginas");
            try
            {
                var service = new MercadoLivrePdfService();
                // Deve lançar Exception (PDF sem páginas válidas ou corrompido)
                Assert.ThrowsAny<Exception>(() => service.CarregarEtiquetas(pdfOrigem));
            }
            finally
            {
                if (System.IO.File.Exists(pdfOrigem)) System.IO.File.Delete(pdfOrigem);
            }
        }

        /// <summary>
        /// Etapa 5: arquivo que não é PDF deve ser rejeitado imediatamente.
        /// </summary>
        [Fact]
        public void ValidarECopiarArquivo_ArquivoNaoPdf_Rejeita()
        {
            string pdfOrigem = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(pdfOrigem, "Isto nao e um PDF de forma alguma");
            try
            {
                var service = new MercadoLivrePdfService();
                Assert.ThrowsAny<Exception>(() => service.CarregarEtiquetas(pdfOrigem));
            }
            finally
            {
                if (System.IO.File.Exists(pdfOrigem)) System.IO.File.Delete(pdfOrigem);
            }
        }
    }
}
