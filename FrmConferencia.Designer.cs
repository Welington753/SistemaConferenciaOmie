namespace SistemaConferenciaPedidos
{
    partial class FrmConferencia
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            grpPedidosFaltantes = new GroupBox();
            grpFaltantesAmazon = new GroupBox();
            grpFaltantesShopee = new GroupBox();
            grpFaltantesMl = new GroupBox();
            lstFaltantesAmazon = new ListBox();
            lstFaltantesShopee = new ListBox();
            lstFaltantesMl = new ListBox();
            grpHistorico = new GroupBox();
            lstErros = new ListBox();
            lstSucesso = new ListBox();
            lblTitulo = new Label();
            lblLeitura = new Label();
            txtLeitura = new TextBox();
            btnConferir = new Button();
            grpResumo = new GroupBox();
            lblResumoShopee = new Label();
            lblResumoAmazon = new Label();
            lblResumoMl = new Label();
            lblResumoGeral = new Label();
            grpHistorico = new GroupBox();
            lstErros = new ListBox();
            lstSucesso = new ListBox();
     
            grpResumo.SuspendLayout();
            grpHistorico.SuspendLayout();
            grpPedidosFaltantes.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitulo.Location = new Point(24, 18);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(318, 37);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Conferência de Pedidos";
            // 
            // lblLeitura
            // 
            lblLeitura.AutoSize = true;
            lblLeitura.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblLeitura.Location = new Point(28, 73);
            lblLeitura.Name = "lblLeitura";
            lblLeitura.Size = new Size(185, 21);
            lblLeitura.TabIndex = 1;
            lblLeitura.Text = "Bipe o código da etiqueta";
            // 
            // txtLeitura
            // 
            txtLeitura.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtLeitura.Location = new Point(28, 98);
            txtLeitura.Name = "txtLeitura";
            txtLeitura.Size = new Size(690, 39);
            txtLeitura.TabIndex = 2;
            txtLeitura.KeyDown += txtLeitura_KeyDown;
            // 
            // btnConferir
            // 
            btnConferir.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnConferir.Location = new Point(28, 149);
            btnConferir.Name = "btnConferir";
            btnConferir.Size = new Size(690, 42);
            btnConferir.TabIndex = 3;
            btnConferir.Text = "Conferir";
            btnConferir.UseVisualStyleBackColor = true;
            btnConferir.Click += btnConferir_Click;
            // 
            // grpResumo
            // 
            grpResumo.Controls.Add(lblResumoShopee);
            grpResumo.Controls.Add(lblResumoAmazon);
            grpResumo.Controls.Add(lblResumoMl);
            grpResumo.Controls.Add(lblResumoGeral);
            grpResumo.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpResumo.Location = new Point(28, 207);
            grpResumo.Name = "grpResumo";
            grpResumo.Size = new Size(690, 170);
            grpResumo.TabIndex = 4;
            grpResumo.TabStop = false;
            grpResumo.Text = "Resumo da Conferência";
            // 
            // lblResumoShopee
            // 
            lblResumoShopee.BorderStyle = BorderStyle.FixedSingle;
            lblResumoShopee.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblResumoShopee.Location = new Point(16, 141);
            lblResumoShopee.Name = "lblResumoShopee";
            lblResumoShopee.Size = new Size(658, 27);
            lblResumoShopee.TabIndex = 3;
            lblResumoShopee.Text = "Shopee: Total 0 | Conferidos 0 | Faltam 0";
            lblResumoShopee.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblResumoAmazon
            // 
            lblResumoAmazon.BorderStyle = BorderStyle.FixedSingle;
            lblResumoAmazon.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblResumoAmazon.Location = new Point(16, 106);
            lblResumoAmazon.Name = "lblResumoAmazon";
            lblResumoAmazon.Size = new Size(658, 27);
            lblResumoAmazon.TabIndex = 2;
            lblResumoAmazon.Text = "Amazon: Total 0 | Conferidos 0 | Faltam 0";
            lblResumoAmazon.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblResumoMl
            // 
            lblResumoMl.BorderStyle = BorderStyle.FixedSingle;
            lblResumoMl.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblResumoMl.Location = new Point(16, 71);
            lblResumoMl.Name = "lblResumoMl";
            lblResumoMl.Size = new Size(658, 27);
            lblResumoMl.TabIndex = 1;
            lblResumoMl.Text = "Mercado Livre: Total 0 | Conferidos 0 | Faltam 0";
            lblResumoMl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblResumoGeral
            // 
            lblResumoGeral.BorderStyle = BorderStyle.FixedSingle;
            lblResumoGeral.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblResumoGeral.Location = new Point(16, 26);
            lblResumoGeral.Name = "lblResumoGeral";
            lblResumoGeral.Size = new Size(658, 34);
            lblResumoGeral.TabIndex = 0;
            lblResumoGeral.Text = "TOTAL DO DIA: 0 | CONFERIDOS: 0 | FALTAM: 0";
            lblResumoGeral.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // grpHistorico
            grpHistorico.Controls.Add(lstErros);
            grpHistorico.Controls.Add(lstSucesso);
            grpHistorico.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpHistorico.Location = new Point(28, 392);
            grpHistorico.Name = "grpHistorico";
            grpHistorico.Size = new Size(690, 262);
            grpHistorico.TabIndex = 5;
            grpHistorico.TabStop = false;
            grpHistorico.Text = "Histórico";
            // 
            // lstErros
            // 
            lstErros.BackColor = Color.MistyRose;
            lstErros.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lstErros.ForeColor = Color.DarkRed;
            lstErros.FormattingEnabled = true;
            lstErros.Location = new Point(348, 28);
            lstErros.Name = "lstErros";
            lstErros.Size = new Size(326, 208);
            lstErros.TabIndex = 1;
            // 
            // lstSucesso
            // 
            lstSucesso.BackColor = Color.Honeydew;
            lstSucesso.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lstSucesso.ForeColor = Color.DarkGreen;
            lstSucesso.FormattingEnabled = true;
            lstSucesso.Location = new Point(16, 28);
            lstSucesso.Name = "lstSucesso";
            lstSucesso.Size = new Size(310, 208);
            lstSucesso.TabIndex = 0;

            // 
            // grpPedidosFaltantes
            // 
            grpPedidosFaltantes.Controls.Add(grpFaltantesAmazon);
            grpPedidosFaltantes.Controls.Add(grpFaltantesShopee);
            grpPedidosFaltantes.Controls.Add(grpFaltantesMl);
            grpPedidosFaltantes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpPedidosFaltantes.Location = new Point(735, 18);
            grpPedidosFaltantes.Name = "grpPedidosFaltantes";
            grpPedidosFaltantes.Size = new Size(500, 636);
            grpPedidosFaltantes.TabIndex = 6;
            grpPedidosFaltantes.TabStop = false;
            grpPedidosFaltantes.Text = "Pedidos faltantes";

            // 
            // grpFaltantesAmazon
            // 
            grpFaltantesAmazon.Controls.Add(lstFaltantesAmazon);
            grpFaltantesAmazon.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpFaltantesAmazon.Location = new Point(14, 28);
            grpFaltantesAmazon.Name = "grpFaltantesAmazon";
            grpFaltantesAmazon.Size = new Size(470, 190);
            grpFaltantesAmazon.TabIndex = 0;
            grpFaltantesAmazon.TabStop = false;
            grpFaltantesAmazon.Text = "Amazon";

            // 
            // lstFaltantesAmazon
            // 
            lstFaltantesAmazon.Font = new Font("Segoe UI", 10F);
            lstFaltantesAmazon.FormattingEnabled = true;
            lstFaltantesAmazon.ItemHeight = 17;
            lstFaltantesAmazon.Location = new Point(10, 25);
            lstFaltantesAmazon.Name = "lstFaltantesAmazon";
            lstFaltantesAmazon.Size = new Size(450, 150);
            lstFaltantesAmazon.TabIndex = 0;

            // 
            // grpFaltantesShopee
            // 
            grpFaltantesShopee.Controls.Add(lstFaltantesShopee);
            grpFaltantesShopee.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpFaltantesShopee.Location = new Point(14, 225);
            grpFaltantesShopee.Name = "grpFaltantesShopee";
            grpFaltantesShopee.Size = new Size(470, 190);
            grpFaltantesShopee.TabIndex = 1;
            grpFaltantesShopee.TabStop = false;
            grpFaltantesShopee.Text = "Shopee";

            // 
            // lstFaltantesShopee
            // 
            lstFaltantesShopee.Font = new Font("Segoe UI", 10F);
            lstFaltantesShopee.FormattingEnabled = true;
            lstFaltantesShopee.ItemHeight = 17;
            lstFaltantesShopee.Location = new Point(10, 25);
            lstFaltantesShopee.Name = "lstFaltantesShopee";
            lstFaltantesShopee.Size = new Size(450, 150);
            lstFaltantesShopee.TabIndex = 0;

            // 
            // grpFaltantesMl
            // 
            grpFaltantesMl.Controls.Add(lstFaltantesMl);
            grpFaltantesMl.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grpFaltantesMl.Location = new Point(14, 422);
            grpFaltantesMl.Name = "grpFaltantesMl";
            grpFaltantesMl.Size = new Size(470, 190);
            grpFaltantesMl.TabIndex = 2;
            grpFaltantesMl.TabStop = false;
            grpFaltantesMl.Text = "Mercado Livre";

            // 
            // lstFaltantesMl
            // 
            lstFaltantesMl.Font = new Font("Segoe UI", 10F);
            lstFaltantesMl.FormattingEnabled = true;
            lstFaltantesMl.ItemHeight = 17;
            lstFaltantesMl.Location = new Point(10, 25);
            lstFaltantesMl.Name = "lstFaltantesMl";
            lstFaltantesMl.Size = new Size(450, 150);
            lstFaltantesMl.TabIndex = 0;
            // FrmConferencia
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1260, 673);
            Controls.Add(grpHistorico);
            Controls.Add(grpPedidosFaltantes);
            Controls.Add(grpResumo);
            Controls.Add(btnConferir);
            Controls.Add(txtLeitura);
            Controls.Add(lblLeitura);
            Controls.Add(lblTitulo);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmConferencia";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Conferência de Pedidos";
            grpResumo.ResumeLayout(false);
            grpHistorico.ResumeLayout(false);
            grpPedidosFaltantes.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitulo;
        private Label lblLeitura;
        private TextBox txtLeitura;
        private Button btnConferir;
        private GroupBox grpResumo;
        private Label lblResumoGeral;
        private Label lblResumoMl;
        private Label lblResumoAmazon;
        private Label lblResumoShopee;
        private GroupBox grpHistorico;
        private ListBox lstSucesso;
        private ListBox lstErros;
        private GroupBox grpPedidosFaltantes;
        private GroupBox grpFaltantesAmazon;
        private GroupBox grpFaltantesShopee;
        private GroupBox grpFaltantesMl;
        private ListBox lstFaltantesAmazon;
        private ListBox lstFaltantesShopee;
        private ListBox lstFaltantesMl;

    }
}