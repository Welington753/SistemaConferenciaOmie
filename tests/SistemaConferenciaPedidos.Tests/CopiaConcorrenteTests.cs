using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SistemaConferenciaPedidos.Services;

namespace SistemaConferenciaPedidos.Tests
{
    public class CopiaConcorrenteTests : IDisposable
    {
        private readonly string _testDir;
        private readonly string _sourcePdf;
        private readonly string _managedPath;

        public CopiaConcorrenteTests()
        {
            _testDir = Path.Combine(
                Path.GetTempPath(),
                "SistemaConferenciaPedidos.Tests",
                "MercadoLivrePdf",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(_testDir);

            // Generate a synthetic valid PDF in memory using PdfSharp to avoid absolute paths to local files
            _sourcePdf = Path.Combine(_testDir, "orig.pdf");
            using (var doc = new PdfSharp.Pdf.PdfDocument())
            {
                doc.AddPage();
                doc.Save(_sourcePdf);
            }

            string hashOrig = CalcularSha256(_sourcePdf);
            _managedPath = Path.Combine(_testDir, $"{hashOrig}.pdf");
        }

        public void Dispose()
        {
            // Clean up exclusive root only
            if (Directory.Exists(_testDir))
            {
                var fullPath = Path.GetFullPath(_testDir);
                if (fullPath.StartsWith(Path.GetTempPath()))
                {
                    try { Directory.Delete(_testDir, true); } catch { }
                }
            }
        }

        [Fact]
        public async Task CargaEtiquetas_Concorrente_Mesmo_PDF_Deve_Ser_Atomico()
        {
            // Instantiate using internal constructor with the isolated path
            var service1 = new MercadoLivrePdfService(_testDir);
            var service2 = new MercadoLivrePdfService(_testDir);

            // Run concurrently
            var t1 = Task.Run(() => service1.CarregarEtiquetas(_sourcePdf));
            var t2 = Task.Run(() => service2.CarregarEtiquetas(_sourcePdf));

            var results = await Task.WhenAll(t1, t2);

            var path1 = results[0].CaminhoGerenciado;
            var path2 = results[1].CaminhoGerenciado;

            // both return the same definitive path
            Assert.Equal(path1, path2);

            // caminho definitivo abaixo dessa raiz temporária
            Assert.StartsWith(_testDir, path1);

            // nenhum caminho retornado contém LocalApplicationData operacional
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string realEtiquetasDir = Path.Combine(appData, "SistemaConferenciaPedidos", "Etiquetas");
            Assert.DoesNotContain(realEtiquetasDir, path1, StringComparison.OrdinalIgnoreCase);

            // nenhum arquivo é criado no AppData
            string hashOrig = CalcularSha256(_sourcePdf);
            string wrongPath = Path.Combine(appData, "SistemaConferenciaPedidos", "Etiquetas", "MercadoLivre", $"{hashOrig}.pdf");
            Assert.False(File.Exists(wrongPath), "Não deve criar arquivo na pasta operacional!");

            // definitive exists
            Assert.True(File.Exists(path1));

            // SHA-256 is equal to original
            string hashDef = CalcularSha256(path1);
            Assert.Equal(hashOrig, hashDef);

            // exactly one definitive PDF exists
            Assert.Equal(_managedPath, path1);

            // no .tmp files left over for THIS hash
            var destDir = Path.GetDirectoryName(path1);
            var tmpFiles = Directory.GetFiles(destDir, $"{hashOrig}*.tmp");
            Assert.Empty(tmpFiles);
        }

        [Fact]
        public void IOException_Desconhecida_Nao_Eh_Engolida()
        {
            var service = new MercadoLivrePdfService(_testDir);

            // Block the source file so it throws an IOException (access denied) when reading
            using var fs = new FileStream(_sourcePdf, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            var ex = Assert.Throws<IOException>(() => service.CarregarEtiquetas(_sourcePdf));
            Assert.NotNull(ex);
        }

        [Fact]
        public void Destino_Invalido_Nao_Aceito_Como_Sucesso()
        {
            var service = new MercadoLivrePdfService(_testDir);

            // Create invalid definitive file (empty)
            File.WriteAllText(_managedPath, "invalid pdf content");

            // Now block it with FileShare.Read so the service catches the move collision, reads the invalid file, and rejects it
            using (var fs = new FileStream(_managedPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var ex = Assert.Throws<InvalidOperationException>(() => service.CarregarEtiquetas(_sourcePdf));
                Assert.Contains("está corrompido", ex.Message);
            }
        }

        private static string CalcularSha256(string caminhoArquivo)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            using var fs = new FileStream(caminhoArquivo, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] hashBytes = sha256.ComputeHash(fs);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
