namespace SistemaConferenciaPedidos
{
    partial class FrmPreparacaoPedidos
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        private void InitializeComponent()
        {
            lblTitulo = new Label();
            lblAtalhos = new Label();
            lblDataInicial = new Label();
            lblDataFinal = new Label();
            dtpDataInicial = new DateTimePicker();
            dtpDataFinal = new DateTimePicker();
            btnBuscarPedidos = new Button();
            lblPedidos = new Label();
            dgvPedidos = new DataGridView();
            lblDetalhes = new Label();
            lblCliente = new Label();
            lblPedidoCliente = new Label();
            lblMarketplace = new Label();
            lblCodigoEtiqueta = new Label();
            txtCliente = new TextBox();
            txtPedidoCliente = new TextBox();
            txtMarketplace = new TextBox();
            txtCodigoEtiqueta = new TextBox();
            lblItens = new Label();
            dgvItensPedido = new DataGridView();
            btnGerarEtiqueta = new Button();
            btnSalvarPedido = new Button();
            btnImprimirEtiqueta = new Button();
            btnAtualizarPedidos = new Button();
            btnImprimirPorProduto = new Button();

            ((System.ComponentModel.ISupportInitialize)dgvPedidos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvItensPedido).BeginInit();
            SuspendLayout();

            // lblTitulo
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitulo.Location = new Point(25, 20);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(275, 32);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Preparação de Pedidos";

            // lblDataInicial
            lblDataInicial.AutoSize = true;
            lblDataInicial.Font = new Font("Segoe UI", 10F);
            lblDataInicial.Location = new Point(27, 75);
            lblDataInicial.Name = "lblDataInicial";
            lblDataInicial.Size = new Size(76, 19);
            lblDataInicial.TabIndex = 1;
            lblDataInicial.Text = "Data Inicial";

            // dtpDataInicial
            dtpDataInicial.Format = DateTimePickerFormat.Short;
            dtpDataInicial.Location = new Point(27, 100);
            dtpDataInicial.Name = "dtpDataInicial";
            dtpDataInicial.Size = new Size(180, 23);
            dtpDataInicial.TabIndex = 3;

            // lblDataFinal
            lblDataFinal.AutoSize = true;
            lblDataFinal.Font = new Font("Segoe UI", 10F);
            lblDataFinal.Location = new Point(250, 75);
            lblDataFinal.Name = "lblDataFinal";
            lblDataFinal.Size = new Size(70, 19);
            lblDataFinal.TabIndex = 2;
            lblDataFinal.Text = "Data Final";

            // dtpDataFinal
            dtpDataFinal.Format = DateTimePickerFormat.Short;
            dtpDataFinal.Location = new Point(250, 100);
            dtpDataFinal.Name = "dtpDataFinal";
            dtpDataFinal.Size = new Size(180, 23);
            dtpDataFinal.TabIndex = 4;

            // btnBuscarPedidos
            btnBuscarPedidos.Location = new Point(460, 95);
            btnBuscarPedidos.Name = "btnBuscarPedidos";
            btnBuscarPedidos.Size = new Size(150, 32);
            btnBuscarPedidos.TabIndex = 5;
            btnBuscarPedidos.Text = "Buscar Pedidos";
            btnBuscarPedidos.UseVisualStyleBackColor = true;
            btnBuscarPedidos.Click += btnBuscarPedidos_Click;

            // btnAtualizarPedidos
            btnAtualizarPedidos.Location = new Point(639, 95);
            btnAtualizarPedidos.Name = "btnAtualizarPedidos";
            btnAtualizarPedidos.Size = new Size(150, 32);
            btnAtualizarPedidos.TabIndex = 22;
            btnAtualizarPedidos.Text = "Atualizar Pedidos";
            btnAtualizarPedidos.UseVisualStyleBackColor = true;
            btnAtualizarPedidos.Click += btnAtualizarPedidos_Click;

            // btnSalvarPedido
            btnSalvarPedido.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSalvarPedido.Location = new Point(820, 95);
            btnSalvarPedido.Name = "btnSalvarPedido";
            btnSalvarPedido.Size = new Size(180, 32);
            btnSalvarPedido.TabIndex = 20;
            btnSalvarPedido.Text = "Importar Etiquetas do Lote";
            btnSalvarPedido.UseVisualStyleBackColor = true;
            btnSalvarPedido.Click += btnSalvarPedido_Click;

            // lblPedidos
            lblPedidos.AutoSize = true;
            lblPedidos.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblPedidos.Location = new Point(27, 145);
            lblPedidos.Name = "lblPedidos";
            lblPedidos.Size = new Size(155, 20);
            lblPedidos.TabIndex = 6;
            lblPedidos.Text = "Pedidos encontrados";

            // dgvPedidos
            dgvPedidos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvPedidos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPedidos.Location = new Point(27, 175);
            dgvPedidos.Name = "dgvPedidos";
            dgvPedidos.Size = new Size(1040, 420);
            dgvPedidos.TabIndex = 7;
            dgvPedidos.CellClick += dgvPedidos_CellClick;

            // lblDetalhes
            lblDetalhes.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblDetalhes.AutoSize = true;
            lblDetalhes.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblDetalhes.Location = new Point(27, 610);
            lblDetalhes.Name = "lblDetalhes";
            lblDetalhes.Size = new Size(230, 20);
            lblDetalhes.TabIndex = 8;
            lblDetalhes.Text = "Detalhes do pedido selecionado";

            // lblCliente
            lblCliente.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblCliente.AutoSize = true;
            lblCliente.Location = new Point(27, 645);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new Size(44, 15);
            lblCliente.TabIndex = 9;
            lblCliente.Text = "Cliente";

            // txtCliente
            txtCliente.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            txtCliente.Location = new Point(27, 663);
            txtCliente.Name = "txtCliente";
            txtCliente.ReadOnly = true;
            txtCliente.Size = new Size(450, 23);
            txtCliente.TabIndex = 13;

            // lblPedidoCliente
            lblPedidoCliente.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblPedidoCliente.AutoSize = true;
            lblPedidoCliente.Location = new Point(27, 690);
            lblPedidoCliente.Name = "lblPedidoCliente";
            lblPedidoCliente.Size = new Size(84, 15);
            lblPedidoCliente.TabIndex = 10;
            lblPedidoCliente.Text = "Pedido Cliente";

            // txtPedidoCliente
            txtPedidoCliente.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            txtPedidoCliente.Location = new Point(27, 708);
            txtPedidoCliente.Name = "txtPedidoCliente";
            txtPedidoCliente.ReadOnly = true;
            txtPedidoCliente.Size = new Size(450, 23);
            txtPedidoCliente.TabIndex = 14;

            // lblMarketplace
            lblMarketplace.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblMarketplace.AutoSize = true;
            lblMarketplace.Location = new Point(510, 645);
            lblMarketplace.Name = "lblMarketplace";
            lblMarketplace.Size = new Size(72, 15);
            lblMarketplace.TabIndex = 11;
            lblMarketplace.Text = "Marketplace";

            // txtMarketplace
            txtMarketplace.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            txtMarketplace.Location = new Point(510, 663);
            txtMarketplace.Name = "txtMarketplace";
            txtMarketplace.ReadOnly = true;
            txtMarketplace.Size = new Size(250, 23);
            txtMarketplace.TabIndex = 15;

            // lblCodigoEtiqueta
            lblCodigoEtiqueta.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblCodigoEtiqueta.AutoSize = true;
            lblCodigoEtiqueta.Location = new Point(510, 690);
            lblCodigoEtiqueta.Name = "lblCodigoEtiqueta";
            lblCodigoEtiqueta.Size = new Size(92, 15);
            lblCodigoEtiqueta.TabIndex = 12;
            lblCodigoEtiqueta.Text = "Código Etiqueta";

            // txtCodigoEtiqueta
            txtCodigoEtiqueta.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            txtCodigoEtiqueta.Location = new Point(510, 708);
            txtCodigoEtiqueta.Name = "txtCodigoEtiqueta";
            txtCodigoEtiqueta.ReadOnly = true;
            txtCodigoEtiqueta.Size = new Size(250, 23);
            txtCodigoEtiqueta.TabIndex = 16;

            // btnGerarEtiqueta
            btnGerarEtiqueta.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGerarEtiqueta.Location = new Point(782, 650);
            btnGerarEtiqueta.Name = "btnGerarEtiqueta";
            btnGerarEtiqueta.Size = new Size(86, 77);
            btnGerarEtiqueta.TabIndex = 19;
            btnGerarEtiqueta.Text = "Conferir";
            btnGerarEtiqueta.UseVisualStyleBackColor = true;
            btnGerarEtiqueta.Click += btnGerarEtiqueta_Click;

            // btnImprimirPorProduto
            btnImprimirPorProduto.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnImprimirPorProduto.Location = new Point(874, 650);
            btnImprimirPorProduto.Name = "btnImprimirPorProduto";
            btnImprimirPorProduto.Size = new Size(86, 77);
            btnImprimirPorProduto.TabIndex = 23;
            btnImprimirPorProduto.Text = "Imprimir por Produto";
            btnImprimirPorProduto.UseVisualStyleBackColor = true;
            btnImprimirPorProduto.Click += btnImprimirPorProduto_Click;

            // btnImprimirEtiqueta
            btnImprimirEtiqueta.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnImprimirEtiqueta.Location = new Point(966, 650);
            btnImprimirEtiqueta.Name = "btnImprimirEtiqueta";
            btnImprimirEtiqueta.Size = new Size(86, 77);
            btnImprimirEtiqueta.TabIndex = 21;
            btnImprimirEtiqueta.Text = "Imprimir Etiqueta";
            btnImprimirEtiqueta.UseVisualStyleBackColor = true;
            btnImprimirEtiqueta.Click += btnImprimirEtiqueta_Click;

            // lblItens
            lblItens.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblItens.AutoSize = true;
            lblItens.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblItens.Location = new Point(27, 745);
            lblItens.Name = "lblItens";
            lblItens.Size = new Size(118, 20);
            lblItens.TabIndex = 17;
            lblItens.Text = "Itens do pedido";

            // dgvItensPedido
            dgvItensPedido.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvItensPedido.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvItensPedido.Location = new Point(27, 773);
            dgvItensPedido.Name = "dgvItensPedido";
            dgvItensPedido.Size = new Size(1040, 90);
            dgvItensPedido.TabIndex = 18;

            // lblAtalhos
            lblAtalhos.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            lblAtalhos.BorderStyle = BorderStyle.FixedSingle;
            lblAtalhos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAtalhos.Location = new Point(12, 875);
            lblAtalhos.Name = "lblAtalhos";
            lblAtalhos.Size = new Size(1060, 30);
            lblAtalhos.TabIndex = 999;
            lblAtalhos.Text = "ENTER = Imprimir Etiqueta    |    F4 = Buscar Produto    |    F5 = Atualizar Pedidos    |    F8 = Conferência    |    ESC = Limpar Seleção";

            // FrmPreparacaoPedidos
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 920);
            MinimumSize = new Size(1116, 959);
            MaximizeBox = true;
            MinimizeBox = true;
            Name = "FrmPreparacaoPedidos";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Preparação de Pedidos";

