using Microsoft.Data.Sqlite;
using SistemaConferenciaPedidos.Data;
using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Repositories;
using SistemaConferenciaPedidos.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaConferenciaPedidos.Services
{
    public class PedidoSincronizacaoService
    {
        private readonly IPedidoRepository _pedidoRepository;

        public PedidoSincronizacaoService(IPedidoRepository pedidoRepository)
        {
            _pedidoRepository = pedidoRepository;
        }

        private static readonly System.Threading.SemaphoreSlim _semaphore = new System.Threading.SemaphoreSlim(1, 1);

        public async System.Threading.Tasks.Task SincronizarAsync(List<PedidoConferencia> pedidosImportados)
        {
            if (pedidosImportados == null || pedidosImportados.Count == 0)
                return;

            // Deduplicação em memória (mantém a primeira ocorrência de cada número)
            var pedidosUnicos = pedidosImportados
                .Where(p => !string.IsNullOrWhiteSpace(p.NumeroPedidoCliente))
                .GroupBy(p => p.NumeroPedidoCliente.Trim())
                .Select(g => g.First())
                .ToList();

            if (!await _semaphore.WaitAsync(0))
            {
                // Já existe uma sincronização em andamento.
                return;
            }

            try
            {
                using var connection = new SqliteConnection(Database.ConnectionString);
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    using var upsertCommand = connection.CreateCommand();
                    upsertCommand.Transaction = transaction;
                    upsertCommand.CommandText = @"
INSERT INTO Pedidos (
    NumeroPedidoCliente, NomeCliente, Marketplace, CodigoEtiqueta, Status, JsonItens, 
    EtiquetaMarketplaceZpl, Impresso, Conferido, DataCriacao, DataAtualizacao, DataConferencia, DataPrevisao
) VALUES (
    $NumeroPedidoCliente, $NomeCliente, $Marketplace, $CodigoEtiqueta, $Status, $JsonItens, 
    $EtiquetaMarketplaceZpl, $Impresso, $Conferido, $DataCriacao, $DataAtualizacao, $DataConferencia, $DataPrevisao
)
ON CONFLICT(NumeroPedidoCliente)
DO UPDATE SET
    NomeCliente = CASE WHEN excluded.NomeCliente != '' THEN excluded.NomeCliente ELSE Pedidos.NomeCliente END,
    Marketplace = CASE WHEN excluded.Marketplace != '' THEN excluded.Marketplace ELSE Pedidos.Marketplace END,
    JsonItens = CASE WHEN excluded.JsonItens != '' THEN excluded.JsonItens ELSE Pedidos.JsonItens END,
    DataPrevisao = CASE WHEN excluded.DataPrevisao IS NOT NULL THEN excluded.DataPrevisao ELSE Pedidos.DataPrevisao END,
    DataAtualizacao = excluded.DataAtualizacao;
";
                    ConfigurarParametros(upsertCommand);

                    foreach (var pedidoNovo in pedidosUnicos)
                    {
                        AtribuirValores(upsertCommand, pedidoNovo);
                        upsertCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }



        private void ConfigurarParametros(SqliteCommand command)
        {
            command.Parameters.Add("$NumeroPedidoCliente", SqliteType.Text);
            command.Parameters.Add("$NomeCliente", SqliteType.Text);
            command.Parameters.Add("$Marketplace", SqliteType.Text);
            command.Parameters.Add("$CodigoEtiqueta", SqliteType.Text);
            command.Parameters.Add("$Status", SqliteType.Text);
            command.Parameters.Add("$JsonItens", SqliteType.Text);
            command.Parameters.Add("$EtiquetaMarketplaceZpl", SqliteType.Text);
            command.Parameters.Add("$Impresso", SqliteType.Integer);
            command.Parameters.Add("$Conferido", SqliteType.Integer);
            command.Parameters.Add("$DataCriacao", SqliteType.Text);
            command.Parameters.Add("$DataAtualizacao", SqliteType.Text);
            command.Parameters.Add("$DataConferencia", SqliteType.Text);
            command.Parameters.Add("$DataPrevisao", SqliteType.Text);
        }

        private void AtribuirValores(SqliteCommand command, PedidoConferencia pedido)
        {
            command.Parameters["$NumeroPedidoCliente"].Value = (pedido.NumeroPedidoCliente ?? "").Trim();
            command.Parameters["$NomeCliente"].Value = pedido.NomeCliente ?? "";
            command.Parameters["$Marketplace"].Value = pedido.Marketplace ?? "";
            command.Parameters["$CodigoEtiqueta"].Value = pedido.CodigoEtiqueta ?? "";
            command.Parameters["$Status"].Value = pedido.Status ?? "";
            command.Parameters["$JsonItens"].Value = pedido.JsonItens ?? "";
            command.Parameters["$EtiquetaMarketplaceZpl"].Value = pedido.EtiquetaMarketplaceZpl ?? "";
            command.Parameters["$Impresso"].Value = pedido.Impresso ? 1 : 0;
            command.Parameters["$Conferido"].Value = pedido.Conferido ? 1 : 0;
            command.Parameters["$DataCriacao"].Value = DataHelper.SerializarData(pedido.DataCriacao) ?? (object)DBNull.Value;
            command.Parameters["$DataAtualizacao"].Value = DataHelper.SerializarData(DateTime.Now);
            command.Parameters["$DataConferencia"].Value = DataHelper.SerializarData(pedido.DataConferencia) ?? (object)DBNull.Value;
            command.Parameters["$DataPrevisao"].Value = DataHelper.SerializarData(pedido.DataPrevisao) ?? (object)DBNull.Value;
        }
    }
}
