namespace ConsultaCEPs
{
    partial class FrmConsultaCEP
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtCEP = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtEstado = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCidade = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtBairro = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtLogradouro = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbCEPs = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtObservacao = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbHistorico = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.lblDesenvolvedor = new System.Windows.Forms.Label();
            this.lblFonteDados = new System.Windows.Forms.Label();
            this.btnCopiarCep = new System.Windows.Forms.Button();
            this.btnLimpar = new System.Windows.Forms.Button();
            this.btnSair = new System.Windows.Forms.Button();
            this.btnConsultar = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.label1.Location = new System.Drawing.Point(25, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Busca rápida por CEP (opcional):";
            // 
            // txtCEP
            // 
            this.txtCEP.BackColor = System.Drawing.Color.White;
            this.txtCEP.Location = new System.Drawing.Point(25, 37);
            this.txtCEP.Name = "txtCEP";
            this.txtCEP.Size = new System.Drawing.Size(201, 20);
            this.txtCEP.TabIndex = 1;
            this.txtCEP.MaxLength = 9;
            this.txtCEP.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCEP_KeyPress);
            this.txtCEP.Leave += new System.EventHandler(this.txtCEP_Leave);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbCEPs);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtObservacao);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.cmbHistorico);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.txtLogradouro);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtBairro);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtCidade);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtEstado);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(25, 81);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(529, 380);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Busca por endereço e resultado";
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            // 
            // txtEstado
            // 
            this.txtEstado.BackColor = System.Drawing.Color.White;
            this.txtEstado.Location = new System.Drawing.Point(6, 39);
            this.txtEstado.Name = "txtEstado";
            this.txtEstado.Size = new System.Drawing.Size(60, 20);
            this.txtEstado.TabIndex = 3;
            this.txtEstado.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtEstado.MaxLength = 2;
            this.txtEstado.TextChanged += new System.EventHandler(this.txtEstado_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.label2.Location = new System.Drawing.Point(6, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "UF (obrigatório, ex.: SP):";
            // 
            // txtCidade
            // 
            this.txtCidade.BackColor = System.Drawing.Color.White;
            this.txtCidade.Location = new System.Drawing.Point(6, 86);
            this.txtCidade.Name = "txtCidade";
            this.txtCidade.Size = new System.Drawing.Size(492, 20);
            this.txtCidade.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.label3.Location = new System.Drawing.Point(6, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Localidade (obrigatório):";
            // 
            // txtBairro
            // 
            this.txtBairro.BackColor = System.Drawing.Color.FromArgb(232, 241, 248);
            this.txtBairro.Location = new System.Drawing.Point(6, 134);
            this.txtBairro.Name = "txtBairro";
            this.txtBairro.Size = new System.Drawing.Size(492, 20);
            this.txtBairro.TabIndex = 7;
            this.txtBairro.ReadOnly = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.label4.Location = new System.Drawing.Point(6, 118);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Bairro/Distrito (resultado):";
            // 
            // txtLogradouro
            // 
            this.txtLogradouro.BackColor = System.Drawing.Color.White;
            this.txtLogradouro.Location = new System.Drawing.Point(9, 193);
            this.txtLogradouro.Name = "txtLogradouro";
            this.txtLogradouro.Size = new System.Drawing.Size(489, 20);
            this.txtLogradouro.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.label5.Location = new System.Drawing.Point(9, 177);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Logradouro/Nome (rua, avenida etc.):";
            // 
            // cmbCEPs
            // 
            this.cmbCEPs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCEPs.FormattingEnabled = true;
            this.cmbCEPs.BackColor = System.Drawing.Color.White;
            this.cmbCEPs.Location = new System.Drawing.Point(6, 260);
            this.cmbCEPs.Name = "cmbCEPs";
            this.cmbCEPs.Size = new System.Drawing.Size(492, 21);
            this.cmbCEPs.TabIndex = 11;
            this.cmbCEPs.SelectedIndexChanged += new System.EventHandler(this.cmbCEPs_SelectedIndexChanged);
            this.cmbCEPs.SelectionChangeCommitted += new System.EventHandler(this.cmbCEPs_SelectionChangeCommitted);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.label6.Location = new System.Drawing.Point(6, 244);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(178, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "CEPs encontrados (se houver mais de um):";
            // 
            // txtObservacao
            // 
            this.txtObservacao.BackColor = System.Drawing.Color.FromArgb(232, 241, 248);
            this.txtObservacao.Location = new System.Drawing.Point(6, 306);
            this.txtObservacao.Name = "txtObservacao";
            this.txtObservacao.ReadOnly = true;
            this.txtObservacao.Size = new System.Drawing.Size(492, 20);
            this.txtObservacao.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.label7.Location = new System.Drawing.Point(6, 290);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(105, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Observação do CEP:";
            // 
            // cmbHistorico
            // 
            this.cmbHistorico.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHistorico.FormattingEnabled = true;
            this.cmbHistorico.BackColor = System.Drawing.Color.White;
            this.cmbHistorico.Location = new System.Drawing.Point(6, 356);
            this.cmbHistorico.Name = "cmbHistorico";
            this.cmbHistorico.Size = new System.Drawing.Size(492, 21);
            this.cmbHistorico.TabIndex = 15;
            this.cmbHistorico.SelectedIndexChanged += new System.EventHandler(this.cmbHistorico_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.label8.Location = new System.Drawing.Point(6, 340);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(164, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Histórico de CEPs (nesta sessão):";
            // 
            // btnLimpar
            // 
            // btnCopiarCep
            // 
            this.btnCopiarCep.BackColor = System.Drawing.Color.FromArgb(63, 145, 205);
            this.btnCopiarCep.FlatAppearance.BorderSize = 0;
            this.btnCopiarCep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopiarCep.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopiarCep.ForeColor = System.Drawing.Color.White;
            this.btnCopiarCep.Location = new System.Drawing.Point(25, 492);
            this.btnCopiarCep.Name = "btnCopiarCep";
            this.btnCopiarCep.Size = new System.Drawing.Size(137, 30);
            this.btnCopiarCep.TabIndex = 6;
            this.btnCopiarCep.Text = "Copiar CEP";
            this.btnCopiarCep.UseVisualStyleBackColor = false;
            this.btnCopiarCep.Click += new System.EventHandler(this.btnCopiarCep_Click);
            // 
            // btnLimpar
            // 
            this.btnLimpar.BackgroundImage = global::ConsultaCEPs.Properties.Resources.limpar;
            this.btnLimpar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnLimpar.BackColor = System.Drawing.Color.FromArgb(92, 155, 213);
            this.btnLimpar.FlatAppearance.BorderSize = 0;
            this.btnLimpar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLimpar.ForeColor = System.Drawing.Color.White;
            this.btnLimpar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLimpar.Location = new System.Drawing.Point(249, 492);
            this.btnLimpar.Name = "btnLimpar";
            this.btnLimpar.Size = new System.Drawing.Size(137, 30);
            this.btnLimpar.TabIndex = 4;
            this.btnLimpar.Text = "Limpar";
            this.btnLimpar.UseVisualStyleBackColor = false;
            this.btnLimpar.Click += new System.EventHandler(this.btnLimpar_Click);
            // 
            // btnSair
            // 
            this.btnSair.BackgroundImage = global::ConsultaCEPs.Properties.Resources.sair;
            this.btnSair.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnSair.BackColor = System.Drawing.Color.FromArgb(27, 64, 96);
            this.btnSair.FlatAppearance.BorderSize = 0;
            this.btnSair.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSair.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSair.ForeColor = System.Drawing.Color.White;
            this.btnSair.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSair.Location = new System.Drawing.Point(406, 492);
            this.btnSair.Name = "btnSair";
            this.btnSair.Size = new System.Drawing.Size(137, 30);
            this.btnSair.TabIndex = 5;
            this.btnSair.Text = "Sair";
            this.btnSair.UseVisualStyleBackColor = false;
            this.btnSair.Click += new System.EventHandler(this.btnSair_Click);
            // 
            // btnConsultar
            // 
            this.btnConsultar.BackColor = System.Drawing.Color.FromArgb(38, 115, 170);
            this.btnConsultar.FlatAppearance.BorderSize = 0;
            this.btnConsultar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConsultar.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConsultar.ForeColor = System.Drawing.Color.White;
            this.btnConsultar.Location = new System.Drawing.Point(249, 32);
            this.btnConsultar.Name = "btnConsultar";
            this.btnConsultar.Size = new System.Drawing.Size(274, 30);
            this.btnConsultar.TabIndex = 3;
            this.btnConsultar.Text = "Consultar CEP";
            this.btnConsultar.UseVisualStyleBackColor = false;
            this.btnConsultar.Click += new System.EventHandler(this.btnConsultar_Click);
            // 
            // lblDesenvolvedor
            // 
            this.lblDesenvolvedor.AutoSize = true;
            this.lblDesenvolvedor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDesenvolvedor.ForeColor = System.Drawing.Color.Gray;
            this.lblDesenvolvedor.Location = new System.Drawing.Point(25, 528);
            this.lblDesenvolvedor.Name = "lblDesenvolvedor";
            this.lblDesenvolvedor.Size = new System.Drawing.Size(193, 13);
            this.lblDesenvolvedor.TabIndex = 6;
            this.lblDesenvolvedor.Text = "Desenvolvido Por Eduardo Y. Matsuda";
            // 
            // lblFonteDados
            // 
            this.lblFonteDados.AutoSize = true;
            this.lblFonteDados.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFonteDados.ForeColor = System.Drawing.Color.Gray;
            this.lblFonteDados.Location = new System.Drawing.Point(421, 528);
            this.lblFonteDados.Name = "lblFonteDados";
            this.lblFonteDados.Size = new System.Drawing.Size(76, 13);
            this.lblFonteDados.TabIndex = 7;
            this.lblFonteDados.Text = "Fonte: ViaCEP";
            // 
            // FrmConsultaCEP
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(220, 236, 247);
            this.ClientSize = new System.Drawing.Size(579, 544);
            this.Controls.Add(this.btnSair);
            this.Controls.Add(this.btnLimpar);
            this.Controls.Add(this.btnCopiarCep);
            this.Controls.Add(this.btnConsultar);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtCEP);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblDesenvolvedor);
            this.Controls.Add(this.lblFonteDados);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "FrmConsultaCEP";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Consultar CEP por endereço";
            this.Load += new System.EventHandler(this.FrmConsultaCEP_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCEP;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtLogradouro;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbCEPs;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtObservacao;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbHistorico;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtBairro;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtCidade;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtEstado;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnConsultar;
        private System.Windows.Forms.Button btnCopiarCep;
        private System.Windows.Forms.Button btnLimpar;
        private System.Windows.Forms.Button btnSair;
        private System.Windows.Forms.Label lblDesenvolvedor;
        private System.Windows.Forms.Label lblFonteDados;
    }
}