            Controls.Add(btnImprimirPorProduto);
            Controls.Add(btnAtualizarPedidos);
            Controls.Add(btnImprimirEtiqueta);
            Controls.Add(btnSalvarPedido);
            Controls.Add(btnGerarEtiqueta);
            Controls.Add(dgvItensPedido);
            Controls.Add(lblItens);
            Controls.Add(txtCodigoEtiqueta);
            Controls.Add(txtMarketplace);
            Controls.Add(txtPedidoCliente);
            Controls.Add(txtCliente);
            Controls.Add(lblCodigoEtiqueta);
            Controls.Add(lblMarketplace);
            Controls.Add(lblPedidoCliente);
            Controls.Add(lblCliente);
            Controls.Add(lblDetalhes);
            Controls.Add(dgvPedidos);
            Controls.Add(lblPedidos);
            Controls.Add(btnBuscarPedidos);
            Controls.Add(dtpDataFinal);
            Controls.Add(dtpDataInicial);
            Controls.Add(lblDataFinal);
            Controls.Add(lblAtalhos);
            Controls.Add(lblDataInicial);
            Controls.Add(lblTitulo);

            ((System.ComponentModel.ISupportInitialize)dgvPedidos).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvItensPedido).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblDataInicial;
        private System.Windows.Forms.Label lblDataFinal;
        private System.Windows.Forms.DateTimePicker dtpDataInicial;
        private System.Windows.Forms.DateTimePicker dtpDataFinal;
        private System.Windows.Forms.Button btnBuscarPedidos;
        private System.Windows.Forms.Label lblPedidos;
        private System.Windows.Forms.DataGridView dgvPedidos;
        private System.Windows.Forms.Label lblDetalhes;
        private System.Windows.Forms.Label lblCliente;
        private System.Windows.Forms.Label lblPedidoCliente;
        private System.Windows.Forms.Label lblMarketplace;
        private System.Windows.Forms.Label lblCodigoEtiqueta;
        private System.Windows.Forms.TextBox txtCliente;
        private System.Windows.Forms.TextBox txtPedidoCliente;
        private System.Windows.Forms.TextBox txtMarketplace;
        private System.Windows.Forms.TextBox txtCodigoEtiqueta;
        private System.Windows.Forms.Label lblItens;
        private System.Windows.Forms.DataGridView dgvItensPedido;
        private System.Windows.Forms.Button btnGerarEtiqueta;
        private System.Windows.Forms.Button btnSalvarPedido;
        private System.Windows.Forms.Button btnImprimirEtiqueta;
        private Button btnAtualizarPedidos;
        private Button btnImprimirPorProduto;
        private Label lblAtalhos;
    }
}