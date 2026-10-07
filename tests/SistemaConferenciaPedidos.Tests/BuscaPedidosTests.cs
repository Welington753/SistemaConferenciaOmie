using Xunit;
using SistemaConferenciaPedidos;
using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Repositories;
using System.Reflection;
using System.Windows.Forms;
using System;
using System.Threading;

namespace SistemaConferenciaPedidos.Tests
{
    public class BuscaPedidosTests : IDisposable
    {
        private FrmPreparacaoPedidos _frm;
        private readonly Infrastructure.SqliteTestFixture _fixture;

        public BuscaPedidosTests()
        {
            _fixture = new Infrastructure.SqliteTestFixture();
        }

        public void Dispose()
        {
            _frm?.Dispose();
            _fixture?.Dispose();
        }

        [Fact]
        public void CarregarPedidos_LeDoBancoEPreencheODataGridView()
        {
            var config = new SistemaConferenciaPedidos.Repositories.ConfiguracaoRepositorySqlite();
            config.SalvarValor("OmieAppKey", "key");
            config.SalvarValor("OmieAppSecret", "secret");
            var repo = new PedidoRepositorySqlite();
            var pedido = new PedidoConferencia
            {
                CodigoEtiqueta = "ETQ-12345",
                NumeroPedidoCliente = "PED-TESTE",
                NomeCliente = "Cliente Falso",
                Marketplace = "Shopee",
                DataPrevisao = DateTime.Today,
                Status = "Pendente",
                EtiquetaMarketplaceZpl = "ZPL",
                Impresso = false,
                Conferido = false
            };
            repo.SalvarOuAtualizar(pedido);

            _frm = new FrmPreparacaoPedidos();
            var handle = _frm.Handle;

            var dtpInicial = (DateTimePicker)_frm.GetType().GetField("dtpDataInicial", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_frm);
            var dtpFinal = (DateTimePicker)_frm.GetType().GetField("dtpDataFinal", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_frm);
            
            dtpInicial.Value = DateTime.Today;
            dtpFinal.Value = DateTime.Today;

            var carregarPedidos = _frm.GetType().GetMethod("CarregarPedidos", BindingFlags.NonPublic | BindingFlags.Instance);
            carregarPedidos.Invoke(_frm, new object[] { null });

            var dgv = (DataGridView)_frm.GetType().GetField("dgvPedidos", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_frm);
            
            Assert.True(dgv.Rows.Count > 0);
            Assert.Equal("PED-TESTE", dgv.Rows[0].Cells["NumeroPedidoCliente"].Value.ToString());
        }
    }
}

