using Xunit;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using SistemaConferenciaPedidos.Data;

namespace SistemaConferenciaPedidos.Tests
{
    public class SqliteIntegrityTests : IDisposable
    {
        private readonly string _tempDbPath;

        public SqliteIntegrityTests()
        {
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"SistemaConferenciaPedidos_Test_{Guid.NewGuid()}.db");
            Database.CaminhoBanco = _tempDbPath;
        }

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(_tempDbPath))
            {
                try
                {
                    File.Delete(_tempDbPath);
                }
                catch { } // Ignore delete errors in tests
            }
        }

        [Fact]
        public void SqliteNativo_VersaoCarregada_EhPeloMenos_3_50_2()
        {
            using var connection = new SqliteConnection($"Data Source={_tempDbPath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT sqlite_version();";
            var result = command.ExecuteScalar()?.ToString();

            Assert.NotNull(result);

            // SQLite version is typically like "3.50.2", "3.51.0"
            // We only take the numeric parts to parse correctly
            if (Version.TryParse(result, out var versionCarregada))
            {
                var versaoMinima = new Version(3, 50, 2);
                Assert.True(versionCarregada >= versaoMinima, $"A versão carregada do SQLite é {versionCarregada}, mas é esperada pelo menos 3.50.2.");
            }
            else
            {
                Assert.Fail($"Não foi possível converter a versão '{result}' para System.Version.");
            }
        }

        [Fact]
        public void AtualizacaoSqlite_AbreBancoExistenteEPreservaPedidos()
        {
            // 1. Criar um banco temporário representando o esquema atual
            Database.Inicializar(); // Usa o código real da aplicação para criar esquema
            
            // 2. Inserir pelo menos um pedido completo
            using (var conn = new SqliteConnection($"Data Source={_tempDbPath}"))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Pedidos (NumeroPedidoCliente, NomeCliente, Marketplace, JsonItens, EtiquetaMarketplaceZpl, Status) 
                    VALUES ('TESTE-123', 'Cliente Teste', 'Shopee', '{}', '', 'Importado');";
                cmd.ExecuteNonQuery();
            }

            // 3. Fechar todas as conexões
            SqliteConnection.ClearAllPools();

            // 4. Reabrir através do código real da aplicação
            // 5. Executar Database.Inicializar()
            Database.Inicializar(); // Testa se o esquema atual sobrevive a uma re-inicialização sem apagar dados

            // 6. Confirmar que o pedido permanece existente e sem alteração
            // 7. Confirmar que as tabelas atuais continuam acessíveis
            using (var conn = new SqliteConnection($"Data Source={_tempDbPath}"))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT NumeroPedidoCliente, NomeCliente FROM Pedidos WHERE NumeroPedidoCliente = 'TESTE-123';";
                using var reader = cmd.ExecuteReader();
                Assert.True(reader.Read(), "O pedido não foi encontrado no banco.");
                
                var numeroPedido = reader.GetString(0);
                var nomeCliente = reader.GetString(1);
                
                Assert.Equal("TESTE-123", numeroPedido);
                Assert.Equal("Cliente Teste", nomeCliente);
            }
            
            // 8. Limpar pools e excluir apenas a pasta temporária autorizada (feito no Dispose)
        }
    }
}
