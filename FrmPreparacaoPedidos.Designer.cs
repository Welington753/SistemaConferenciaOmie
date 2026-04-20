namespace SistemaConferenciaPedidos
{
    partial class FrmPreparacaoPedidos
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
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
        /// Método necessário para suporte ao Designer.
        /// Não modifique o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitulo = new Label();
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
            ((System.ComponentModel.ISupportInitialize)dgvPedidos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvItensPedido).BeginInit();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitulo.Location = new Point(25, 20);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(275, 32);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Preparação de Pedidos";
            // 
            // lblDataInicial
            // 
            lblDataInicial.AutoSize = true;
            lblDataInicial.Font = new Font("Segoe UI", 10F);
            lblDataInicial.Location = new Point(27, 75);
            lblDataInicial.Name = "lblDataInicial";
            lblDataInicial.Size = new Size(76, 19);
            lblDataInicial.TabIndex = 1;
            lblDataInicial.Text = "Data Inicial";
            // 
            // lblDataFinal
            // 
            lblDataFinal.AutoSize = true;
            lblDataFinal.Font = new Font("Segoe UI", 10F);
            lblDataFinal.Location = new Point(250, 75);
            lblDataFinal.Name = "lblDataFinal";
            lblDataFinal.Size = new Size(70, 19);
            lblDataFinal.TabIndex = 2;
            lblDataFinal.Text = "Data Final";
            // 
            // dtpDataInicial
            // 
            dtpDataInicial.Format = DateTimePickerFormat.Short;
            dtpDataInicial.Location = new Point(27, 100);
            dtpDataInicial.Name = "dtpDataInicial";
            dtpDataInicial.Size = new Size(180, 23);
            dtpDataInicial.TabIndex = 3;
            // 
            // dtpDataFinal
            // 
            dtpDataFinal.Format = DateTimePickerFormat.Short;
            dtpDataFinal.Location = new Point(250, 100);
            dtpDataFinal.Name = "dtpDataFinal";
            dtpDataFinal.Size = new Size(180, 23);
            dtpDataFinal.TabIndex = 4;
            // 
            // btnBuscarPedidos
            // 
            btnBuscarPedidos.Location = new Point(460, 95);
            btnBuscarPedidos.Name = "btnBuscarPedidos";
            btnBuscarPedidos.Size = new Size(150, 32);
            btnBuscarPedidos.TabIndex = 5;
            btnBuscarPedidos.Text = "Buscar Pedidos";
            btnBuscarPedidos.UseVisualStyleBackColor = true;
            btnBuscarPedidos.Click += btnBuscarPedidos_Click;
            // 
            // lblPedidos
            // 
            lblPedidos.AutoSize = true;
            lblPedidos.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblPedidos.Location = new Point(27, 145);
            lblPedidos.Name = "lblPedidos";
            lblPedidos.Size = new Size(155, 20);
            lblPedidos.TabIndex = 6;
            lblPedidos.Text = "Pedidos encontrados";
            // 
            // dgvPedidos
            // 
            dgvPedidos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPedidos.Location = new Point(27, 175);
            dgvPedidos.Name = "dgvPedidos";
            dgvPedidos.Size = new Size(1040, 180);
            dgvPedidos.TabIndex = 7;
            dgvPedidos.CellClick += dgvPedidos_CellClick;
            // 
            // lblDetalhes
            // 
            lblDetalhes.AutoSize = true;
            lblDetalhes.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblDetalhes.Location = new Point(27, 375);
            lblDetalhes.Name = "lblDetalhes";
            lblDetalhes.Size = new Size(230, 20);
            lblDetalhes.TabIndex = 8;
            lblDetalhes.Text = "Detalhes do pedido selecionado";
            // 
            // lblCliente
            // 
            lblCliente.AutoSize = true;
            lblCliente.Location = new Point(27, 410);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new Size(44, 15);
            lblCliente.TabIndex = 9;
            lblCliente.Text = "Cliente";
            // 
            // lblPedidoCliente
            // 
            lblPedidoCliente.AutoSize = true;
            lblPedidoCliente.Location = new Point(27, 455);
            lblPedidoCliente.Name = "lblPedidoCliente";
            lblPedidoCliente.Size = new Size(84, 15);
            lblPedidoCliente.TabIndex = 10;
            lblPedidoCliente.Text = "Pedido Cliente";
            // 
            // lblMarketplace
            // 
            lblMarketplace.AutoSize = true;
            lblMarketplace.Location = new Point(550, 410);
            lblMarketplace.Name = "lblMarketplace";
            lblMarketplace.Size = new Size(72, 15);
            lblMarketplace.TabIndex = 11;
            lblMarketplace.Text = "Marketplace";
            // 
            // lblCodigoEtiqueta
            // 
            lblCodigoEtiqueta.AutoSize = true;
            lblCodigoEtiqueta.Location = new Point(550, 455);
            lblCodigoEtiqueta.Name = "lblCodigoEtiqueta";
            lblCodigoEtiqueta.Size = new Size(92, 15);
            lblCodigoEtiqueta.TabIndex = 12;
            lblCodigoEtiqueta.Text = "Código Etiqueta";
            // 
            // txtCliente
            // 
            txtCliente.Location = new Point(27, 428);
            txtCliente.Name = "txtCliente";
            txtCliente.ReadOnly = true;
            txtCliente.Size = new Size(450, 23);
            txtCliente.TabIndex = 13;
            // 
            // txtPedidoCliente
            // 
            txtPedidoCliente.Location = new Point(27, 473);
            txtPedidoCliente.Name = "txtPedidoCliente";
            txtPedidoCliente.ReadOnly = true;
            txtPedidoCliente.Size = new Size(450, 23);
            txtPedidoCliente.TabIndex = 14;
            // 
            // txtMarketplace
            // 
            txtMarketplace.Location = new Point(550, 428);
            txtMarketplace.Name = "txtMarketplace";
            txtMarketplace.ReadOnly = true;
            txtMarketplace.Size = new Size(250, 23);
            txtMarketplace.TabIndex = 15;
            // 
            // txtCodigoEtiqueta
            // 
            txtCodigoEtiqueta.Location = new Point(550, 473);
            txtCodigoEtiqueta.Name = "txtCodigoEtiqueta";
            txtCodigoEtiqueta.ReadOnly = true;
            txtCodigoEtiqueta.Size = new Size(250, 23);
            txtCodigoEtiqueta.TabIndex = 16;
            // 
            // lblItens
            // 
            lblItens.AutoSize = true;
            lblItens.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblItens.Location = new Point(27, 520);
            lblItens.Name = "lblItens";
            lblItens.Size = new Size(118, 20);
            lblItens.TabIndex = 17;
            lblItens.Text = "Itens do pedido";
            // 
            // dgvItensPedido
            // 
            dgvItensPedido.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvItensPedido.Location = new Point(27, 548);
            dgvItensPedido.Name = "dgvItensPedido";
            dgvItensPedido.Size = new Size(1040, 140);
            dgvItensPedido.TabIndex = 18;
            // 
            // btnGerarEtiqueta
            // 
            btnGerarEtiqueta.Location = new Point(820, 423);
            btnGerarEtiqueta.Name = "btnGerarEtiqueta";
            btnGerarEtiqueta.Size = new Size(120, 77);
            btnGerarEtiqueta.TabIndex = 19;
            btnGerarEtiqueta.Text = "Conferir";
            btnGerarEtiqueta.UseVisualStyleBackColor = true;
            btnGerarEtiqueta.Click += btnGerarEtiqueta_Click;
            // 
            // btnSalvarPedido
            // 
            btnSalvarPedido.Location = new Point(630, 95);
            btnSalvarPedido.Name = "btnSalvarPedido";
            btnSalvarPedido.Size = new Size(180, 32);
            btnSalvarPedido.TabIndex = 20;
            btnSalvarPedido.Text = "Importar Etiquetas do Lote";
            btnSalvarPedido.UseVisualStyleBackColor = true;
            btnSalvarPedido.Click += btnSalvarPedido_Click;
            // 
            // btnImprimirEtiqueta
            // 
            btnImprimirEtiqueta.Location = new Point(947, 423);
            btnImprimirEtiqueta.Name = "btnImprimirEtiqueta";
            btnImprimirEtiqueta.Size = new Size(120, 77);
            btnImprimirEtiqueta.TabIndex = 21;
            btnImprimirEtiqueta.Text = "Imprimir Etiqueta";
            btnImprimirEtiqueta.UseVisualStyleBackColor = true;
            btnImprimirEtiqueta.Click += btnImprimirEtiqueta_Click;
            // 
            // FrmPreparacaoPedidos
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 720);
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
            Controls.Add(lblDataInicial);
            Controls.Add(lblTitulo);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmPreparacaoPedidos";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Preparação de Pedidos";
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
    }
}