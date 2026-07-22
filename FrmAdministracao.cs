using System;
using System.Drawing;
using System.Windows.Forms;
using SistemaConferenciaPedidos.Repositories;
using SistemaConferenciaPedidos.Data;

namespace SistemaConferenciaPedidos
{
    public class FrmAdministracao : Form
    {
        private readonly IPedidoRepository _pedidoRepository = new PedidoRepositorySqlite();
        
        private Button btnExcluirPedido;
        private Button btnResetarBanco;
        private TextBox txtNumeroPedido;
        private Label lblInstrucao;
        private Label lblEstatisticas;
        
        private Label lblDataReset;
        private DateTimePicker dtpDataReset;
        
        private Button btnAlterarSenha;

        public FrmAdministracao()
        {
            Text = "Administração do Sistema";
            Size = new Size(400, 420);
            StartPosition = FormStartPosition.CenterScreen;

            lblInstrucao = new Label
            {
                Text = "Número do Pedido para Ocultar/Remover:",
                Location = new Point(20, 20),
                AutoSize = true
            };

            txtNumeroPedido = new TextBox
            {
                Location = new Point(20, 40),
                Width = 200
            };

            btnExcluirPedido = new Button
            {
                Text = "Remover Pedido",
                Location = new Point(230, 38),
                Width = 120
            };
            btnExcluirPedido.Click += BtnExcluirPedido_Click;

            lblDataReset = new Label
            {
                Text = "Data para Reset:",
                Location = new Point(20, 85),
                AutoSize = true
            };

            dtpDataReset = new DateTimePicker
            {
                Location = new Point(130, 80),
                Format = DateTimePickerFormat.Short,
                Width = 120
            };

            btnResetarBanco = new Button
            {
                Text = "Resetar Status dos Pedidos do Dia",
                Location = new Point(20, 115),
                Width = 330,
                Height = 40,
                ForeColor = Color.DarkOrange
            };
            btnResetarBanco.Click += BtnResetarBanco_Click;

            btnAlterarSenha = new Button
            {
                Text = "Alterar Senha Administrativa",
                Location = new Point(20, 200),
                Width = 330,
                Height = 40
            };
            btnAlterarSenha.Click += BtnAlterarSenha_Click;

            lblEstatisticas = new Label
            {
                Location = new Point(20, 260),
                AutoSize = true
            };

            Controls.Add(lblInstrucao);
            Controls.Add(txtNumeroPedido);
            Controls.Add(btnExcluirPedido);
            Controls.Add(lblDataReset);
            Controls.Add(dtpDataReset);
            Controls.Add(btnResetarBanco);
            Controls.Add(btnAlterarSenha);
            Controls.Add(lblEstatisticas);

            AtualizarEstatisticas();
        }

        private void BtnAlterarSenha_Click(object sender, EventArgs e)
        {
            if (!FrmSenhaAdministrativa.SolicitarAutorizacao(this))
                return;

            var authService = new SistemaConferenciaPedidos.Services.AdminAuthService();
            var prompt = new FrmSenhaAdministrativaRedefinir(authService);
            if (prompt.ShowDialog(this) == DialogResult.OK)
            {
                authService.RegistrarAcao("ALTERAR_SENHA_ADMIN");
                MessageBox.Show("Senha alterada com sucesso.");
            }
        }

        private void AtualizarEstatisticas()
        {
            try
            {
                var pedidos = _pedidoRepository.ObterTodos();
                int total = pedidos.Count;
                lblEstatisticas.Text = $"Total de Pedidos no Banco (Ativos): {total}";
            }
            catch (Exception ex)
            {
                lblEstatisticas.Text = "Erro ao carregar estatísticas: " + ex.Message;
            }
        }

        private void BtnExcluirPedido_Click(object sender, EventArgs e)
        {
            string numero = txtNumeroPedido.Text.Trim();
            if (string.IsNullOrWhiteSpace(numero))
            {
                MessageBox.Show("Digite o número do pedido.");
                return;
            }

            if (!FrmSenhaAdministrativa.SolicitarAutorizacao(this))
                return;

            var confirm = MessageBox.Show($"Deseja realmente ocultar o pedido {numero}?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    bool sucesso = _pedidoRepository.OcultarPedido(numero);
                    if (sucesso)
                    {
                        var authService = new SistemaConferenciaPedidos.Services.AdminAuthService();
                        authService.RegistrarAcao("REMOVER_PEDIDO_PREPARACAO", numero);

                        MessageBox.Show("Pedido ocultado com sucesso!");
                        txtNumeroPedido.Clear();
                        AtualizarEstatisticas();
                    }
                    else
                    {
                        MessageBox.Show("Pedido não encontrado ou já ocultado.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro: " + ex.Message);
                }
            }
        }

