using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Repositories;
using SistemaConferenciaPedidos.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SistemaConferenciaPedidos
{
    public partial class FrmBuscarPedidoPorProduto : Form
    {
        private readonly PedidoProdutoBuscaService _pedidoProdutoBuscaService =
            new PedidoProdutoBuscaService();

        private readonly IPedidoRepository _pedidoRepository =
            new PedidoRepositorySqlite();

        public PedidoConferencia PedidoSelecionado { get; private set; }

        public FrmBuscarPedidoPorProduto()
        {
            InitializeComponent();

            ConfigurarGrid();

            btnBuscar.Click += btnBuscar_Click;
            btnConfirmarImprimir.Click += btnConfirmarImprimir_Click;

            txtCodigoProduto.KeyDown += txtCodigoProduto_KeyDown;
        }

        private void ConfigurarGrid()
        {
            dgvResultados.AutoGenerateColumns = false;
            dgvResultados.Columns.Clear();

            dgvResultados.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NumeroPedidoCliente",
                HeaderText = "Pedido"
            });

            dgvResultados.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NomeCliente",
                HeaderText = "Cliente"
            });

            dgvResultados.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Marketplace",
                HeaderText = "Marketplace"
            });

            dgvResultados.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CodigoEtiqueta",
                HeaderText = "Etiqueta"
            });
        }

        private void txtCodigoProduto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Buscar();
                e.SuppressKeyPress = true;
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            Buscar();
        }

        private void Buscar()
        {
            string codigo = txtCodigoProduto.Text?.Trim();

            if (string.IsNullOrWhiteSpace(codigo))
            {
                MessageBox.Show("Digite ou bipa um EAN/SKU.");
                return;
            }

            var pedidos = _pedidoRepository.ObterTodos();

            var encontrados = _pedidoProdutoBuscaService
                .BuscarPedidosPorEanOuSku(pedidos, codigo);

            dgvResultados.DataSource = null;
            dgvResultados.DataSource = encontrados;

            if (encontrados.Count == 0)
            {
                MessageBox.Show("Nenhum pedido encontrado.");
            }
        }

        private void btnConfirmarImprimir_Click(object sender, EventArgs e)
        {
            if (dgvResultados.CurrentRow == null)
            {
                MessageBox.Show("Selecione um pedido.");
                return;
            }

            PedidoSelecionado =
                dgvResultados.CurrentRow.DataBoundItem as PedidoConferencia;

            if (PedidoSelecionado == null)
                return;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}