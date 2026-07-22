using Microsoft.Data.Sqlite;
using SistemaConferenciaPedidos.Data;
using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaConferenciaPedidos.Repositories
{
    public class PedidoRepositorySqlite : IPedidoRepository
    {
        public List<PedidoConferencia> ObterTodos()
        {
            var lista = new List<PedidoConferencia>();

            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
SELECT
    Id,
    NumeroPedidoCliente,
    NomeCliente,
    Marketplace,
    CodigoEtiqueta,
    Status,
    JsonItens,
    EtiquetaMarketplaceZpl,
    Impresso,
    Conferido,
    DataCriacao,
    DataAtualizacao,
    DataConferencia,
    DataPrevisao,
    CaminhoZipImportacao,
    NomePdfNoZip,
    PaginaPdf,
    DataPrimeiraImpressao,
    DataReimpressao,
    MotivoReimpressao,
    Oculto
FROM Pedidos
WHERE IFNULL(Oculto, 0) = 0";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new PedidoConferencia
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    NumeroPedidoCliente = reader["NumeroPedidoCliente"]?.ToString(),
                    NomeCliente = reader["NomeCliente"]?.ToString(),
                    Marketplace = reader["Marketplace"]?.ToString(),
                    CodigoEtiqueta = reader["CodigoEtiqueta"]?.ToString(),
                    Status = reader["Status"]?.ToString(),
                    JsonItens = reader["JsonItens"]?.ToString(),
                    EtiquetaMarketplaceZpl = reader["EtiquetaMarketplaceZpl"]?.ToString(),
                    Impresso = Convert.ToInt32(reader["Impresso"]) == 1,
                    Conferido = Convert.ToInt32(reader["Conferido"]) == 1,
                    DataCriacao = DataHelper.DesserializarDataObrigatoria(reader["DataCriacao"]?.ToString()),
                    DataAtualizacao = DataHelper.DesserializarData(reader["DataAtualizacao"]?.ToString()),
                    DataConferencia = DataHelper.DesserializarData(reader["DataConferencia"]?.ToString()),
                    DataPrevisao = DataHelper.DesserializarData(reader["DataPrevisao"]?.ToString()),
                    CaminhoZipImportacao = reader["CaminhoZipImportacao"]?.ToString() ?? "",
                    NomePdfNoZip = reader["NomePdfNoZip"]?.ToString() ?? "",
                    PaginaPdf = reader["PaginaPdf"] != DBNull.Value ? Convert.ToInt32(reader["PaginaPdf"]) : (int?)null,
                    DataPrimeiraImpressao = DataHelper.DesserializarData(reader["DataPrimeiraImpressao"]?.ToString()),
                    DataReimpressao = DataHelper.DesserializarData(reader["DataReimpressao"]?.ToString()),
                    MotivoReimpressao = reader["MotivoReimpressao"]?.ToString() ?? "",
                    Oculto = reader["Oculto"] != DBNull.Value && Convert.ToInt32(reader["Oculto"]) == 1
                });
            }

            return lista;
        }

        public PedidoConferencia ObterPorId(int id)
        {
            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
SELECT Id, NumeroPedidoCliente, NomeCliente, Marketplace, CodigoEtiqueta, Status, JsonItens, EtiquetaMarketplaceZpl, Impresso, Conferido, DataCriacao, DataAtualizacao, DataConferencia, DataPrevisao, CaminhoZipImportacao, NomePdfNoZip, PaginaPdf, DataPrimeiraImpressao, DataReimpressao, MotivoReimpressao, Oculto
FROM Pedidos WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new PedidoConferencia
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    NumeroPedidoCliente = reader["NumeroPedidoCliente"]?.ToString(),
                    NomeCliente = reader["NomeCliente"]?.ToString(),
                    Marketplace = reader["Marketplace"]?.ToString(),
                    CodigoEtiqueta = reader["CodigoEtiqueta"]?.ToString(),
                    Status = reader["Status"]?.ToString(),
                    JsonItens = reader["JsonItens"]?.ToString(),
                    EtiquetaMarketplaceZpl = reader["EtiquetaMarketplaceZpl"]?.ToString(),
                    Impresso = Convert.ToInt32(reader["Impresso"]) == 1,
                    Conferido = Convert.ToInt32(reader["Conferido"]) == 1,
                    DataCriacao = DataHelper.DesserializarDataObrigatoria(reader["DataCriacao"]?.ToString()),
                    DataAtualizacao = DataHelper.DesserializarData(reader["DataAtualizacao"]?.ToString()),
                    DataConferencia = DataHelper.DesserializarData(reader["DataConferencia"]?.ToString()),
                    DataPrevisao = DataHelper.DesserializarData(reader["DataPrevisao"]?.ToString()),
                    CaminhoZipImportacao = reader["CaminhoZipImportacao"]?.ToString() ?? "",
                    NomePdfNoZip = reader["NomePdfNoZip"]?.ToString() ?? "",
                    PaginaPdf = reader["PaginaPdf"] != DBNull.Value ? Convert.ToInt32(reader["PaginaPdf"]) : (int?)null,
                    DataPrimeiraImpressao = DataHelper.DesserializarData(reader["DataPrimeiraImpressao"]?.ToString()),
                    DataReimpressao = DataHelper.DesserializarData(reader["DataReimpressao"]?.ToString()),
                    MotivoReimpressao = reader["MotivoReimpressao"]?.ToString() ?? "",
                    Oculto = reader["Oculto"] != DBNull.Value && Convert.ToInt32(reader["Oculto"]) == 1
                };
            }
            return null;
        }

        public PedidoConferencia ObterPorNumero(string numeroPedido)
        {
            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
SELECT Id, NumeroPedidoCliente, NomeCliente, Marketplace, CodigoEtiqueta, Status, JsonItens, EtiquetaMarketplaceZpl, Impresso, Conferido, DataCriacao, DataAtualizacao, DataConferencia, DataPrevisao, CaminhoZipImportacao, NomePdfNoZip, PaginaPdf, DataPrimeiraImpressao, DataReimpressao, MotivoReimpressao, Oculto
FROM Pedidos WHERE NumeroPedidoCliente = @NumeroPedidoCliente";
            command.Parameters.AddWithValue("@NumeroPedidoCliente", numeroPedido ?? "");
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new PedidoConferencia
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    NumeroPedidoCliente = reader["NumeroPedidoCliente"]?.ToString(),
                    NomeCliente = reader["NomeCliente"]?.ToString(),
                    Marketplace = reader["Marketplace"]?.ToString(),
                    CodigoEtiqueta = reader["CodigoEtiqueta"]?.ToString(),
                    Status = reader["Status"]?.ToString(),
                    JsonItens = reader["JsonItens"]?.ToString(),
                    EtiquetaMarketplaceZpl = reader["EtiquetaMarketplaceZpl"]?.ToString(),
                    Impresso = Convert.ToInt32(reader["Impresso"]) == 1,
                    Conferido = Convert.ToInt32(reader["Conferido"]) == 1,
                    DataCriacao = DataHelper.DesserializarDataObrigatoria(reader["DataCriacao"]?.ToString()),
                    DataAtualizacao = DataHelper.DesserializarData(reader["DataAtualizacao"]?.ToString()),
                    DataConferencia = DataHelper.DesserializarData(reader["DataConferencia"]?.ToString()),
                    DataPrevisao = DataHelper.DesserializarData(reader["DataPrevisao"]?.ToString()),
                    CaminhoZipImportacao = reader["CaminhoZipImportacao"]?.ToString() ?? "",
                    NomePdfNoZip = reader["NomePdfNoZip"]?.ToString() ?? "",
                    PaginaPdf = reader["PaginaPdf"] != DBNull.Value ? Convert.ToInt32(reader["PaginaPdf"]) : (int?)null,
                    DataPrimeiraImpressao = DataHelper.DesserializarData(reader["DataPrimeiraImpressao"]?.ToString()),
                    DataReimpressao = DataHelper.DesserializarData(reader["DataReimpressao"]?.ToString()),
                    MotivoReimpressao = reader["MotivoReimpressao"]?.ToString() ?? "",
                    Oculto = reader["Oculto"] != DBNull.Value && Convert.ToInt32(reader["Oculto"]) == 1
                };
            }
            return null;
        }

        public List<PedidoConferencia> ObterPorPeriodo(DateTime inicio, DateTime fimExclusivo, bool incluirOcultos = false)
        {
            var lista = new List<PedidoConferencia>();

            string inicioStr = inicio.ToString("yyyy-MM-dd");
            string fimStr = fimExclusivo.ToString("yyyy-MM-dd");

            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = @"
SELECT
    Id,
    NumeroPedidoCliente,
    NomeCliente,
    Marketplace,
    CodigoEtiqueta,
    Status,
    JsonItens,
    EtiquetaMarketplaceZpl,
    Impresso,
    Conferido,
    DataCriacao,
    DataAtualizacao,
    DataConferencia,
    DataPrevisao,
    CaminhoZipImportacao,
    NomePdfNoZip,
    PaginaPdf,
    DataPrimeiraImpressao,
    DataReimpressao,
    MotivoReimpressao,
    Oculto
FROM Pedidos
WHERE (@IncluirOcultos = 1 OR IFNULL(Oculto, 0) = 0)
  AND (
    (DataPrevisao IS NOT NULL AND DataPrevisao != ''
        AND date(DataPrevisao) >= date(@Inicio) AND date(DataPrevisao) < date(@FimExclusivo))
    OR
    (IFNULL(DataPrevisao, '') = ''
        AND date(DataCriacao) >= date(@Inicio) AND date(DataCriacao) < date(@FimExclusivo))
  )
ORDER BY COALESCE(NULLIF(DataPrevisao,''), DataCriacao), NumeroPedidoCliente";

            command.Parameters.AddWithValue("@Inicio", inicioStr);
            command.Parameters.AddWithValue("@FimExclusivo", fimStr);
            command.Parameters.AddWithValue("@IncluirOcultos", incluirOcultos ? 1 : 0);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new PedidoConferencia
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    NumeroPedidoCliente = reader["NumeroPedidoCliente"]?.ToString(),
                    NomeCliente = reader["NomeCliente"]?.ToString(),
                    Marketplace = reader["Marketplace"]?.ToString(),
                    CodigoEtiqueta = reader["CodigoEtiqueta"]?.ToString(),
                    Status = reader["Status"]?.ToString(),
                    JsonItens = reader["JsonItens"]?.ToString(),
                    EtiquetaMarketplaceZpl = reader["EtiquetaMarketplaceZpl"]?.ToString(),
                    Impresso = Convert.ToInt32(reader["Impresso"]) == 1,
                    Conferido = Convert.ToInt32(reader["Conferido"]) == 1,
                    DataCriacao = DataHelper.DesserializarDataObrigatoria(reader["DataCriacao"]?.ToString()),
                    DataAtualizacao = DataHelper.DesserializarData(reader["DataAtualizacao"]?.ToString()),
                    DataConferencia = DataHelper.DesserializarData(reader["DataConferencia"]?.ToString()),
                    DataPrevisao = DataHelper.DesserializarData(reader["DataPrevisao"]?.ToString()),
                    CaminhoZipImportacao = reader["CaminhoZipImportacao"]?.ToString() ?? "",
                    NomePdfNoZip = reader["NomePdfNoZip"]?.ToString() ?? "",
                    PaginaPdf = reader["PaginaPdf"] != DBNull.Value ? Convert.ToInt32(reader["PaginaPdf"]) : (int?)null,
                    DataPrimeiraImpressao = DataHelper.DesserializarData(reader["DataPrimeiraImpressao"]?.ToString()),
                    DataReimpressao = DataHelper.DesserializarData(reader["DataReimpressao"]?.ToString()),
                    MotivoReimpressao = reader["MotivoReimpressao"]?.ToString() ?? "",
                    Oculto = reader["Oculto"] != DBNull.Value && Convert.ToInt32(reader["Oculto"]) == 1
                });
            }

            return lista;
        }

        private void ExecutarUpsert(SqliteConnection connection, SqliteTransaction transaction, PedidoConferencia pedido)
        {
            var command = connection.CreateCommand();
            if (transaction != null)
                command.Transaction = transaction;

            command.CommandText = @"
INSERT INTO Pedidos (
    NumeroPedidoCliente,
    NomeCliente,
    Marketplace,
    CodigoEtiqueta,
    Status,
    JsonItens,
    EtiquetaMarketplaceZpl,
    Impresso,
    Conferido,
    DataCriacao,
    DataAtualizacao,
    DataConferencia,
    DataPrevisao,
    CaminhoZipImportacao,
    NomePdfNoZip,
    PaginaPdf,
    DataPrimeiraImpressao,
    DataReimpressao,
    MotivoReimpressao,
    Oculto
)
VALUES (
    $NumeroPedidoCliente,
    $NomeCliente,
    $Marketplace,
    $CodigoEtiqueta,
    $Status,
    $JsonItens,
    $EtiquetaMarketplaceZpl,
    $Impresso,
    $Conferido,
    $DataCriacao,
    $DataAtualizacao,
    $DataConferencia,
    $DataPrevisao,
    $CaminhoZipImportacao,
    $NomePdfNoZip,
    $PaginaPdf,
    $DataPrimeiraImpressao,
    $DataReimpressao,
    $MotivoReimpressao,
    $Oculto
)
ON CONFLICT(NumeroPedidoCliente) DO UPDATE SET
    NomeCliente = $NomeCliente,
    Marketplace = $Marketplace,
    CodigoEtiqueta = $CodigoEtiqueta,
    Status = $Status,
    JsonItens = $JsonItens,
    EtiquetaMarketplaceZpl = $EtiquetaMarketplaceZpl,
    Impresso = $Impresso,
    Conferido = $Conferido,
    DataAtualizacao = $DataAtualizacao,
    DataConferencia = $DataConferencia,
    DataPrevisao = $DataPrevisao,
    CaminhoZipImportacao = $CaminhoZipImportacao,
    NomePdfNoZip = $NomePdfNoZip,
    PaginaPdf = $PaginaPdf,
    DataPrimeiraImpressao = COALESCE(Pedidos.DataPrimeiraImpressao, $DataPrimeiraImpressao),
    DataReimpressao = $DataReimpressao,
    MotivoReimpressao = $MotivoReimpressao,
    Oculto = $Oculto
RETURNING Id;";

            command.Parameters.AddWithValue("$NumeroPedidoCliente", pedido.NumeroPedidoCliente ?? "");
            command.Parameters.AddWithValue("$NomeCliente", pedido.NomeCliente ?? "");
            command.Parameters.AddWithValue("$Marketplace", pedido.Marketplace ?? "");
            command.Parameters.AddWithValue("$CodigoEtiqueta", pedido.CodigoEtiqueta ?? "");
            command.Parameters.AddWithValue("$Status", pedido.Status ?? "");
            command.Parameters.AddWithValue("$JsonItens", pedido.JsonItens ?? "");
            command.Parameters.AddWithValue("$EtiquetaMarketplaceZpl", pedido.EtiquetaMarketplaceZpl ?? "");
            command.Parameters.AddWithValue("$Impresso", pedido.Impresso ? 1 : 0);
            command.Parameters.AddWithValue("$Conferido", pedido.Conferido ? 1 : 0);
            command.Parameters.AddWithValue("$DataCriacao", DataHelper.SerializarData(pedido.DataCriacao) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$DataAtualizacao", DataHelper.SerializarData(pedido.DataAtualizacao) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$DataConferencia", DataHelper.SerializarData(pedido.DataConferencia) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$DataPrevisao", DataHelper.SerializarData(pedido.DataPrevisao) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$CaminhoZipImportacao", pedido.CaminhoZipImportacao ?? "");
            command.Parameters.AddWithValue("$NomePdfNoZip", pedido.NomePdfNoZip ?? "");
            command.Parameters.AddWithValue("$PaginaPdf", pedido.PaginaPdf.HasValue ? pedido.PaginaPdf.Value : DBNull.Value);
            command.Parameters.AddWithValue("$DataPrimeiraImpressao", DataHelper.SerializarData(pedido.DataPrimeiraImpressao) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$DataReimpressao", DataHelper.SerializarData(pedido.DataReimpressao) ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$MotivoReimpressao", pedido.MotivoReimpressao ?? "");
            command.Parameters.AddWithValue("$Oculto", pedido.Oculto ? 1 : 0);

            var result = command.ExecuteScalar();
            if (result != null && result != DBNull.Value)
            {
                pedido.Id = Convert.ToInt32(result);
            }
        }

        public void SalvarOuAtualizar(PedidoConferencia pedido)
        {
            if (pedido == null) return;
            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();
            ExecutarUpsert(connection, null, pedido);
        }

        public void SalvarOuAtualizarVarios(List<PedidoConferencia> pedidos)
        {
            if (pedidos == null || !pedidos.Any())
                return;

            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var pedido in pedidos)
                {
                    ExecutarUpsert(connection, transaction, pedido);
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void SalvarOuAtualizarPreservandoStatus(PedidoConferencia pedido)
        {
            if (pedido == null)
                return;

            // Busca incluindo ocultos para não perder o flag Oculto na re-sync
            var existente = ObterPorNumero(pedido.NumeroPedidoCliente);

            if (existente != null)
            {
                // PRESERVA STATUS IMPORTANTES
                pedido.Impresso = existente.Impresso;
                pedido.Conferido = existente.Conferido;

                if (!string.IsNullOrWhiteSpace(existente.CodigoEtiqueta))
                    pedido.CodigoEtiqueta = existente.CodigoEtiqueta;

                if (!string.IsNullOrWhiteSpace(existente.EtiquetaMarketplaceZpl))
                    pedido.EtiquetaMarketplaceZpl = existente.EtiquetaMarketplaceZpl;

                if (!string.IsNullOrWhiteSpace(existente.Status))
                    pedido.Status = existente.Status;

                pedido.DataCriacao = existente.DataCriacao;
                pedido.DataConferencia = existente.DataConferencia;
                pedido.DataPrevisao = existente.DataPrevisao;
                
                if (string.IsNullOrWhiteSpace(pedido.CaminhoZipImportacao) && !string.IsNullOrWhiteSpace(existente.CaminhoZipImportacao))
                    pedido.CaminhoZipImportacao = existente.CaminhoZipImportacao;

                if (string.IsNullOrWhiteSpace(pedido.NomePdfNoZip) && !string.IsNullOrWhiteSpace(existente.NomePdfNoZip))
                    pedido.NomePdfNoZip = existente.NomePdfNoZip;

                if (!pedido.PaginaPdf.HasValue && existente.PaginaPdf.HasValue)
                    pedido.PaginaPdf = existente.PaginaPdf;

                pedido.DataPrimeiraImpressao = existente.DataPrimeiraImpressao;
                pedido.DataReimpressao = existente.DataReimpressao;
                pedido.MotivoReimpressao = existente.MotivoReimpressao;

                // PRESERVA Oculto: pedido oculto jamais reaparece por nova sincronização
                pedido.Oculto = existente.Oculto;
            }

            SalvarOuAtualizar(pedido);
        }
        public bool OcultarPedido(string numeroPedidoCliente)
        {
            string numero = (numeroPedidoCliente ?? "").Trim();

            if (string.IsNullOrWhiteSpace(numero))
                return false;

            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            int alterados;

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
UPDATE Pedidos
SET Oculto = 1,
    DataOcultacao = $DataOcultacao,
    Status = 'Removido pelo administrador'
WHERE NumeroPedidoCliente = $NumeroPedidoCliente;";
                command.Parameters.AddWithValue("$DataOcultacao", DataHelper.SerializarData(DateTime.Now));
                command.Parameters.AddWithValue("$NumeroPedidoCliente", numero);
                alterados = command.ExecuteNonQuery();
            }

            if (alterados > 0)
            {
                using var auditoria = connection.CreateCommand();
                auditoria.Transaction = transaction;
                auditoria.CommandText = @"
INSERT INTO AuditoriaAdministrativa (Acao, NumeroPedidoCliente, DataAcao)
VALUES ('REMOVER_PEDIDO_PREPARACAO', $NumeroPedidoCliente, $DataAcao);";
                auditoria.Parameters.AddWithValue("$NumeroPedidoCliente", numero);
                auditoria.Parameters.AddWithValue("$DataAcao", DataHelper.SerializarData(DateTime.Now));
                auditoria.ExecuteNonQuery();
            }

            transaction.Commit();
            return alterados > 0;
        }

        public void Limpar()
        {
            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            // Mantém os pedidos removidos pelo administrador para que não voltem
            // a aparecer em uma nova busca do Omie.
            command.CommandText = "DELETE FROM Pedidos WHERE IFNULL(Oculto, 0) = 0";

            command.ExecuteNonQuery();
        }

        public bool LimparPedidosPorDia(DateTime dataAlvo, bool restaurarRemovidos = false)
        {
            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            int alterados;
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                
                string dataInicio = dataAlvo.ToString("yyyy-MM-dd") + " 00:00:00";
                string dataFim = dataAlvo.AddDays(1).ToString("yyyy-MM-dd") + " 00:00:00";
                
                string sqlOculto = restaurarRemovidos ? "Oculto = 0, DataOcultacao = NULL," : "";
                string whereOculto = restaurarRemovidos ? "" : "AND IFNULL(Oculto, 0) = 0";

                command.CommandText = $@"
UPDATE Pedidos
SET Status = 'Pendente',
    Conferido = 0,
    Impresso = 0,
    DataAtualizacao = $DataAtualizacao,
    DataConferencia = NULL,
    CodigoEtiqueta = NULL,
    EtiquetaMarketplaceZpl = NULL,
    CaminhoZipImportacao = NULL,
    NomePdfNoZip = NULL,
    PaginaPdf = NULL,
    DataPrimeiraImpressao = NULL,
    DataReimpressao = NULL,
    MotivoReimpressao = NULL,
    {sqlOculto}
    DataCriacao = DataCriacao -- dummy for trailing comma
WHERE DataPrevisao >= $DataInicio AND DataPrevisao < $DataFim
  {whereOculto};";
                
                command.Parameters.AddWithValue("$DataAtualizacao", DataHelper.SerializarData(DateTime.Now));
                command.Parameters.AddWithValue("$DataInicio", dataInicio);
                command.Parameters.AddWithValue("$DataFim", dataFim);
                
                alterados = command.ExecuteNonQuery();
            }

            if (alterados > 0)
            {
                using var auditoria = connection.CreateCommand();
                auditoria.Transaction = transaction;
                auditoria.CommandText = @"
INSERT INTO AuditoriaAdministrativa (Acao, NumeroPedidoCliente, DataAcao)
VALUES ('RESET_DIA_OPERACIONAL', $DataAlvoString, $DataAcao);";
                auditoria.Parameters.AddWithValue("$DataAlvoString", dataAlvo.ToString("yyyy-MM-dd"));
                auditoria.Parameters.AddWithValue("$DataAcao", DataHelper.SerializarData(DateTime.Now));
                auditoria.ExecuteNonQuery();
            }

            transaction.Commit();
            return alterados > 0;
        }

        public void RegistrarAuditoria(string acao, string numeroPedidoCliente, string detalhes = "")
        {
            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();

            using var auditoria = connection.CreateCommand();
            auditoria.CommandText = @"
INSERT INTO AuditoriaAdministrativa (Acao, NumeroPedidoCliente, DataAcao)
VALUES ($Acao, $NumeroPedidoCliente, $DataAcao);";
            // Para adicionar Detalhes, a tabela precisaria ter uma coluna. 
            // Como nÃ£o tem, colocamos o detalhe na prÃ³pria Acao se necessÃ¡rio
            string acaoFinal = string.IsNullOrWhiteSpace(detalhes) ? acao : $"{acao} | {detalhes}";
            
            auditoria.Parameters.AddWithValue("$Acao", acaoFinal);
            auditoria.Parameters.AddWithValue("$NumeroPedidoCliente", numeroPedidoCliente ?? "");
            auditoria.Parameters.AddWithValue("$DataAcao", DataHelper.SerializarData(DateTime.Now));
            auditoria.ExecuteNonQuery();
        }
    }
}
