using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Repositories;
using SistemaConferenciaPedidos.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SistemaConferenciaPedidos
{
    public partial class FrmConferencia : Form
    {
        private readonly IPedidoRepository _pedidoRepository = new PedidoRepositorySqlite();
        private readonly ConferenciaService _conferenciaService = new ConferenciaService();

        public FrmConferencia()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AtualizarTelaConferencia();
            txtLeitura.Focus();
        }

        private void btnConferir_Click(object sender, EventArgs e)
        {
            string textoLido = txtLeitura.Text.Trim();

            if (string.IsNullOrWhiteSpace(textoLido))
            {
                txtLeitura.Focus();
                return;
            }

            btnConferir.Enabled = false;

            try
            {
                bool encontradoPorNumeroPedido;

                var pedido = _conferenciaService.BuscarPedidoPorCodigoOuNumero(
                    _pedidoRepository.ObterTodos(),
                    textoLido,
                    out encontradoPorNumeroPedido);

                if (pedido == null)
                {
                    lstErros.Items.Insert(0, $"❌ NÃO ENCONTRADO: {textoLido}");
                    Console.Beep(1500, 1500);
                    return;
                }

                if (pedido.Conferido)
                {
                    lstErros.Items.Insert(0,
                        $"🔵 JÁ CONFERIDO: {pedido.NumeroPedidoCliente} | {pedido.NomeCliente}");

                    Console.Beep(1500, 1500);
                    return;
                }

                pedido.Conferido = true;
                pedido.DataConferencia = DateTime.Now;

                _pedidoRepository.SalvarOuAtualizar(pedido);

                lstSucesso.Items.Insert(0,
                    $"✅ CONFERIDO: {pedido.NumeroPedidoCliente} | {pedido.NomeCliente}");

                AtualizarTelaConferencia();
            }
            catch (Exception ex)
            {
                lstErros.Items.Insert(0, $"⚠ ERRO: {ex.Message}");
                Console.Beep(1500, 1500);
            }
            finally
            {
                txtLeitura.Clear();
                txtLeitura.Focus();
                txtLeitura.Select();
                btnConferir.Enabled = true;
            }
        }

        private void AtualizarTelaConferencia()
        {
            var pedidos = _pedidoRepository.ObterTodos();

            AtualizarResumoConferencia(pedidos);
            AtualizarPedidosFaltantes(pedidos);
        }

        private void AtualizarResumoConferencia(List<PedidoConferencia> pedidos)
        {
            int totalGeral = pedidos.Count;
            int conferidosGeral = pedidos.Count(p => p.Conferido);
            int faltamGeral = totalGeral - conferidosGeral;

            var ml = pedidos.Where(p => _conferenciaService.NormalizarMarketplaceResumo(p.Marketplace) == "MERCADO LIVRE").ToList();
            var amazon = pedidos.Where(p => _conferenciaService.NormalizarMarketplaceResumo(p.Marketplace) == "AMAZON").ToList();
            var shopee = pedidos.Where(p => _conferenciaService.NormalizarMarketplaceResumo(p.Marketplace) == "SHOPEE").ToList();

            lblResumoGeral.Text = $"TOTAL DO DIA: {totalGeral} | CONFERIDOS: {conferidosGeral} | FALTAM: {faltamGeral}";
            lblResumoMl.Text = MontarResumoMarketplace("MERCADO LIVRE", ml);
            lblResumoAmazon.Text = MontarResumoMarketplace("AMAZON", amazon);
            lblResumoShopee.Text = MontarResumoMarketplace("SHOPEE", shopee);

            DestacarResumo(lblResumoGeral, faltamGeral);
            DestacarResumo(lblResumoMl, ml.Count(p => !p.Conferido));
            DestacarResumo(lblResumoAmazon, amazon.Count(p => !p.Conferido));
            DestacarResumo(lblResumoShopee, shopee.Count(p => !p.Conferido));
        }

        private string MontarResumoMarketplace(string nome, List<PedidoConferencia> pedidos)
        {
            int total = pedidos.Count;
            int conferidos = pedidos.Count(p => p.Conferido);
            int faltam = total - conferidos;

            return $"{nome} | TOTAL: {total} | CONFERIDOS: {conferidos} | FALTAM: {faltam}";
        }

        private void AtualizarPedidosFaltantes(List<PedidoConferencia> pedidos)
        {
            PreencherListaFaltantes(lstFaltantesAmazon, pedidos, "AMAZON");
            PreencherListaFaltantes(lstFaltantesShopee, pedidos, "SHOPEE");
            PreencherListaFaltantes(lstFaltantesMl, pedidos, "MERCADO LIVRE");
        }

        private void PreencherListaFaltantes(
            ListBox lista,
            List<PedidoConferencia> pedidos,
            string marketplace)
        {
            lista.Items.Clear();

            var faltantes = pedidos
                .Where(p =>
                    !p.Conferido &&
                    _conferenciaService.NormalizarMarketplaceResumo(p.Marketplace) == marketplace)
                .OrderBy(p => p.NomeCliente)
                .ToList();

            foreach (var pedido in faltantes)
            {
                lista.Items.Add($"{pedido.NumeroPedidoCliente} | {pedido.NomeCliente}");
            }

            if (faltantes.Count == 0)
            {
                lista.Items.Add("✅ Todos os pedidos conferidos!");
            }
        }

        private void DestacarResumo(Label label, int faltam)
        {
            if (faltam <= 0)
            {
                label.BackColor = System.Drawing.Color.Honeydew;
                label.ForeColor = System.Drawing.Color.DarkGreen;
            }
            else
            {
                label.BackColor = System.Drawing.Color.MistyRose;
                label.ForeColor = System.Drawing.Color.DarkRed;
            }
        }

        private void txtLeitura_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnConferir.PerformClick();
            }
        }

        private void FrmConferencia_Load(object sender, EventArgs e)
        {
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F2:
                    txtLeitura.Focus();
                    txtLeitura.SelectAll();
                    return true;

                case Keys.F5:
                    AtualizarTelaConferencia();
                    txtLeitura.Focus();
                    return true;

                case Keys.Escape:
                    txtLeitura.Clear();
                    txtLeitura.Focus();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}