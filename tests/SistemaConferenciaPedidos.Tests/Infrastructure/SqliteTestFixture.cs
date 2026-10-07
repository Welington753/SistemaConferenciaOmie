using System;
using System.IO;
using SistemaConferenciaPedidos.Data;

namespace SistemaConferenciaPedidos.Tests.Infrastructure
{
    public class SqliteTestFixture : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly string _tempDatabasePath;
        private readonly string _previousDatabasePath;
        
        public string ExpectedDatabasePath => _tempDatabasePath;

        public SqliteTestFixture() : this(null)
        {
        }

        internal SqliteTestFixture(string forceDatabasePath)
        {
            _previousDatabasePath = Database.CaminhoBanco;
            try
            {
                var rootTemp = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "SistemaConferenciaPedidos.Tests"));
                
                if (string.IsNullOrEmpty(forceDatabasePath))
                {
                    _tempDirectory = Path.GetFullPath(Path.Combine(rootTemp, Guid.NewGuid().ToString()));
                    _tempDatabasePath = Path.GetFullPath(Path.Combine(_tempDirectory, "test.db"));
                }
                else
                {
                    _tempDatabasePath = Path.GetFullPath(forceDatabasePath);
                    _tempDirectory = Path.GetDirectoryName(_tempDatabasePath);
                }
                
                var relativePath = Path.GetRelativePath(rootTemp, _tempDirectory);
                if (relativePath.StartsWith("..") || Path.IsPathRooted(relativePath))
                {
                    throw new InvalidOperationException("Tentativa de escape de diretÃ³rio raiz dos testes detectada.");
                }

                Directory.CreateDirectory(_tempDirectory);
                
                Database.CaminhoBanco = _tempDatabasePath;
                Database.Inicializar();
            }
            catch
            {
                Database.CaminhoBanco = _previousDatabasePath;
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                
                if (Database.CaminhoBanco != _tempDatabasePath)
                {
                    throw new InvalidOperationException("O banco de dados foi alterado durante o teste.");
                }

                var rootTemp = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "SistemaConferenciaPedidos.Tests"));
                if (!string.IsNullOrEmpty(_tempDirectory))
                {
                    var relativePath = Path.GetRelativePath(rootTemp, _tempDirectory);
                    if (relativePath.StartsWith("..") || Path.IsPathRooted(relativePath))
                    {
                        throw new InvalidOperationException("Falha de seguranÃ§a: Tentativa de apagar diretÃ³rio fora da raiz de testes.");
                    }
                    
                    if (File.Exists(_tempDatabasePath))
                    {
                        File.Delete(_tempDatabasePath);
                    }
                    if (Directory.Exists(_tempDirectory))
                    {
                        Directory.Delete(_tempDirectory, true);
                    }
                }
            }
            finally
            {
                Database.CaminhoBanco = _previousDatabasePath;
            }
        }
    }
}
