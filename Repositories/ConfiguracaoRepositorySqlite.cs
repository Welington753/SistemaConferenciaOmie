using Microsoft.Data.Sqlite;
using SistemaConferenciaPedidos.Data;
using System;

namespace SistemaConferenciaPedidos.Repositories
{
    public class ConfiguracaoRepositorySqlite
    {
        public string ObterValor(string chave, string valorPadrao = "")
        {
            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Valor FROM Configuracoes WHERE Chave = $Chave";
            command.Parameters.AddWithValue("$Chave", chave);

            var resultado = command.ExecuteScalar();
            if (resultado != null && resultado != DBNull.Value)
            {
                return resultado.ToString();
            }

            return valorPadrao;
        }

        public void SalvarValor(string chave, string valor)
        {
            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Configuracoes (Chave, Valor) 
                VALUES ($Chave, $Valor)
                ON CONFLICT(Chave) DO UPDATE SET Valor = excluded.Valor;";

            command.Parameters.AddWithValue("$Chave", chave);
            command.Parameters.AddWithValue("$Valor", valor);

            command.ExecuteNonQuery();
        }
    }
}
