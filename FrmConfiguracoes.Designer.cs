namespace ConsultaCEPs
{
    partial class FrmConfiguracoes
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblEscala = new System.Windows.Forms.Label();
            this.trkEscalaFonte = new System.Windows.Forms.TrackBar();
            this.lblValorEscala = new System.Windows.Forms.Label();
            this.lblValidadeCache = new System.Windows.Forms.Label();
            this.trkValidadeCache = new System.Windows.Forms.TrackBar();
            this.lblValorValidadeCache = new System.Windows.Forms.Label();
            this.chkModoEscuro = new System.Windows.Forms.CheckBox();
            this.btnSalvar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trkEscalaFonte)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkValidadeCache)).BeginInit();
            this.SuspendLayout();
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.lblTitulo.Location = new System.Drawing.Point(22, 18);
            this.lblTitulo.Text = "Configurações";
            this.lblEscala.AutoSize = true;
            this.lblEscala.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.lblEscala.Location = new System.Drawing.Point(23, 59);
            this.lblEscala.Text = "Tamanho da fonte:";
            this.trkEscalaFonte.Location = new System.Drawing.Point(20, 80);
            this.trkEscalaFonte.Maximum = 125;
            this.trkEscalaFonte.Minimum = 85;
            this.trkEscalaFonte.TickFrequency = 5;
            this.trkEscalaFonte.Size = new System.Drawing.Size(270, 45);
            this.trkEscalaFonte.Scroll += new System.EventHandler(this.trkEscalaFonte_Scroll);
            this.lblValorEscala.AutoSize = true;
            this.lblValorEscala.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblValorEscala.ForeColor = System.Drawing.Color.FromArgb(38, 115, 170);
            this.lblValorEscala.Location = new System.Drawing.Point(302, 88);
            this.lblValorEscala.Text = "100%";
            this.lblValidadeCache.AutoSize = true;
            this.lblValidadeCache.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.lblValidadeCache.Location = new System.Drawing.Point(23, 125);
            this.lblValidadeCache.Text = "Validade do cache de consultas:";
            this.trkValidadeCache.Location = new System.Drawing.Point(20, 146);
            this.trkValidadeCache.Maximum = 60;
            this.trkValidadeCache.Minimum = 1;
            this.trkValidadeCache.TickFrequency = 5;
            this.trkValidadeCache.Size = new System.Drawing.Size(270, 45);
            this.trkValidadeCache.Scroll += new System.EventHandler(this.trkValidadeCache_Scroll);
            this.lblValorValidadeCache.AutoSize = true;
            this.lblValorValidadeCache.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.lblValorValidadeCache.ForeColor = System.Drawing.Color.FromArgb(38, 115, 170);
            this.lblValorValidadeCache.Location = new System.Drawing.Point(302, 154);
            this.lblValorValidadeCache.Text = "10 min";
            this.chkModoEscuro.AutoSize = true;
            this.chkModoEscuro.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.chkModoEscuro.Location = new System.Drawing.Point(23, 190);
            this.chkModoEscuro.Text = "Usar modo escuro";
            this.btnSalvar.BackColor = System.Drawing.Color.FromArgb(72, 137, 104);
            this.btnSalvar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalvar.ForeColor = System.Drawing.Color.White;
            this.btnSalvar.Location = new System.Drawing.Point(145, 215);
            this.btnSalvar.Size = new System.Drawing.Size(95, 30);
            this.btnSalvar.Text = "Aplicar";
            this.btnSalvar.UseVisualStyleBackColor = false;
            this.btnSalvar.Click += new System.EventHandler(this.btnSalvar_Click);
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.Location = new System.Drawing.Point(250, 215);
            this.btnCancelar.Size = new System.Drawing.Size(95, 30);
            this.btnCancelar.Text = "Cancelar";
            this.Controls.Add(this.lblTitulo);
            this.Controls.Add(this.lblEscala);
            this.Controls.Add(this.trkEscalaFonte);
            this.Controls.Add(this.lblValorEscala);
            this.Controls.Add(this.lblValidadeCache);
            this.Controls.Add(this.trkValidadeCache);
            this.Controls.Add(this.lblValorValidadeCache);
            this.Controls.Add(this.chkModoEscuro);
            this.Controls.Add(this.btnSalvar);
            this.Controls.Add(this.btnCancelar);
            this.AcceptButton = this.btnSalvar;
            this.CancelButton = this.btnCancelar;
            this.BackColor = System.Drawing.Color.FromArgb(226, 240, 248);
            this.ClientSize = new System.Drawing.Size(370, 265);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configurações - Consultar CEP";
            ((System.ComponentModel.ISupportInitialize)(this.trkEscalaFonte)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkValidadeCache)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblEscala;
        private System.Windows.Forms.TrackBar trkEscalaFonte;
        private System.Windows.Forms.Label lblValorEscala;
        private System.Windows.Forms.Label lblValidadeCache;
        private System.Windows.Forms.TrackBar trkValidadeCache;
        private System.Windows.Forms.Label lblValorValidadeCache;
        private System.Windows.Forms.CheckBox chkModoEscuro;
        private System.Windows.Forms.Button btnSalvar;
        private System.Windows.Forms.Button btnCancelar;
    }
}
