using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SistemaConferenciaPedidos
{
    public partial class FrmValidacaoEan : Form
    {
        private class ItemValidacao
        {
            public string Produto { get; set; } = "";
            public string Sku { get; set; } = "";
            public string Ean { get; set; } = "";
            public bool Conferido { get; set; }
        }

        private readonly List<ItemValidacao> _itens;

        public FrmValidacaoEan(IEnumerable<object> itens, string numeroPedido, string nomeCliente)
        {
            _itens = new List<ItemValidacao>();

            if (itens != null)
            {
                foreach (var item in itens)
                {
                    string produto = LerPropriedade(item, "Produto");
                    string sku = LerPropriedade(item, "Sku");
                    string ean = LerPropriedade(item, "Ean");

                    _itens.Add(new ItemValidacao
                    {
                        Produto = produto,
                        Sku = sku,
                        Ean = SomenteNumeros(ean),
                        Conferido = false
                    });
                }
            }

            InitializeComponent();

            lblTitulo.Text = $"Pedido: {numeroPedido}   |   Cliente: {nomeCliente}";
            AtualizarTela();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtLeitura.Focus();
        }

        private void txtLeitura_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ProcessarLeitura();
            }
        }

        private void btnConfirmar_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ProcessarLeitura()
        {
            string textoOriginal = txtLeitura.Text?.Trim() ?? "";
            string lido = SomenteNumeros(textoOriginal);

            txtLeitura.Clear();
            txtLeitura.Focus();

            if (string.IsNullOrWhiteSpace(lido))
                return;

            // SENHA / CÓDIGO DE LIBERAÇÃO MANUAL
            if (lido == "0051000012517")
            {
                System.Media.SystemSounds.Asterisk.Play();
                lstHistorico.Items.Insert(0, "🔓 LIBERAÇÃO MANUAL AUTORIZADA");

                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            var item = _itens.FirstOrDefault(x => !x.Conferido && x.Ean == lido);

            if (item == null)
            {
                System.Media.SystemSounds.Exclamation.Play();
                lstHistorico.Items.Insert(0, $"❌ NÃO CONFERE: {lido}");
                return;
            }

            item.Conferido = true;

            System.Media.SystemSounds.Asterisk.Play();
            lstHistorico.Items.Insert(0, $"✅ OK: {item.Ean} | {item.Produto}");

            AtualizarTela();

            bool todosConferidos = _itens.All(x => x.Conferido);

            if (todosConferidos)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void AtualizarTela()
        {
            lstPendentes.Items.Clear();

            var pendentes = _itens.Where(x => !x.Conferido).ToList();

            foreach (var item in pendentes)
            {
                lstPendentes.Items.Add($"{item.Ean} | {item.Produto} | SKU: {item.Sku}");
            }

            int total = _itens.Count;
            int conferidos = _itens.Count(x => x.Conferido);
            int faltam = total - conferidos;

            lblResumo.Text = $"Total: {total}   |   Conferidos: {conferidos}   |   Faltam: {faltam}";

            btnConfirmar.Enabled = false;
            btnConfirmar.Visible = false;
        }

        private string SomenteNumeros(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return "";

            return Regex.Replace(valor, @"\D", "");
        }

        private string LerPropriedade(object obj, string nomePropriedade)
        {
            if (obj == null || string.IsNullOrWhiteSpace(nomePropriedade))
                return "";

            try
            {
                PropertyInfo prop = obj.GetType().GetProperty(
                    nomePropriedade,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (prop == null)
                    return "";

                object valor = prop.GetValue(obj);
                return valor?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }
    }
}