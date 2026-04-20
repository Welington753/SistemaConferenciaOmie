using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Services;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SistemaConferenciaPedidos
{
    public partial class FrmConferencia : Form
    {
        public FrmConferencia()
        {
            InitializeComponent();
        }

        private readonly ConferenciaService _conferenciaService = new ConferenciaService();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AtualizarResumoConferencia();
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
                 PedidoRepository.ObterTodos(),
                 textoLido,
                 out encontradoPorNumeroPedido);

                if (pedido == null)
                {
                    string textoHistorico = _conferenciaService.NormalizarNumeroPedido(textoLido);
                    if (string.IsNullOrWhiteSpace(textoHistorico))
                        textoHistorico = textoLido.Trim().ToUpperInvariant();

                    lstHistorico.Items.Insert(0, $"❌ NÃO ENCONTRADO: {textoHistorico}");

                    System.Media.SystemSounds.Exclamation.Play();
                    MessageBox.Show(
                        "Pedido não encontrado.\n\n" +
                        "Confira o código de rastreio ou o número do pedido.",
                        "Divergência",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                if (pedido.Conferido)
                {
                    lstHistorico.Items.Insert(0,
                        $"🔵 JÁ CONFERIDO: {pedido.NumeroPedidoCliente} | {pedido.NomeCliente}");

                    System.Media.SystemSounds.Exclamation.Play();
                    MessageBox.Show(
                        "Esse pedido já foi conferido anteriormente.",
                        "Divergência",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                pedido.Conferido = true;

                bool temEtiquetaVinculada = !string.IsNullOrWhiteSpace(pedido.CodigoEtiqueta);

                if (encontradoPorNumeroPedido && !temEtiquetaVinculada)
                {
                    lstHistorico.Items.Insert(0,
                        $"✅ CONFERIDO SEM ETIQUETA: {pedido.NumeroPedidoCliente} | {pedido.NomeCliente}");
                }
                else if (encontradoPorNumeroPedido)
                {
                    lstHistorico.Items.Insert(0,
                        $"✅ CONFERIDO POR PEDIDO: {pedido.NumeroPedidoCliente} | {pedido.NomeCliente}");
                }
                else
                {
                    lstHistorico.Items.Insert(0,
                        $"✅ CONFERIDO: {pedido.CodigoEtiqueta} | {pedido.NumeroPedidoCliente} | {pedido.NomeCliente}");
                }

                AtualizarResumoConferencia();
            }
            catch (Exception ex)
            {
                lstHistorico.Items.Insert(0, $"⚠ ERRO: {ex.Message}");

                System.Media.SystemSounds.Exclamation.Play();
                MessageBox.Show(
                    "Erro ao conferir: " + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                txtLeitura.Clear();
                txtLeitura.Focus();
                btnConferir.Enabled = true;
            }
        }

  

    
     
        private string MontarResumoProdutos(string jsonPedido)
        {
            if (string.IsNullOrWhiteSpace(jsonPedido))
                return "- Sem itens";

            try
            {
                var sb = new StringBuilder();

                using var json = JsonDocument.Parse(jsonPedido);
                var root = json.RootElement;

                if (!root.TryGetProperty("det", out var detNode))
                    return "- Sem itens";

                foreach (var item in detNode.EnumerateArray())
                {
                    if (!item.TryGetProperty("produto", out var produtoNode))
                        continue;

                    string descricao = "";
                    string quantidade = "";

                    if (produtoNode.TryGetProperty("descricao", out var descricaoNode))
                        descricao = LerValorComoTexto(descricaoNode);

                    if (produtoNode.TryGetProperty("quantidade", out var quantidadeNode))
                        quantidade = LerValorComoTexto(quantidadeNode);

                    sb.AppendLine($"- {descricao} | Qtd: {quantidade}");
                }

                string texto = sb.ToString().Trim();
                return string.IsNullOrWhiteSpace(texto) ? "- Sem itens" : texto;
            }
            catch
            {
                return "- Erro ao ler itens";
            }
        }

        private string LerValorComoTexto(JsonElement elemento)
        {
            switch (elemento.ValueKind)
            {
                case JsonValueKind.String:
                    return elemento.GetString() ?? "";
                case JsonValueKind.Number:
                    return elemento.ToString();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return elemento.GetBoolean().ToString();
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return "";
                default:
                    return elemento.ToString();
            }
        }

        private void AtualizarResumoConferencia()
        {
            var pedidos = PedidoRepository.ObterTodos();

            int totalGeral = pedidos.Count;
            int conferidosGeral = pedidos.Count(p => p.Conferido);
            int faltamGeral = totalGeral - conferidosGeral;

            var ml = pedidos.Where(p => _conferenciaService.NormalizarMarketplaceResumo(p.Marketplace) == "MERCADO LIVRE").ToList();
            var amazon = pedidos.Where(p => _conferenciaService.NormalizarMarketplaceResumo(p.Marketplace) == "AMAZON").ToList();
            var shopee = pedidos.Where(p => _conferenciaService.NormalizarMarketplaceResumo(p.Marketplace) == "SHOPEE").ToList();

            int mlConferidos = ml.Count(p => p.Conferido);
            int amazonConferidos = amazon.Count(p => p.Conferido);
            int shopeeConferidos = shopee.Count(p => p.Conferido);

            int mlFaltam = ml.Count - mlConferidos;
            int amazonFaltam = amazon.Count - amazonConferidos;
            int shopeeFaltam = shopee.Count - shopeeConferidos;

            lblResumoGeral.Text = $"TOTAL DO DIA: {totalGeral}   |   CONFERIDOS: {conferidosGeral}   |   FALTAM: {faltamGeral}";
            lblResumoMl.Text = $"MERCADO LIVRE   |   TOTAL: {ml.Count}   |   CONFERIDOS: {mlConferidos}   |   FALTAM: {mlFaltam}";
            lblResumoAmazon.Text = $"AMAZON               |   TOTAL: {amazon.Count}   |   CONFERIDOS: {amazonConferidos}   |   FALTAM: {amazonFaltam}";
            lblResumoShopee.Text = $"SHOPEE                |   TOTAL: {shopee.Count}   |   CONFERIDOS: {shopeeConferidos}   |   FALTAM: {shopeeFaltam}";

            DestacarResumo(lblResumoGeral, faltamGeral);
            DestacarResumo(lblResumoMl, mlFaltam);
            DestacarResumo(lblResumoAmazon, amazonFaltam);
            DestacarResumo(lblResumoShopee, shopeeFaltam);
        }

        private void DestacarResumo(Label label, int faltam)
        {
            if (label == null)
                return;

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
    }
}