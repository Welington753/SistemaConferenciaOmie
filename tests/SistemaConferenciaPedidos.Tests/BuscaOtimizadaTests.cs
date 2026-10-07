using Xunit;
using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Repositories;
using System.Collections.Generic;
using System;

namespace SistemaConferenciaPedidos.Tests
{
    public class BuscaOtimizadaTests : IDisposable
    {
        private readonly Infrastructure.SqliteTestFixture _fixture;

        public BuscaOtimizadaTests()
        {
            _fixture = new Infrastructure.SqliteTestFixture();
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }

        [Fact]
        public void InsercaoEmMassa_OtimizadaParaBancoDeDados()
        {
            var repo = new PedidoRepositorySqlite();
            var pedidos = new List<PedidoConferencia>();

            for (int i = 0; i < 500; i++)
            {
                pedidos.Add(new PedidoConferencia
                {
                    CodigoEtiqueta = $"ETQ-{i}",
                    NumeroPedidoCliente = $"PED-{i}",
                    NomeCliente = "Cliente Falso",
                    Marketplace = "Shopee",
                    DataPrevisao = DateTime.Today,
                    Status = "Pendente",
                    EtiquetaMarketplaceZpl = "ZPL",
                    Impresso = false,
                    Conferido = false
                });
            }

            repo.SalvarOuAtualizarVarios(pedidos);
            var count = repo.ObterPorPeriodo(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1)).Count;

            Assert.Equal(500, count);
        }
    }
}
