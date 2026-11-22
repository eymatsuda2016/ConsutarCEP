using System;
using System.Drawing;
using System.Windows.Forms;

namespace ConsultaCEPs
{
    public partial class FrmConfiguracoes : Form
    {
        public float EscalaSelecionada { get; private set; }

        public FrmConfiguracoes(float escalaAtual)
        {
            InitializeComponent();
            EscalaSelecionada = escalaAtual;
            trkEscalaFonte.Value = Math.Max(trkEscalaFonte.Minimum, Math.Min(trkEscalaFonte.Maximum, (int)Math.Round(escalaAtual * 100)));
            AtualizarTextoEscala();
        }

        private void trkEscalaFonte_Scroll(object sender, EventArgs e)
        {
            AtualizarTextoEscala();
        }

        private void AtualizarTextoEscala()
        {
            lblValorEscala.Text = string.Format("{0}%", trkEscalaFonte.Value);
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            EscalaSelecionada = trkEscalaFonte.Value / 100f;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
