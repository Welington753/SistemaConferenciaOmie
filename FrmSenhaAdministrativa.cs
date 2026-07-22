using SistemaConferenciaPedidos.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaConferenciaPedidos
{
    public sealed class FrmSenhaAdministrativa : Form
    {
        private readonly AdminAuthService _authService;
        private readonly bool _modoConfiguracao;
        private readonly TextBox _txtSenha = new TextBox();
        private readonly TextBox _txtConfirmarSenha = new TextBox();
        private readonly Label _lblConfirmarSenha = new Label();

        private FrmSenhaAdministrativa(AdminAuthService authService)
        {
            _authService = authService;
            _modoConfiguracao = !_authService.SenhaConfigurada();

            Text = _modoConfiguracao
                ? "Configurar senha administrativa"
                : "Autorização administrativa";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(430, _modoConfiguracao ? 230 : 175);

            var lblOrientacao = new Label
            {
                AutoSize = false,
                Location = new Point(20, 18),
                Size = new Size(390, 44),
                Text = _modoConfiguracao
                    ? "Nenhuma senha administrativa foi configurada. Crie uma senha para autorizar ações administrativas."
                    : "Digite a senha administrativa para confirmar esta ação."
            };

            var lblSenha = new Label
            {
                AutoSize = true,
                Location = new Point(20, 72),
                Text = _modoConfiguracao ? "Nova senha" : "Senha administrativa"
            };

            _txtSenha.Location = new Point(20, 94);
            _txtSenha.Size = new Size(390, 23);
            _txtSenha.UseSystemPasswordChar = true;

            _lblConfirmarSenha.AutoSize = true;
            _lblConfirmarSenha.Location = new Point(20, 126);
            _lblConfirmarSenha.Text = "Confirmar nova senha";
            _lblConfirmarSenha.Visible = _modoConfiguracao;

            _txtConfirmarSenha.Location = new Point(20, 148);
            _txtConfirmarSenha.Size = new Size(390, 23);
            _txtConfirmarSenha.UseSystemPasswordChar = true;
            _txtConfirmarSenha.Visible = _modoConfiguracao;

            var btnCancelar = new Button
            {
                DialogResult = DialogResult.Cancel,
                Location = new Point(230, _modoConfiguracao ? 188 : 133),
                Size = new Size(85, 30),
                Text = "Cancelar"
            };

            var btnConfirmar = new Button
            {
                Location = new Point(325, _modoConfiguracao ? 188 : 133),
                Size = new Size(85, 30),
                Text = "Confirmar"
            };
            btnConfirmar.Click += BtnConfirmar_Click;

            Controls.Add(lblOrientacao);
            Controls.Add(lblSenha);
            Controls.Add(_txtSenha);
            Controls.Add(_lblConfirmarSenha);
            Controls.Add(_txtConfirmarSenha);
            Controls.Add(btnCancelar);
            Controls.Add(btnConfirmar);

            AcceptButton = btnConfirmar;
            CancelButton = btnCancelar;
        }

        public static bool SolicitarAutorizacao(IWin32Window owner)
        {
            var authService = new AdminAuthService();
            using var form = new FrmSenhaAdministrativa(authService);
            return form.ShowDialog(owner) == DialogResult.OK;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _txtSenha.Focus();
        }

        private void BtnConfirmar_Click(object? sender, EventArgs e)
        {
            string senha = _txtSenha.Text;

            if (_modoConfiguracao)
            {
                if (senha != _txtConfirmarSenha.Text)
                {
                    MessageBox.Show(
                        "As senhas informadas não são iguais.",
                        "Senha administrativa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    _txtConfirmarSenha.Clear();
                    _txtConfirmarSenha.Focus();
                    return;
                }

                try
                {
                    _authService.ConfigurarSenha(senha);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Senha administrativa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    _txtSenha.Focus();
                    _txtSenha.SelectAll();
                }

                return;
            }

            if (!_authService.ValidarSenha(senha))
            {
                MessageBox.Show(
                    "Senha administrativa incorreta.",
                    "Acesso negado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _txtSenha.Clear();
                _txtSenha.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