        private void BtnResetarBanco_Click(object sender, EventArgs e)
        {
            var data = dtpDataReset.Value.Date;
            
            var pedidosDoDia = _pedidoRepository.ObterPorPeriodo(data, data.AddDays(1), true);
            
            int total = pedidosDoDia.Count;
            if (total == 0)
            {
                MessageBox.Show($"Nenhum pedido encontrado para a data {data:dd/MM/yyyy}.", "Resetar Status do Dia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            int impressos = pedidosDoDia.Count(p => p.Impresso);
            int conferidos = pedidosDoDia.Count(p => p.Conferido);
            int comEtiqueta = pedidosDoDia.Count(p => !string.IsNullOrEmpty(p.CodigoEtiqueta));
            int comPdf = pedidosDoDia.Count(p => !string.IsNullOrEmpty(p.CaminhoZipImportacao));
            int reimpressoes = pedidosDoDia.Count(p => p.DataReimpressao.HasValue);
            int removidos = pedidosDoDia.Count(p => p.Oculto);
            
            var dialog = new FrmResetConfirmacao(data, total, impressos, conferidos, comEtiqueta, comPdf, reimpressoes, removidos);
            if (dialog.ShowDialog(this) != DialogResult.Yes)
                return;
                
            bool restaurarRemovidos = dialog.RestaurarRemovidos;

            if (!FrmSenhaAdministrativa.SolicitarAutorizacao(this))
                return;

            string motivo = FrmMotivoReimpressao.SolicitarMotivo(this); // Aproveitando o mesmo formulário para motivo
            if (string.IsNullOrWhiteSpace(motivo))
            {
                MessageBox.Show("Operação cancelada (nenhum motivo fornecido).", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Faz o backup
                var backupService = new SistemaConferenciaPedidos.Services.BackupBancoService();
                backupService.RealizarBackup();

                bool alterou = _pedidoRepository.LimparPedidosPorDia(data, restaurarRemovidos);
                if (alterou)
                {
                    var authService = new SistemaConferenciaPedidos.Services.AdminAuthService();
                    authService.RegistrarAcao("RESET_DIA_OPERACIONAL", "", $"Data alvo: {data:dd/MM/yyyy} - Motivo: {motivo}");
                    MessageBox.Show($"Status dos pedidos do dia {data:dd/MM/yyyy} resetados com sucesso.");
                }
                else
                    MessageBox.Show($"Nenhum pedido operacional para alterar na data {data:dd/MM/yyyy}.");
                AtualizarEstatisticas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao resetar: " + ex.Message);
            }
        }
    }

    public sealed class FrmResetConfirmacao : Form
    {
        public bool RestaurarRemovidos => _chkRestaurarRemovidos.Checked;
        private readonly CheckBox _chkRestaurarRemovidos;

        public FrmResetConfirmacao(DateTime data, int total, int impressos, int conferidos, int comEtiqueta, int comPdf, int reimpressoes, int removidos)
        {
            Text = "Confirmação de Reset";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(400, 360);

            string msg = $"Resetar os dados operacionais de {data:dd/MM/yyyy}?\n\n" +
                         $"Pedidos: {total}\n" +
                         $"Impressos: {impressos}\n" +
                         $"Conferidos: {conferidos}\n" +
                         $"Etiquetas vinculadas: {comEtiqueta}\n" +
                         $"Referências PDF/ZIP: {comPdf}\n" +
                         $"Reimpressões: {reimpressoes}\n" +
                         $"Removidos: {removidos}\n\n" +
                         "Outras datas não serão afetadas.";

            var lblMsg = new Label { Location = new Point(20, 20), Size = new Size(360, 220), Text = msg, Font = new Font(Font.FontFamily, 10) };
            
            _chkRestaurarRemovidos = new CheckBox 
            { 
                Location = new Point(20, 260), 
                Size = new Size(360, 25), 
                Text = "Restaurar também pedidos removidos (ocultos) deste dia",
                Enabled = removidos > 0
            };

            var btnNao = new Button { DialogResult = DialogResult.No, Location = new Point(210, 310), Size = new Size(80, 30), Text = "Não" };
            var btnSim = new Button { DialogResult = DialogResult.Yes, Location = new Point(300, 310), Size = new Size(80, 30), Text = "Sim, resetar" };

            Controls.Add(lblMsg);
            Controls.Add(_chkRestaurarRemovidos);
            Controls.Add(btnNao);
            Controls.Add(btnSim);

            AcceptButton = btnNao; // Padrão "Não"
            CancelButton = btnNao;
        }
    }

    public sealed class FrmSenhaAdministrativaRedefinir : Form
    {
        private readonly TextBox _txtSenha = new TextBox();
        private readonly TextBox _txtConfirmarSenha = new TextBox();
        private readonly SistemaConferenciaPedidos.Services.AdminAuthService _authService;

        public FrmSenhaAdministrativaRedefinir(SistemaConferenciaPedidos.Services.AdminAuthService authService)
        {
            _authService = authService;
            Text = "Redefinir senha administrativa";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(430, 230);

            var lblOrientacao = new Label { AutoSize = false, Location = new Point(20, 18), Size = new Size(390, 44), Text = "Crie uma nova senha administrativa." };
            var lblSenha = new Label { AutoSize = true, Location = new Point(20, 72), Text = "Nova senha" };
            _txtSenha.Location = new Point(20, 94); _txtSenha.Size = new Size(390, 23); _txtSenha.UseSystemPasswordChar = true;

            var lblConfirmarSenha = new Label { AutoSize = true, Location = new Point(20, 126), Text = "Confirmar nova senha" };
            _txtConfirmarSenha.Location = new Point(20, 148); _txtConfirmarSenha.Size = new Size(390, 23); _txtConfirmarSenha.UseSystemPasswordChar = true;

            var btnCancelar = new Button { DialogResult = DialogResult.Cancel, Location = new Point(230, 188), Size = new Size(85, 30), Text = "Cancelar" };
            var btnConfirmar = new Button { Location = new Point(325, 188), Size = new Size(85, 30), Text = "Confirmar" };
            
            btnConfirmar.Click += (s, e) =>
            {
                if (_txtSenha.Text != _txtConfirmarSenha.Text)
                {
                    MessageBox.Show("As senhas informadas não são iguais.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    _authService.ConfigurarSenha(_txtSenha.Text);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (ArgumentException ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };

            Controls.Add(lblOrientacao); Controls.Add(lblSenha); Controls.Add(_txtSenha); Controls.Add(lblConfirmarSenha); Controls.Add(_txtConfirmarSenha); Controls.Add(btnCancelar); Controls.Add(btnConfirmar);
            AcceptButton = btnConfirmar; CancelButton = btnCancelar;
        }
    }
}
