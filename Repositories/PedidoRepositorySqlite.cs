using Microsoft.Data.Sqlite;
using SistemaConferenciaPedidos.Data;
using SistemaConferenciaPedidos.Models;
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
    DataConferencia
FROM Pedidos";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new PedidoConferencia
                {
                    NumeroPedidoCliente = reader["NumeroPedidoCliente"]?.ToString(),
                    NomeCliente = reader["NomeCliente"]?.ToString(),
                    Marketplace = reader["Marketplace"]?.ToString(),
                    CodigoEtiqueta = reader["CodigoEtiqueta"]?.ToString(),
                    Status = reader["Status"]?.ToString(),
                    JsonItens = reader["JsonItens"]?.ToString(),
                    EtiquetaMarketplaceZpl = reader["EtiquetaMarketplaceZpl"]?.ToString(),
                    Impresso = Convert.ToInt32(reader["Impresso"]) == 1,
                    Conferido = Convert.ToInt32(reader["Conferido"]) == 1
                });
            }

            return lista;
        }

        public void SalvarOuAtualizar(PedidoConferencia pedido)
        {
            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

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
    DataConferencia
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
    $DataConferencia
)
ON CONFLICT(NumeroPedidoCliente)
DO UPDATE SET
    NomeCliente = excluded.NomeCliente,
    Marketplace = excluded.Marketplace,
    CodigoEtiqueta = excluded.CodigoEtiqueta,
    Status = excluded.Status,
    JsonItens = excluded.JsonItens,
    EtiquetaMarketplaceZpl = excluded.EtiquetaMarketplaceZpl,
    Impresso = excluded.Impresso,
    Conferido = excluded.Conferido,
    DataCriacao = excluded.DataCriacao,
    DataConferencia = excluded.DataConferencia;
";

            command.Parameters.AddWithValue("$NumeroPedidoCliente", pedido.NumeroPedidoCliente ?? "");
            command.Parameters.AddWithValue("$NomeCliente", pedido.NomeCliente ?? "");
            command.Parameters.AddWithValue("$Marketplace", pedido.Marketplace ?? "");
            command.Parameters.AddWithValue("$CodigoEtiqueta", pedido.CodigoEtiqueta ?? "");
            command.Parameters.AddWithValue("$Status", pedido.Status ?? "");
            command.Parameters.AddWithValue("$JsonItens", pedido.JsonItens ?? "");
            command.Parameters.AddWithValue("$EtiquetaMarketplaceZpl", pedido.EtiquetaMarketplaceZpl ?? "");
            command.Parameters.AddWithValue("$Impresso", pedido.Impresso ? 1 : 0);
            command.Parameters.AddWithValue("$Conferido", pedido.Conferido ? 1 : 0);
            command.Parameters.AddWithValue("$DataCriacao", pedido.DataCriacao.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue(
                "$DataConferencia",
                pedido.DataConferencia?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");

            command.ExecuteNonQuery();
        }

        public void SalvarOuAtualizarVarios(List<PedidoConferencia> pedidos)
        {
            if (pedidos == null)
                return;

            foreach (var pedido in pedidos)
                SalvarOuAtualizar(pedido);
        }

        public void SalvarOuAtualizarPreservandoStatus(PedidoConferencia pedido)
        {
            if (pedido == null)
                return;

            var existente = ObterTodos()
                .FirstOrDefault(x =>
                    x.NumeroPedidoCliente == pedido.NumeroPedidoCliente);

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

                pedido.DataConferencia = existente.DataConferencia;
            }

            SalvarOuAtualizar(pedido);
        }
        public void Limpar()
        {
            using var connection = new SqliteConnection(Database.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();

            command.CommandText = "DELETE FROM Pedidos";

            command.ExecuteNonQuery();
        }
    }
}