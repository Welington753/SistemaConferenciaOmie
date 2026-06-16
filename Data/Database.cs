using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace SistemaConferenciaPedidos.Data
{
    public static class Database
    {
        private static readonly string PastaDados =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        private static readonly string CaminhoBanco =
            Path.Combine(PastaDados, "sistema_conferencia.db");

        public static string ConnectionString =>
            $"Data Source={CaminhoBanco}";

        public static void Inicializar()
        {
            if (!Directory.Exists(PastaDados))
                Directory.CreateDirectory(PastaDados);

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
CREATE TABLE IF NOT EXISTS Pedidos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NumeroPedidoCliente TEXT NOT NULL UNIQUE,
    NomeCliente TEXT,
    Marketplace TEXT,
    CodigoEtiqueta TEXT,
    Status TEXT,
    JsonItens TEXT,
    EtiquetaMarketplaceZpl TEXT,
    Impresso INTEGER NOT NULL DEFAULT 0,
    Conferido INTEGER NOT NULL DEFAULT 0,
    DataCriacao TEXT,
    DataConferencia TEXT
);";

            command.ExecuteNonQuery();
            AdicionarColunaSeNaoExistir(connection, "Pedidos", "DataConferencia", "TEXT");

        }

        private static void AdicionarColunaSeNaoExistir(
    SqliteConnection connection,
    string tabela,
    string coluna,
    string tipo)
        {
            using var verificar = connection.CreateCommand();
            verificar.CommandText = $"PRAGMA table_info({tabela});";

            bool existe = false;

            using (var reader = verificar.ExecuteReader())
            {
                while (reader.Read())
                {
                    string nomeColuna = reader["name"]?.ToString();

                    if (string.Equals(nomeColuna, coluna, StringComparison.OrdinalIgnoreCase))
                    {
                        existe = true;
                        break;
                    }
                }
            }

            if (existe)
                return;

            using var alterar = connection.CreateCommand();
            alterar.CommandText = $"ALTER TABLE {tabela} ADD COLUMN {coluna} {tipo};";
            alterar.ExecuteNonQuery();
        }
    }
}