using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace SistemaConferenciaPedidos.Data
{
    public static class Database
    {
        private static readonly string PastaDados =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        public static string CaminhoBanco =
            Path.Combine(PastaDados, "sistema_conferencia.db");

        public static string ConnectionString =>
            $"Data Source={CaminhoBanco}";

        public static void Inicializar()
        {
            if (!Directory.Exists(PastaDados))
                Directory.CreateDirectory(PastaDados);

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var pragmaCmd = connection.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;";
            pragmaCmd.ExecuteNonQuery();

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
    DataAtualizacao TEXT,
    DataConferencia TEXT,
    DataPrevisao TEXT,
    Oculto INTEGER NOT NULL DEFAULT 0,
    DataOcultacao TEXT,
    CaminhoZipImportacao TEXT,
    NomePdfNoZip TEXT,
    PaginaPdf INTEGER,
    DataPrimeiraImpressao TEXT,
    DataReimpressao TEXT,
    MotivoReimpressao TEXT
);";

            command.ExecuteNonQuery();
            AdicionarColunaSeNaoExistir(connection, "Pedidos", "DataAtualizacao", "TEXT");
            AdicionarColunaSeNaoExistir(connection, "Pedidos", "DataConferencia", "TEXT");
            AdicionarColunaSeNaoExistir(connection, "Pedidos", "DataPrevisao", "TEXT");
            AdicionarColunaSeNaoExistir(connection, "Pedidos", "Oculto", "INTEGER NOT NULL DEFAULT 0");
            AdicionarColunaSeNaoExistir(connection, "Pedidos", "DataOcultacao", "TEXT");
            AdicionarColunaSeNaoExistir(connection, "Pedidos", "CaminhoZipImportacao", "TEXT");
            AdicionarColunaSeNaoExistir(connection, "Pedidos", "NomePdfNoZip", "TEXT");
            AdicionarColunaSeNaoExistir(connection, "Pedidos", "PaginaPdf", "INTEGER");
            AdicionarColunaSeNaoExistir(connection, "Pedidos", "DataPrimeiraImpressao", "TEXT");
            AdicionarColunaSeNaoExistir(connection, "Pedidos", "DataReimpressao", "TEXT");
            AdicionarColunaSeNaoExistir(connection, "Pedidos", "MotivoReimpressao", "TEXT");

            using var configuracoes = connection.CreateCommand();
            configuracoes.CommandText = @"
CREATE TABLE IF NOT EXISTS Configuracoes (
    Chave TEXT PRIMARY KEY,
    Valor TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS AuditoriaAdministrativa (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Acao TEXT NOT NULL,
    NumeroPedidoCliente TEXT,
    DataAcao TEXT NOT NULL
);";
            configuracoes.ExecuteNonQuery();

            using var indicesCmd = connection.CreateCommand();
            indicesCmd.CommandText = @"
CREATE INDEX IF NOT EXISTS idx_pedidos_numeropedidocliente ON Pedidos (NumeroPedidoCliente);
CREATE INDEX IF NOT EXISTS idx_pedidos_marketplace ON Pedidos (Marketplace);
CREATE INDEX IF NOT EXISTS idx_pedidos_codigoetiqueta ON Pedidos (CodigoEtiqueta);
CREATE INDEX IF NOT EXISTS idx_pedidos_impresso ON Pedidos (Impresso);
CREATE INDEX IF NOT EXISTS idx_pedidos_dataconferencia ON Pedidos (DataConferencia);
CREATE INDEX IF NOT EXISTS idx_pedidos_dataprevisao ON Pedidos (DataPrevisao);
CREATE INDEX IF NOT EXISTS idx_pedidos_oculto ON Pedidos (Oculto);";
            indicesCmd.ExecuteNonQuery();

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