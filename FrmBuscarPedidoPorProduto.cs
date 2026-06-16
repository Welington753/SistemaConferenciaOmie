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
        private readonly Action<PedidoConferencia> _aoConfirmarImpressao;

        private readonly PedidoProdutoBuscaService _pedidoProdutoBuscaService =
            new PedidoProdutoBuscaService();

        private readonly IPedidoRepository _pedidoRepository =
            new PedidoRepositorySqlite();

        public PedidoConferencia PedidoSelecionado { get; private set; }

        public FrmBuscarPedidoPorProduto(Action<PedidoConferencia> aoConfirmarImpressao)
        {
            InitializeComponent();

            KeyPreview = true;

            _aoConfirmarImpressao = aoConfirmarImpressao;

            ConfigurarGrid();

            btnBuscar.Click += btnBuscar_Click;
            btnConfirmarImprimir.Click += btnConfirmarImprimir_Click;

            txtCodigoProduto.KeyDown += txtCodigoProduto_KeyDown;
            dgvResultados.KeyDown += dgvResultados_KeyDown;
            dgvResultados.CellDoubleClick += dgvResultados_CellDoubleClick;
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
                e.SuppressKeyPress = true;
                btnBuscar.PerformClick();

                if (dgvResultados.Rows.Count > 0)
                {
                    dgvResultados.Focus();
                    dgvResultados.ClearSelection();
                    dgvResultados.Rows[0].Selected = true;
                    dgvResultados.CurrentCell = dgvResultados.Rows[0].Cells[1];
                }
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
            ConfirmarPedidoSelecionado();
        }
        private void dgvResultados_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.F2)
            {
                e.SuppressKeyPress = true;
                ConfirmarPedidoSelecionado();
            }
        }

        private void dgvResultados_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                ConfirmarPedidoSelecionado();
        }

        private void ConfirmarPedidoSelecionado()
        {
            if (dgvResultados.CurrentRow == null)
            {
                MessageBox.Show("Selecione um pedido.");
                return;
            }

            var pedido = dgvResultados.CurrentRow.DataBoundItem as PedidoConferencia;

            if (pedido == null)
                return;

            _aoConfirmarImpressao?.Invoke(pedido);

            LimparParaNovaBusca();
        }
        private void LimparParaNovaBusca()
        {
            txtCodigoProduto.Clear();

            dgvResultados.DataSource = null;
            dgvResultados.Rows.Clear();

            txtCodigoProduto.Focus();
        }
    }
}