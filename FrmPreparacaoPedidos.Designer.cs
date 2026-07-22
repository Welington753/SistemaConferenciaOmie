namespace SistemaConferenciaPedidos
{
    partial class FrmPreparacaoPedidos
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel layoutPrincipal;
        private Panel panelTopo;
        private Panel panelFiltros;
        private Panel panelPedidos;
        private Panel panelDetalhes;
        private Panel panelItens;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        private void InitializeComponent()
        {
            layoutPrincipal = new TableLayoutPanel();
            panelTopo = new Panel();
            panelFiltros = new Panel();
            panelPedidos = new Panel();
            panelDetalhes = new Panel();
            panelItens = new Panel();

            lblTitulo = new Label();
            lblAtalhos = new Label();
            lblDataInicial = new Label();
            lblDataFinal = new Label();
            dtpDataInicial = new DateTimePicker();
            dtpDataFinal = new DateTimePicker();
            btnBuscarPedidos = new Button();
            btnAtualizarPedidos = new Button();
            btnSalvarPedido = new Button();
            btnExcluirPedido = new Button();
            btnAdministracao = new Button();
            btnValidarVinculos = new Button();
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
            btnGerarEtiqueta = new Button();
            btnImprimirPorProduto = new Button();
            btnImprimirEtiqueta = new Button();
            lblItens = new Label();
            dgvItensPedido = new DataGridView();
            panelResumo = new Panel();
            lblResumoTotal = new Label();
            lblResumoPreparados = new Label();
            lblResumoFaltam = new Label();
            lblResumoPercentual = new Label();
            pbProgressoResumo = new ProgressBar();
            chkSomenteFaltantes = new CheckBox();
            panelDataOperacional = new Panel();
            lblDataOperacional = new Label();
            lblUltimoImpresso = new Label();

            ((System.ComponentModel.ISupportInitialize)dgvPedidos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvItensPedido).BeginInit();
            SuspendLayout();

            // layoutPrincipal
            layoutPrincipal.ColumnCount = 1;
            layoutPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutPrincipal.Dock = DockStyle.Fill;
            layoutPrincipal.RowCount = 6;
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
            layoutPrincipal.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));

            // panelTopo
            panelTopo.Dock = DockStyle.Fill;
            panelTopo.Controls.Add(lblTitulo);
            panelTopo.Controls.Add(btnAdministracao);
            panelTopo.Controls.Add(panelDataOperacional);
            
            // panelDataOperacional
            panelDataOperacional.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panelDataOperacional.Location = new Point(800, 15);
            panelDataOperacional.Size = new Size(250, 40);
            panelDataOperacional.BackColor = Color.Orange;
            panelDataOperacional.Controls.Add(lblDataOperacional);
            panelDataOperacional.Visible = false; // será gerenciado pelo código
            
            // lblDataOperacional
            lblDataOperacional.Dock = DockStyle.Fill;
            lblDataOperacional.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblDataOperacional.TextAlign = ContentAlignment.MiddleCenter;
            lblDataOperacional.Text = "Data Operacional: 00/00/0000";

            // lblTitulo
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitulo.Location = new Point(20, 18);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(275, 32);
            lblTitulo.Text = "Preparação de Pedidos";

            panelFiltros.Dock = DockStyle.Fill;
            panelFiltros.Controls.Add(lblDataInicial);
            panelFiltros.Controls.Add(dtpDataInicial);
            panelFiltros.Controls.Add(lblDataFinal);
            panelFiltros.Controls.Add(dtpDataFinal);
            panelFiltros.Controls.Add(chkSomenteFaltantes);
            panelFiltros.Controls.Add(btnBuscarPedidos);
            panelFiltros.Controls.Add(btnAtualizarPedidos);
            panelFiltros.Controls.Add(btnSalvarPedido);
            panelFiltros.Controls.Add(btnValidarVinculos);

            lblDataInicial.AutoSize = true;
            lblDataInicial.Location = new Point(20, 5);
            lblDataInicial.Text = "Data Inicial";

            dtpDataInicial.Format = DateTimePickerFormat.Short;
            dtpDataInicial.Location = new Point(20, 30);
            dtpDataInicial.Size = new Size(180, 23);

            lblDataFinal.AutoSize = true;
            lblDataFinal.Location = new Point(240, 5);
            lblDataFinal.Text = "Data Final";

            dtpDataFinal.Format = DateTimePickerFormat.Short;
            dtpDataFinal.Location = new Point(240, 30);
            dtpDataFinal.Size = new Size(180, 23);
            dtpDataInicial.ValueChanged += dtpData_ValueChanged;
            dtpDataFinal.ValueChanged += dtpData_ValueChanged;

            // chkSomenteFaltantes
            chkSomenteFaltantes.AutoSize = true;
            chkSomenteFaltantes.Location = new Point(440, 30);
            chkSomenteFaltantes.Text = "Mostrar somente faltantes";
            chkSomenteFaltantes.CheckedChanged += chkSomenteFaltantes_CheckedChanged;

            btnBuscarPedidos.Location = new Point(620, 25);
            btnBuscarPedidos.Size = new Size(110, 32);
            btnBuscarPedidos.Text = "Buscar Pedidos";
            btnBuscarPedidos.UseVisualStyleBackColor = true;
            btnBuscarPedidos.Click += btnBuscarPedidos_Click;

            btnAtualizarPedidos.Location = new Point(740, 25);
            btnAtualizarPedidos.Size = new Size(110, 32);
            btnAtualizarPedidos.Text = "Atualizar Pedidos";
            btnAtualizarPedidos.UseVisualStyleBackColor = true;
            btnAtualizarPedidos.Click += btnAtualizarPedidos_Click;

            btnSalvarPedido.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnSalvarPedido.Location = new Point(860, 25);
            btnSalvarPedido.Size = new Size(160, 32);
            btnSalvarPedido.Text = "Importar Etiquetas do Lote";
            btnSalvarPedido.UseVisualStyleBackColor = true;
            btnSalvarPedido.Click += btnSalvarPedido_Click;

            btnValidarVinculos.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnValidarVinculos.Location = new Point(1030, 25);
            btnValidarVinculos.Size = new Size(150, 32);
            btnValidarVinculos.Text = "Validar Vínculos do Dia";
            btnValidarVinculos.UseVisualStyleBackColor = true;
            btnValidarVinculos.Click += btnValidarVinculos_Click;

            btnAdministracao.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnAdministracao.Location = new Point(320, 18);
            btnAdministracao.Size = new Size(150, 32);
            btnAdministracao.Text = "⚙ Administração";
            btnAdministracao.UseVisualStyleBackColor = true;
            btnAdministracao.Click += btnAdministracao_Click;

            // panelPedidos
            panelPedidos.Dock = DockStyle.Fill;
            panelPedidos.Padding = new Padding(20, 0, 20, 5);
            panelPedidos.Controls.Add(dgvPedidos);
            panelPedidos.Controls.Add(panelResumo);
            panelPedidos.Controls.Add(lblPedidos);
            panelPedidos.Controls.Add(btnExcluirPedido);

            lblPedidos.Dock = DockStyle.Top;
            lblPedidos.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblPedidos.Height = 28;
            lblPedidos.Text = "Pedidos encontrados";

            // panelResumo
            panelResumo.Dock = DockStyle.Top;
            panelResumo.Height = 32;
            panelResumo.Controls.Add(lblResumoTotal);
            panelResumo.Controls.Add(lblResumoPreparados);
            panelResumo.Controls.Add(lblResumoFaltam);
            panelResumo.Controls.Add(lblResumoPercentual);
            panelResumo.Controls.Add(pbProgressoResumo);
            panelResumo.Controls.Add(lblUltimoImpresso);

            lblResumoTotal.AutoSize = true;
            lblResumoTotal.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResumoTotal.ForeColor = Color.FromArgb(50, 50, 50);
            lblResumoTotal.Location = new Point(0, 5);
            lblResumoTotal.Text = "Total: 0";

            lblResumoPreparados.AutoSize = true;
            lblResumoPreparados.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResumoPreparados.ForeColor = Color.ForestGreen;
            lblResumoPreparados.Location = new Point(120, 5);
            lblResumoPreparados.Text = "Preparados: 0";

            lblResumoFaltam.AutoSize = true;
            lblResumoFaltam.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResumoFaltam.ForeColor = Color.DarkOrange;
            lblResumoFaltam.Location = new Point(270, 5);
            lblResumoFaltam.Text = "Faltam: 0";

            lblResumoPercentual.AutoSize = true;
            lblResumoPercentual.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblResumoPercentual.ForeColor = Color.FromArgb(30, 30, 30);
            lblResumoPercentual.Location = new Point(390, 5);
            lblResumoPercentual.Text = "Progresso: 0%";

            pbProgressoResumo.Location = new Point(520, 5);
            pbProgressoResumo.Size = new Size(200, 18);
            pbProgressoResumo.Minimum = 0;
            pbProgressoResumo.Maximum = 100;
            pbProgressoResumo.Value = 0;

            lblUltimoImpresso.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUltimoImpresso.AutoSize = true;
            lblUltimoImpresso.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUltimoImpresso.ForeColor = Color.Blue;
            lblUltimoImpresso.Location = new Point(740, 5);
            lblUltimoImpresso.Text = "Último: Nenhum";
            lblUltimoImpresso.TextAlign = ContentAlignment.MiddleRight;

            btnExcluirPedido.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExcluirPedido.BackColor = Color.MistyRose;
            btnExcluirPedido.Location = new Point(930, 0);
            btnExcluirPedido.Size = new Size(130, 27);
            btnExcluirPedido.Text = "Remover pedido";
            btnExcluirPedido.UseVisualStyleBackColor = false;
            btnExcluirPedido.Click += btnExcluirPedido_Click;
            btnExcluirPedido.BringToFront();

            dgvPedidos.Dock = DockStyle.Fill;
            dgvPedidos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPedidos.Name = "dgvPedidos";
            dgvPedidos.CellClick += dgvPedidos_CellClick;

            // panelDetalhes
            panelDetalhes.Dock = DockStyle.Fill;
            panelDetalhes.Padding = new Padding(20, 0, 20, 0);
            panelDetalhes.Controls.Add(lblDetalhes);
            panelDetalhes.Controls.Add(lblCliente);
            panelDetalhes.Controls.Add(txtCliente);
            panelDetalhes.Controls.Add(lblPedidoCliente);
            panelDetalhes.Controls.Add(txtPedidoCliente);
            panelDetalhes.Controls.Add(lblMarketplace);
            panelDetalhes.Controls.Add(txtMarketplace);
            panelDetalhes.Controls.Add(lblCodigoEtiqueta);
            panelDetalhes.Controls.Add(txtCodigoEtiqueta);
            panelDetalhes.Controls.Add(btnGerarEtiqueta);
            panelDetalhes.Controls.Add(btnImprimirPorProduto);
            panelDetalhes.Controls.Add(btnImprimirEtiqueta);

            lblDetalhes.AutoSize = true;
            lblDetalhes.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblDetalhes.Location = new Point(20, 5);
            lblDetalhes.Text = "Detalhes do pedido selecionado";

            lblCliente.AutoSize = true;
            lblCliente.Location = new Point(20, 40);
            lblCliente.Text = "Cliente";

            txtCliente.Location = new Point(20, 58);
            txtCliente.ReadOnly = true;
            txtCliente.Size = new Size(450, 23);

            lblPedidoCliente.AutoSize = true;
            lblPedidoCliente.Location = new Point(20, 86);
            lblPedidoCliente.Text = "Pedido Cliente";

            txtPedidoCliente.Location = new Point(20, 104);
            txtPedidoCliente.ReadOnly = true;
            txtPedidoCliente.Size = new Size(450, 23);

            lblMarketplace.AutoSize = true;
            lblMarketplace.Location = new Point(500, 40);
            lblMarketplace.Text = "Marketplace";

            txtMarketplace.Location = new Point(500, 58);
            txtMarketplace.ReadOnly = true;
            txtMarketplace.Size = new Size(250, 23);

            lblCodigoEtiqueta.AutoSize = true;
            lblCodigoEtiqueta.Location = new Point(500, 86);
            lblCodigoEtiqueta.Text = "Código Etiqueta";

            txtCodigoEtiqueta.Location = new Point(500, 104);
            txtCodigoEtiqueta.ReadOnly = true;
            txtCodigoEtiqueta.Size = new Size(250, 23);

            btnGerarEtiqueta.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGerarEtiqueta.Location = new Point(780, 45);
            btnGerarEtiqueta.Size = new Size(86, 77);
            btnGerarEtiqueta.Text = "Conferir";
            btnGerarEtiqueta.UseVisualStyleBackColor = true;
            btnGerarEtiqueta.Click += btnGerarEtiqueta_Click;

            btnImprimirPorProduto.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnImprimirPorProduto.Location = new Point(875, 45);
            btnImprimirPorProduto.Size = new Size(86, 77);
            btnImprimirPorProduto.Text = "Imprimir por Produto";
            btnImprimirPorProduto.UseVisualStyleBackColor = true;
            btnImprimirPorProduto.Click += btnImprimirPorProduto_Click;

            btnImprimirEtiqueta.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnImprimirEtiqueta.Location = new Point(970, 45);
            btnImprimirEtiqueta.Size = new Size(86, 77);
            btnImprimirEtiqueta.Text = "Imprimir Etiqueta";
            btnImprimirEtiqueta.UseVisualStyleBackColor = true;
            btnImprimirEtiqueta.Click += btnImprimirEtiqueta_Click;

            // panelItens
            panelItens.Dock = DockStyle.Fill;
            panelItens.Padding = new Padding(20, 0, 20, 5);
            panelItens.Controls.Add(dgvItensPedido);
            panelItens.Controls.Add(lblItens);

            lblItens.Dock = DockStyle.Top;
            lblItens.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblItens.Height = 28;
            lblItens.Text = "Itens do pedido";

            dgvItensPedido.Dock = DockStyle.Fill;
            dgvItensPedido.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvItensPedido.Name = "dgvItensPedido";

            // lblAtalhos
            lblAtalhos.Dock = DockStyle.Fill;
            lblAtalhos.BorderStyle = BorderStyle.FixedSingle;
            lblAtalhos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAtalhos.TextAlign = ContentAlignment.MiddleLeft;
            lblAtalhos.Text = "  ENTER = Imprimir Etiqueta    |    DEL = Remover Pedido (ADM)    |    F4 = Buscar Produto    |    F5 = Atualizar Pedidos    |    F8 = Conferência";

            layoutPrincipal.Controls.Add(panelTopo, 0, 0);
            layoutPrincipal.Controls.Add(panelFiltros, 0, 1);
            layoutPrincipal.Controls.Add(panelPedidos, 0, 2);
            layoutPrincipal.Controls.Add(panelDetalhes, 0, 3);
            layoutPrincipal.Controls.Add(panelItens, 0, 4);
            layoutPrincipal.Controls.Add(lblAtalhos, 0, 5);

            // FrmPreparacaoPedidos
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 760);
            MinimumSize = new Size(900, 600);
            Controls.Add(layoutPrincipal);
            MaximizeBox = true;
            MinimizeBox = true;
            Name = "FrmPreparacaoPedidos";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Preparação de Pedidos";

            ((System.ComponentModel.ISupportInitialize)dgvPedidos).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvItensPedido).EndInit();
            ResumeLayout(false);
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
        private System.Windows.Forms.Button btnExcluirPedido;
        private System.Windows.Forms.Button btnImprimirEtiqueta;
        private Button btnAtualizarPedidos;
        private Button btnImprimirPorProduto;
        private Button btnValidarVinculos;
        private Button btnAdministracao;
        private Label lblAtalhos;
        public System.Windows.Forms.Panel panelResumo;
        public System.Windows.Forms.Label lblResumoTotal;
        public System.Windows.Forms.Label lblResumoPreparados;
        public System.Windows.Forms.Label lblResumoFaltam;
        public System.Windows.Forms.Label lblResumoPercentual;
        public System.Windows.Forms.ProgressBar pbProgressoResumo;
        public System.Windows.Forms.CheckBox chkSomenteFaltantes;
        public System.Windows.Forms.Panel panelDataOperacional;
        public System.Windows.Forms.Label lblDataOperacional;
        public System.Windows.Forms.Label lblUltimoImpresso;
    }
}