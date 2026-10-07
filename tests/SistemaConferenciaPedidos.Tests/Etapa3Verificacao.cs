using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using SistemaConferenciaPedidos.Services;

namespace SistemaConferenciaPedidos.Tests
{
    public class Etapa3Verificacao
    {
        private readonly ITestOutputHelper _output;

        public Etapa3Verificacao(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Executar5Vezes()
        {
            var sourcePdf = @"C:\Users\User\Downloads\8B0D15041FDD712038D910CD211CDB06_labels.pdf";
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string destDir = Path.Combine(appData, "SistemaConferenciaPedidos", "Etiquetas", "MercadoLivre");
            Directory.CreateDirectory(destDir);

            // Calcula o hash real
            string hashString;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            using (var fs = new FileStream(sourcePdf, FileMode.Open, FileAccess.Read))
            {
                var h = sha.ComputeHash(fs);
                hashString = BitConverter.ToString(h).Replace("-", "").ToLowerInvariant();
            }
            
            for (int i = 1; i <= 5; i++)
            {
                _output.WriteLine($"=== Execução {i} ===");
                var sw = System.Diagnostics.Stopwatch.StartNew();
                
                string managedPath = Path.Combine(destDir, $"{hashString}.pdf");
                if (File.Exists(managedPath)) File.Delete(managedPath);
                foreach (var tmp in Directory.GetFiles(destDir, "*.tmp")) File.Delete(tmp);

                var service1 = new MercadoLivrePdfService();
                var service2 = new MercadoLivrePdfService();

                var t1 = Task.Run(() => service1.CarregarEtiquetas(sourcePdf));
                var t2 = Task.Run(() => service2.CarregarEtiquetas(sourcePdf));

                bool aprovado = true;
                string caminhoFinal = "";
                try
                {
                    var results = await Task.WhenAll(t1, t2);
                    if (results[0].CaminhoGerenciado != results[1].CaminhoGerenciado) aprovado = false;
                    caminhoFinal = results[0].CaminhoGerenciado;
                }
                catch (Exception ex)
                {
                    aprovado = false;
                    _output.WriteLine("Exceção: " + ex.Message);
                }

                sw.Stop();
                
                int pdfs = Directory.GetFiles(destDir, $"{hashString}.pdf").Length;
                int tmps = Directory.GetFiles(destDir, $"{hashString}*.tmp").Length;

                _output.WriteLine($"- aprovado/falhou: {(aprovado ? "aprovado" : "falhou")}");
                _output.WriteLine($"- duração: {sw.ElapsedMilliseconds} ms");
                _output.WriteLine($"- caminho final: {caminhoFinal}");
                _output.WriteLine($"- quantidade de PDFs definitivos: {pdfs}");
                _output.WriteLine($"- quantidade de temporários restantes: {tmps}");
                
                Assert.True(aprovado, $"Falha na execução {i}");
            }
        }
    }
}
