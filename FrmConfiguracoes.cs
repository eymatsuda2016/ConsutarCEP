using System;
using System.Drawing;
using System.Windows.Forms;

namespace ConsultaCEPs
{
    public partial class FrmConfiguracoes : Form
    {
        public float EscalaSelecionada { get; private set; }
        public int ValidadeCacheSelecionada { get; private set; }
        public bool ModoEscuroSelecionado { get; private set; }

        public FrmConfiguracoes(float escalaAtual, int validadeCacheAtual, bool modoEscuroAtual)
        {
            InitializeComponent();
            EscalaSelecionada = escalaAtual;
            ValidadeCacheSelecionada = validadeCacheAtual;
            ModoEscuroSelecionado = modoEscuroAtual;
            trkEscalaFonte.Value = Math.Max(trkEscalaFonte.Minimum, Math.Min(trkEscalaFonte.Maximum, (int)Math.Round(escalaAtual * 100)));
            trkValidadeCache.Value = Math.Max(trkValidadeCache.Minimum, Math.Min(trkValidadeCache.Maximum, validadeCacheAtual));
            chkModoEscuro.Checked = modoEscuroAtual;
            AtualizarTextoEscala();
            AtualizarTextoValidadeCache();
        }

        private void trkEscalaFonte_Scroll(object sender, EventArgs e)
        {
            AtualizarTextoEscala();
        }

        private void AtualizarTextoEscala()
        {
            lblValorEscala.Text = string.Format("{0}%", trkEscalaFonte.Value);
        }

        private void trkValidadeCache_Scroll(object sender, EventArgs e)
        {
            AtualizarTextoValidadeCache();
        }

        private void AtualizarTextoValidadeCache()
        {
            lblValorValidadeCache.Text = string.Format("{0} min", trkValidadeCache.Value);
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            EscalaSelecionada = trkEscalaFonte.Value / 100f;
            ValidadeCacheSelecionada = trkValidadeCache.Value;
            ModoEscuroSelecionado = chkModoEscuro.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
