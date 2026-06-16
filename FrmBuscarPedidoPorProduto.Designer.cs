namespace SistemaConferenciaPedidos
{
    partial class FrmBuscarPedidoPorProduto
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblCodigo = new Label();
            txtCodigoProduto = new TextBox();
            btnBuscar = new Button();
            dgvResultados = new DataGridView();
            btnConfirmarImprimir = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvResultados).BeginInit();
            SuspendLayout();
            // 
            // lblCodigo
            // 
            lblCodigo.AutoSize = true;
            lblCodigo.Font = new Font("Segoe UI", 10F);
            lblCodigo.Location = new Point(20, 20);
            lblCodigo.Name = "lblCodigo";
            lblCodigo.Size = new Size(176, 19);
            lblCodigo.TabIndex = 0;
            lblCodigo.Text = "Bipe ou digite o EAN / SKU:";
            // 
            // txtCodigoProduto
            // 
            txtCodigoProduto.Font = new Font("Segoe UI", 14F);
            txtCodigoProduto.Location = new Point(24, 48);
            txtCodigoProduto.Name = "txtCodigoProduto";
            txtCodigoProduto.Size = new Size(520, 32);
            txtCodigoProduto.TabIndex = 1;
            // 
            // btnBuscar
            // 
            btnBuscar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnBuscar.Location = new Point(560, 47);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(120, 34);
            btnBuscar.TabIndex = 2;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            // 
            // dgvResultados
            // 
            dgvResultados.AllowUserToAddRows = false;
            dgvResultados.AllowUserToDeleteRows = false;
            dgvResultados.AllowUserToResizeRows = false;
            dgvResultados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResultados.BackgroundColor = Color.White;
            dgvResultados.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResultados.Location = new Point(24, 100);
            dgvResultados.MultiSelect = false;
            dgvResultados.Name = "dgvResultados";
            dgvResultados.ReadOnly = true;
            dgvResultados.RowTemplate.Height = 28;
            dgvResultados.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResultados.Size = new Size(820, 360);
            dgvResultados.TabIndex = 3;
            // 
            // btnConfirmarImprimir
            // 
            btnConfirmarImprimir.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConfirmarImprimir.Location = new Point(644, 480);
            btnConfirmarImprimir.Name = "btnConfirmarImprimir";
            btnConfirmarImprimir.Size = new Size(200, 42);
            btnConfirmarImprimir.TabIndex = 4;
            btnConfirmarImprimir.Text = "Confirmar e imprimir";
            btnConfirmarImprimir.UseVisualStyleBackColor = true;
            // 
            // FrmBuscarPedidoPorProduto
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(870, 545);
            Controls.Add(btnConfirmarImprimir);
            Controls.Add(dgvResultados);
            Controls.Add(btnBuscar);
            Controls.Add(txtCodigoProduto);
            Controls.Add(lblCodigo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FrmBuscarPedidoPorProduto";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Buscar pedido por produto";
            ((System.ComponentModel.ISupportInitialize)dgvResultados).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblCodigo;
        private TextBox txtCodigoProduto;
        private Button btnBuscar;
        private DataGridView dgvResultados;
        private Button btnConfirmarImprimir;
    }
}