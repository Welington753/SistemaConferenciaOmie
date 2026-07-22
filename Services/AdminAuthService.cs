using System;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using SistemaConferenciaPedidos.Data;

namespace SistemaConferenciaPedidos.Services
{
    public sealed class AdminAuthService
    {
        private const string ChaveHash = "SenhaAdminHash";
        private const string ChaveSalt = "SenhaAdminSalt";
        private const int Iteracoes = 210_000;
        private const int TamanhoHash = 32;

        public bool SenhaConfigurada()
        {
            return !string.IsNullOrWhiteSpace(LerConfiguracao(ChaveHash)) &&
                   !string.IsNullOrWhiteSpace(LerConfiguracao(ChaveSalt));
        }

        public void ConfigurarSenha(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6)
                throw new ArgumentException("A senha administrativa deve ter pelo menos 6 caracteres.");

            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = GerarHash(senha, salt);

            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            SalvarConfiguracao(connection, transaction, ChaveSalt, Convert.ToBase64String(salt));
            SalvarConfiguracao(connection, transaction, ChaveHash, Convert.ToBase64String(hash));

            transaction.Commit();
        }

        public bool ValidarSenha(string senha)
        {
            if (string.IsNullOrEmpty(senha))
                return false;

            string hashSalvo = LerConfiguracao(ChaveHash);
            string saltSalvo = LerConfiguracao(ChaveSalt);

            if (string.IsNullOrWhiteSpace(hashSalvo) || string.IsNullOrWhiteSpace(saltSalvo))
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(saltSalvo);
                byte[] esperado = Convert.FromBase64String(hashSalvo);
                byte[] atual = GerarHash(senha, salt);

                return CryptographicOperations.FixedTimeEquals(atual, esperado);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public void RegistrarAcao(string acao, string numeroPedidoCliente = "", string detalhes = "")
        {
            // Instancia o repositório para evitar duplicação de lógica de banco, 
            // ou escreve direto se preferir. Aqui usamos a lógica centralizada no repositório.
            var repo = new Repositories.PedidoRepositorySqlite();
            repo.RegistrarAuditoria(acao, numeroPedidoCliente, detalhes);
        }

        private static byte[] GerarHash(string senha, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                senha,
                salt,
                Iteracoes,
                HashAlgorithmName.SHA256,
                TamanhoHash);
        }

        private static string LerConfiguracao(string chave)
        {
            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Valor FROM Configuracoes WHERE Chave = $Chave LIMIT 1";
            command.Parameters.AddWithValue("$Chave", chave);

            return command.ExecuteScalar()?.ToString() ?? "";
        }

        private static void SalvarConfiguracao(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string chave,
            string valor)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
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
