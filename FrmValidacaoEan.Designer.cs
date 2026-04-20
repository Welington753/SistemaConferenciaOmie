namespace SistemaConferenciaPedidos
{
    partial class FrmValidacaoEan
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos sendo usados.
        /// </summary>
        /// <param name="disposing">true se os recursos gerenciados devem ser descartados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblResumo = new System.Windows.Forms.Label();
            this.txtLeitura = new System.Windows.Forms.TextBox();
            this.btnConfirmar = new System.Windows.Forms.Button();
            this.lblPendentes = new System.Windows.Forms.Label();
            this.lstPendentes = new System.Windows.Forms.ListBox();
            this.lblHistorico = new System.Windows.Forms.Label();
            this.lstHistorico = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(12, 12);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(960, 42);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Pedido:";
            // 
            // lblResumo
            // 
            this.lblResumo.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblResumo.Location = new System.Drawing.Point(12, 56);
            this.lblResumo.Name = "lblResumo";
            this.lblResumo.Size = new System.Drawing.Size(960, 28);
            this.lblResumo.TabIndex = 1;
            this.lblResumo.Text = "Resumo";
            // 
            // txtLeitura
            // 
            this.txtLeitura.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.txtLeitura.Location = new System.Drawing.Point(16, 96);
            this.txtLeitura.Name = "txtLeitura";
            this.txtLeitura.Size = new System.Drawing.Size(430, 32);
            this.txtLeitura.TabIndex = 2;
            this.txtLeitura.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtLeitura_KeyDown);
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);
            // 
            // btnConfirmar
            // 
            this.btnConfirmar.Enabled = false;
            this.btnConfirmar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnConfirmar.Location = new System.Drawing.Point(462, 95);
            this.btnConfirmar.Name = "btnConfirmar";
            this.btnConfirmar.Size = new System.Drawing.Size(190, 34);
            this.btnConfirmar.TabIndex = 3;
            this.btnConfirmar.Text = "Confirmar e imprimir";
            this.btnConfirmar.UseVisualStyleBackColor = true;
            this.btnConfirmar.Visible = false;
            this.btnConfirmar.Click += new System.EventHandler(this.btnConfirmar_Click);
            // 
            // lblPendentes
            // 
            this.lblPendentes.AutoSize = true;
            this.lblPendentes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPendentes.Location = new System.Drawing.Point(13, 145);
            this.lblPendentes.Name = "lblPendentes";
            this.lblPendentes.Size = new System.Drawing.Size(95, 15);
            this.lblPendentes.TabIndex = 4;
            this.lblPendentes.Text = "Itens pendentes";
            // 
            // lstPendentes
            // 
            this.lstPendentes.Font = new System.Drawing.Font("Consolas", 10F);
            this.lstPendentes.FormattingEnabled = true;
            this.lstPendentes.ItemHeight = 15;
            this.lstPendentes.Location = new System.Drawing.Point(16, 165);
            this.lstPendentes.Name = "lstPendentes";
            this.lstPendentes.Size = new System.Drawing.Size(460, 334);
            this.lstPendentes.TabIndex = 5;
            // 
            // lblHistorico
            // 
            this.lblHistorico.AutoSize = true;
            this.lblHistorico.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblHistorico.Location = new System.Drawing.Point(495, 145);
            this.lblHistorico.Name = "lblHistorico";
            this.lblHistorico.Size = new System.Drawing.Size(112, 15);
            this.lblHistorico.TabIndex = 6;
            this.lblHistorico.Text = "Histórico de leitura";
            // 
            // lstHistorico
            // 
            this.lstHistorico.Font = new System.Drawing.Font("Consolas", 10F);
            this.lstHistorico.FormattingEnabled = true;
            this.lstHistorico.ItemHeight = 15;
            this.lstHistorico.Location = new System.Drawing.Point(498, 165);
            this.lstHistorico.Name = "lstHistorico";
            this.lstHistorico.Size = new System.Drawing.Size(474, 334);
            this.lstHistorico.TabIndex = 7;
            // 
            // FrmValidacaoEan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 521);
            this.Controls.Add(this.lstHistorico);
            this.Controls.Add(this.lblHistorico);
            this.Controls.Add(this.lstPendentes);
            this.Controls.Add(this.lblPendentes);
            this.Controls.Add(this.btnConfirmar);
            this.Controls.Add(this.txtLeitura);
            this.Controls.Add(this.lblResumo);
            this.Controls.Add(this.lblTitulo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmValidacaoEan";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Validação dos EANs antes da impressão";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblResumo;
        private System.Windows.Forms.TextBox txtLeitura;
        private System.Windows.Forms.Button btnConfirmar;
        private System.Windows.Forms.Label lblPendentes;
        private System.Windows.Forms.ListBox lstPendentes;
        private System.Windows.Forms.Label lblHistorico;
        private System.Windows.Forms.ListBox lstHistorico;
    }
}