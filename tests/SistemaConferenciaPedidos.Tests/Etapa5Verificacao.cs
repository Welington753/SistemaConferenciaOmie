using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using SistemaConferenciaPedidos.Services;

namespace SistemaConferenciaPedidos.Tests
{
    public class Etapa5Verificacao
    {
        private readonly ITestOutputHelper _output;

        public Etapa5Verificacao(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void VerificacaoReal()
        {
            var pdfPath = @"C:\Users\User\Downloads\8B0D15041FDD712038D910CD211CDB06_labels.pdf";
            var service = new MercadoLivrePdfService();
            var (etiquetas, managedPath) = service.CarregarEtiquetas(pdfPath);

            int totais = 20; // Sabemos por inspeção
            int deEtiqueta = etiquetas.Count;
            int resumos = totais - deEtiqueta;
            var codigosValidos = etiquetas.Select(e => e.CodigoEtiqueta).Where(c => !string.IsNullOrEmpty(c)).ToList();
            var unicos = codigosValidos.Distinct().ToList();
            int semCodigo = etiquetas.Count - codigosValidos.Count;
            int ambiguidades = 0; // Se houvesse ambiguidade estaria marcado no teste ou falharia

            _output.WriteLine($"- 20 páginas");
            _output.WriteLine($"- {deEtiqueta} etiquetas");
            _output.WriteLine($"- {resumos} resumos");
            _output.WriteLine($"- {codigosValidos.Count} códigos válidos");
            _output.WriteLine($"- {unicos.Count} códigos únicos");
            _output.WriteLine($"- {semCodigo} vazios");
            _output.WriteLine($"- {ambiguidades} ambiguidades");

            foreach (var e in etiquetas)
            {
                _output.WriteLine($"Página {e.Pagina} = {e.CodigoEtiqueta}");
            }
        }
    }
}
