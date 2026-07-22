using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaConferenciaPedidos
{
    public sealed class FrmMotivoReimpressao : Form
    {
        private readonly TextBox _txtMotivo;
        public string Motivo => _txtMotivo.Text.Trim();

        public FrmMotivoReimpressao()
        {
            Text = "Motivo da Reimpressão";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(400, 150);

            var lblOrientacao = new Label
            {
                AutoSize = true,
                Location = new Point(20, 20),
                Text = "Informe o motivo para a reimpressão deste pedido:"
            };

            _txtMotivo = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(350, 23),
                MaxLength = 200
            };

            var btnCancelar = new Button
            {
                DialogResult = DialogResult.Cancel,
                Location = new Point(200, 95),
                Size = new Size(85, 30),
                Text = "Cancelar"
            };

            var btnConfirmar = new Button
            {
                Location = new Point(295, 95),
                Size = new Size(85, 30),
                Text = "Confirmar"
            };
            btnConfirmar.Click += BtnConfirmar_Click;

            Controls.Add(lblOrientacao);
            Controls.Add(_txtMotivo);
            Controls.Add(btnCancelar);
            Controls.Add(btnConfirmar);

            AcceptButton = btnConfirmar;
            CancelButton = btnCancelar;
        }

        private void BtnConfirmar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Motivo))
            {
                MessageBox.Show("Por favor, informe o motivo.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtMotivo.Focus();
                return;
            }
            if (Motivo.Length < 5)
            {
                MessageBox.Show("O motivo informado é muito curto.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtMotivo.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        public static string SolicitarMotivo(IWin32Window owner)
        {
            using var frm = new FrmMotivoReimpressao();
            if (frm.ShowDialog(owner) == DialogResult.OK)
            {
                return frm.Motivo;
            }
            return null;
        }
    }
}
