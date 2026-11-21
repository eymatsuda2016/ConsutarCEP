using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsultaCEPs
{
    public partial class FrmConsultaCEP : Form
    {
        private ViaCepEndereco[] enderecosEncontrados = new ViaCepEndereco[0];
        private Bitmap marcaDagua;
        private readonly List<string> historicoCEPs = new List<string>();
        private readonly Dictionary<string, string> cacheRespostas = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> ordemCacheRespostas = new List<string>();
        private bool atualizandoHistorico;
        private static readonly string arquivoHistorico = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ConsultarCEP",
            "historico-ceps.txt");
        private float escalaFonte = 1.0f;
        private const float EscalaFonteMinima = 0.85f;
        private const float EscalaFonteMaxima = 1.25f;
        private const float TamanhoFonteBase = 8.25f;

        private static readonly HttpClient clienteHttp = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public FrmConsultaCEP()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            InitializeComponent();
            DoubleBuffered = true;
            Paint += FrmConsultaCEP_Paint;
            Shown += FrmConsultaCEP_Shown;
            FormClosed += FrmConsultaCEP_FormClosed;
            LocationChanged += FrmConsultaCEP_LocationChanged;
            Icon = CriarIconeCabecalho();
        }

        private async void FrmConsultaCEP_Shown(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            AtualizarLimitesMaximizacao();

            try
            {
                Bitmap imagem = await Task.Run(() => new Bitmap(Properties.Resources.marca_dagua_trabalhador_postal));
                if (IsDisposed)
                {
                    imagem.Dispose();
                    return;
                }

                marcaDagua = imagem;
                Invalidate();
            }
            catch (Exception excecao)
            {
                // A imagem é decorativa; a aplicação deve continuar funcionando sem ela.
                LogTecnico.Registrar("Não foi possível carregar a marca d'água", excecao);
            }
        }

        private void FrmConsultaCEP_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (marcaDagua != null)
            {
                marcaDagua.Dispose();
                marcaDagua = null;
            }
        }

        private void FrmConsultaCEP_Paint(object sender, PaintEventArgs e)
        {
            if (marcaDagua == null)
            {
                return;
            }

            using (ImageAttributes atributos = new ImageAttributes())
            {
                ColorMatrix matriz = new ColorMatrix { Matrix33 = 0.16f };
                atributos.SetColorMatrix(matriz);

                Rectangle destino = ClientRectangle;
                float proporcaoDestino = (float)destino.Width / destino.Height;
                float proporcaoImagem = (float)marcaDagua.Width / marcaDagua.Height;
                Rectangle origem;

                if (proporcaoImagem > proporcaoDestino)
                {
                    int larguraRecorte = (int)(marcaDagua.Height * proporcaoDestino);
                    origem = new Rectangle((marcaDagua.Width - larguraRecorte) / 2, 0, larguraRecorte, marcaDagua.Height);
                }
                else
                {
                    int alturaRecorte = (int)(marcaDagua.Width / proporcaoDestino);
                    origem = new Rectangle(0, (marcaDagua.Height - alturaRecorte) / 2, marcaDagua.Width, alturaRecorte);
                }

                e.Graphics.DrawImage(marcaDagua, destino, origem.X, origem.Y, origem.Width, origem.Height, GraphicsUnit.Pixel, atributos);
            }
        }

        private static Icon CriarIconeCabecalho()
        {
            using (Bitmap imagem = new Bitmap(Properties.Resources.busca_cep))
            {
                IntPtr handle = imagem.GetHicon();
                using (Icon temporario = Icon.FromHandle(handle))
                {
                    return (Icon)temporario.Clone();
                }
            }
        }

        private void FrmConsultaCEP_Load(object sender, EventArgs e)
        {
            PrepararJanelaInicial();
            ArredondarBotao(btnConsultar, 12);
            ArredondarBotao(btnLimpar, 10);
            ArredondarBotao(btnSair, 10);
            ConfigurarEfeitoHover(btnConsultar, Color.FromArgb(38, 115, 170), Color.FromArgb(63, 145, 205));
            ConfigurarEfeitoHover(btnLimpar, Color.FromArgb(92, 155, 213), Color.FromArgb(124, 181, 230));
            ConfigurarEfeitoHover(btnSair, Color.FromArgb(27, 64, 96), Color.FromArgb(48, 94, 130));
            ConfigurarEfeitoHover(btnCopiarCep, Color.FromArgb(63, 145, 205), Color.FromArgb(92, 169, 220));
            ConfigurarEfeitoHover(btnExportar, Color.FromArgb(72, 137, 104), Color.FromArgb(95, 164, 126));
            ConfigurarEfeitoHover(btnDiminuirFonte, Color.FromArgb(92, 155, 213), Color.FromArgb(124, 181, 230));
            ConfigurarEfeitoHover(btnAumentarFonte, Color.FromArgb(38, 115, 170), Color.FromArgb(63, 145, 205));
            btnCopiarCep.Enabled = false;
            btnExportar.Enabled = false;
            CarregarHistorico();
            AtualizarLimitesMaximizacao();
            btnConsultar.Image = CriarImagemBotaoConsultar();
            btnConsultar.ImageAlign = ContentAlignment.MiddleLeft;
            btnConsultar.TextImageRelation = TextImageRelation.ImageBeforeText;
            txtCEP.Focus();
        }

        private void FrmConsultaCEP_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                AtualizarLimitesMaximizacao();
            }
        }

        private void FrmConsultaCEP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (btnConsultar.Enabled)
                {
                    btnConsultar.PerformClick();
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                btnLimpar.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void AtualizarLimitesMaximizacao()
        {
            Screen telaAtual = Screen.FromControl(this);
            MaximizedBounds = telaAtual.WorkingArea;
        }

        private void PrepararJanelaInicial()
        {
            WindowState = FormWindowState.Normal;
            ClientSize = new Size(579, 544);

            Screen telaAtual = Screen.FromPoint(Cursor.Position);
            Rectangle areaUtil = telaAtual.WorkingArea;
            int x = areaUtil.Left + (areaUtil.Width - Width) / 2;
            int y = areaUtil.Top + (areaUtil.Height - Height) / 2;

            Location = new Point(x, y);
            MaximizedBounds = areaUtil;
        }

        private static Bitmap CriarImagemBotaoConsultar()
        {
            return new Bitmap(Properties.Resources.busca_cep, new Size(42, 12));
        }

        private static void ConfigurarEfeitoHover(Button botao, Color corNormal, Color corHover)
        {
            botao.Cursor = Cursors.Hand;
            botao.BackColor = corNormal;
            botao.MouseEnter += delegate { botao.BackColor = corHover; };
            botao.MouseLeave += delegate { botao.BackColor = corNormal; };
        }

        private void btnDiminuirFonte_Click(object sender, EventArgs e)
        {
            escalaFonte = Math.Max(EscalaFonteMinima, escalaFonte - 0.05f);
            AplicarEscalaFonte();
        }

        private void btnAumentarFonte_Click(object sender, EventArgs e)
        {
            escalaFonte = Math.Min(EscalaFonteMaxima, escalaFonte + 0.05f);
            AplicarEscalaFonte();
        }

        private void AplicarEscalaFonte()
        {
            AplicarEscalaFonte(this, escalaFonte);
        }

        private static void AplicarEscalaFonte(Control controle, float escala)
        {
            if (controle == null)
            {
                return;
            }

            controle.Font = new Font(controle.Font.FontFamily, TamanhoFonteBase * escala, controle.Font.Style);
            foreach (Control filho in controle.Controls)
            {
                AplicarEscalaFonte(filho, escala);
            }
        }

        private void txtEstado_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtCEP_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtCEP_Leave(object sender, EventArgs e)
        {
            txtCEP.Text = CepUtil.Formatar(txtCEP.Text);
        }

        private async void btnConsultar_Click(object sender, EventArgs e)
        {
            string cep = SomenteNumeros(txtCEP.Text);
            string estado = txtEstado.Text.Trim();
            string cidade = txtCidade.Text.Trim();
            string logradouro = txtLogradouro.Text.Trim();
            bool informouEndereco = !string.IsNullOrWhiteSpace(estado) ||
                                    !string.IsNullOrWhiteSpace(cidade) ||
                                    !string.IsNullOrWhiteSpace(logradouro);

            if (!informouEndereco && string.IsNullOrWhiteSpace(cep))
            {
                MessageBox.Show("Informe um CEP ou preencha UF, Cidade e Logradouro para pesquisar.", "Dados incompletos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (informouEndereco &&
                (string.IsNullOrWhiteSpace(estado) || estado.Length != 2 || estado.Any(c => !char.IsLetter(c)) ||
                 string.IsNullOrWhiteSpace(cidade) || string.IsNullOrWhiteSpace(logradouro)))
            {
                MessageBox.Show("Para buscar por endereço, preencha a UF com 2 letras, a Cidade e o Logradouro.", "Dados incompletos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!informouEndereco && !string.IsNullOrWhiteSpace(cep) && cep.Length != 8)
            {
                MessageBox.Show("O CEP deve conter 8 números.", "CEP inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string textoBotaoConsultar = btnConsultar.Text;
            btnConsultar.Enabled = false;
            btnConsultar.Text = "Consultando...";
            btnConsultar.Cursor = Cursors.WaitCursor;
            btnConsultar.Refresh();

            try
            {
                bool buscaPorEndereco = informouEndereco;
                LogTecnico.Registrar(buscaPorEndereco ? "Iniciada consulta por endereço" : "Iniciada consulta por CEP");
                string url = buscaPorEndereco
                    ? string.Format("https://viacep.com.br/ws/{0}/{1}/{2}/json/", Uri.EscapeDataString(estado), Uri.EscapeDataString(cidade), Uri.EscapeDataString(logradouro))
                    : string.Format("https://viacep.com.br/ws/{0}/json/", cep);

                string json;
                if (cacheRespostas.TryGetValue(url, out json))
                {
                    LogTecnico.Registrar("Consulta atendida pelo cache recente");
                }
                else
                {
                    if (!NetworkInterface.GetIsNetworkAvailable())
                    {
                        MessageBox.Show("Não há conexão de rede disponível. Verifique sua internet e tente novamente.", "Sem conexão", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    using (HttpResponseMessage resposta = await clienteHttp.GetAsync(url))
                    {
                        resposta.EnsureSuccessStatusCode();
                        LogTecnico.Registrar("Serviço ViaCEP respondeu com sucesso");
                        json = await resposta.Content.ReadAsStringAsync();
                    }

                    ArmazenarNoCache(url, json);
                }

                if (buscaPorEndereco)
                {
                    ViaCepEndereco[] enderecos = Desserializar<ViaCepEndereco[]>(json);

                    if (enderecos == null || enderecos.Length == 0)
                    {
                        LimparResultado();
                        MessageBox.Show("Rua inexistente ou endereço não encontrado.", "Endereço não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    enderecosEncontrados = enderecos;
                    label6.Text = string.Format("CEPs encontrados: {0} (selecione um):", enderecos.Length);
                    cmbCEPs.DataSource = null;
                    cmbCEPs.DisplayMember = "Cep";
                    cmbCEPs.DataSource = enderecosEncontrados;
                    cmbCEPs.SelectedIndex = 0;

                    if (enderecos.Length > 1)
                    {
                        MessageBox.Show("Foram encontrados vários CEPs. Use o campo 'CEPs encontrados' para selecionar outro resultado.", "Consulta concluída", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    enderecosEncontrados = new ViaCepEndereco[0];
                    label6.Text = "CEPs encontrados (se houver mais de um):";
                    cmbCEPs.DataSource = null;
                    ViaCepEndereco endereco = Desserializar<ViaCepEndereco>(json);

                    if (endereco == null || string.IsNullOrWhiteSpace(endereco.Cep))
                    {
                        LimparResultado();
                        MessageBox.Show("CEP não encontrado.", "Consulta sem resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    PreencherResultado(endereco);
                }
            }
            catch (TaskCanceledException excecao)
            {
                LogTecnico.Registrar("Consulta cancelada por tempo esgotado", excecao);
                MessageBox.Show("A consulta demorou demais e foi cancelada.", "Tempo esgotado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (HttpRequestException excecao)
            {
                LogTecnico.Registrar("Falha de comunicação com o serviço ViaCEP", excecao);
                MessageBox.Show("Não foi possível acessar o serviço de CEP. Verifique sua conexão com a internet.", "Erro de conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (SerializationException excecao)
            {
                LogTecnico.Registrar("Resposta inválida recebida do serviço ViaCEP", excecao);
                MessageBox.Show("O serviço de CEP retornou uma resposta inválida. Tente novamente em alguns instantes.", "Resposta inválida", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception excecao)
            {
                LogTecnico.Registrar("Erro inesperado durante consulta", excecao);
                MessageBox.Show("Ocorreu um erro inesperado durante a consulta. Tente novamente.", "Erro na consulta", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConsultar.Enabled = true;
                btnConsultar.Text = textoBotaoConsultar;
                btnConsultar.Cursor = Cursors.Hand;
            }
        }

        private void btnLimpar_Click(object sender, EventArgs e)
        {
            txtCEP.Clear();
            LimparResultado();
            txtEstado.Focus();
        }

        private void ArmazenarNoCache(string chave, string resposta)
        {
            cacheRespostas[chave] = resposta;
            ordemCacheRespostas.Remove(chave);
            ordemCacheRespostas.Add(chave);

            while (ordemCacheRespostas.Count > 20)
            {
                string chaveMaisAntiga = ordemCacheRespostas[0];
                ordemCacheRespostas.RemoveAt(0);
                cacheRespostas.Remove(chaveMaisAntiga);
            }

            LogTecnico.Registrar("Resposta armazenada no cache recente");
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PreencherResultado(ViaCepEndereco endereco)
        {
            txtCEP.Text = endereco.Cep;
            txtEstado.Text = endereco.Uf;
            txtCidade.Text = endereco.Localidade;
            txtBairro.Text = endereco.Bairro;
            txtLogradouro.Text = endereco.Logradouro;
            int indiceResultado = Array.IndexOf(enderecosEncontrados, endereco);
            string observacao = enderecosEncontrados.Length > 0
                ? string.Format("CEP {0} | Resultado {1} de {2}. ", endereco.Cep, indiceResultado + 1, enderecosEncontrados.Length)
                : string.Format("CEP {0}. ", endereco.Cep);

            observacao += string.IsNullOrWhiteSpace(endereco.Complemento)
                ? "Faixa numérica não informada pela base de CEP."
                : "Complemento: " + endereco.Complemento.Trim() + ".";

            if (!string.IsNullOrWhiteSpace(endereco.Unidade))
            {
                observacao += " Unidade: " + endereco.Unidade.Trim() + ".";
            }

            txtObservacao.Text = observacao;
            btnCopiarCep.Enabled = !string.IsNullOrWhiteSpace(endereco.Cep);
            btnExportar.Enabled = !string.IsNullOrWhiteSpace(endereco.Cep);
            RegistrarHistorico(endereco.Cep);
        }

        private void RegistrarHistorico(string cep)
        {
            string cepFormatado = CepUtil.SomenteNumeros(cep);
            if (cepFormatado.Length != 8)
            {
                return;
            }

            cepFormatado = cepFormatado.Insert(5, "-");
            historicoCEPs.Remove(cepFormatado);
            historicoCEPs.Insert(0, cepFormatado);

            if (historicoCEPs.Count > 10)
            {
                historicoCEPs.RemoveAt(historicoCEPs.Count - 1);
            }

            atualizandoHistorico = true;
            cmbHistorico.DataSource = null;
            cmbHistorico.DataSource = historicoCEPs.ToList();
            atualizandoHistorico = false;
            SalvarHistorico();
        }

        private void CarregarHistorico()
        {
            try
            {
                if (!File.Exists(arquivoHistorico))
                {
                    return;
                }

                historicoCEPs.Clear();
                foreach (string linha in File.ReadAllLines(arquivoHistorico, Encoding.UTF8))
                {
                    string cep = CepUtil.SomenteNumeros(linha);
                    if (cep.Length == 8 && !historicoCEPs.Contains(cep.Insert(5, "-")))
                    {
                        historicoCEPs.Add(cep.Insert(5, "-"));
                    }

                    if (historicoCEPs.Count == 10)
                    {
                        break;
                    }
                }

                atualizandoHistorico = true;
                cmbHistorico.DataSource = null;
                cmbHistorico.DataSource = historicoCEPs.ToList();
                atualizandoHistorico = false;
                LogTecnico.Registrar("Histórico de CEPs carregado");
            }
            catch (Exception excecao)
            {
                LogTecnico.Registrar("Não foi possível carregar o histórico de CEPs", excecao);
            }
        }

        private void SalvarHistorico()
        {
            try
            {
                string pasta = Path.GetDirectoryName(arquivoHistorico);
                Directory.CreateDirectory(pasta);
                File.WriteAllLines(arquivoHistorico, historicoCEPs, Encoding.UTF8);
            }
            catch (Exception excecao)
            {
                LogTecnico.Registrar("Não foi possível salvar o histórico de CEPs", excecao);
            }
        }

        private void cmbHistorico_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (atualizandoHistorico)
            {
                return;
            }

            string cep = cmbHistorico.SelectedItem as string;
            if (!string.IsNullOrWhiteSpace(cep))
            {
                txtCEP.Text = cep;
                txtCEP.Focus();
                txtCEP.SelectionStart = txtCEP.Text.Length;
            }
        }

        private void LimparResultado()
        {
            enderecosEncontrados = new ViaCepEndereco[0];
            label6.Text = "CEPs encontrados (se houver mais de um):";
            cmbCEPs.DataSource = null;
            txtEstado.Clear();
            txtCidade.Clear();
            txtBairro.Clear();
            txtLogradouro.Clear();
            txtObservacao.Clear();
            btnCopiarCep.Enabled = false;
            btnExportar.Enabled = false;
        }

        private void btnCopiarCep_Click(object sender, EventArgs e)
        {
            string cep = txtCEP.Text.Trim();
            if (string.IsNullOrWhiteSpace(cep))
            {
                return;
            }

            Clipboard.SetText(cep);
            MessageBox.Show("CEP copiado para a área de transferência.", "CEP copiado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            string cep = txtCEP.Text.Trim();
            if (string.IsNullOrWhiteSpace(cep))
            {
                return;
            }

            using (SaveFileDialog dialogo = new SaveFileDialog())
            {
                dialogo.Title = "Exportar consulta de CEP";
                dialogo.Filter = "Arquivo de texto (*.txt)|*.txt|Todos os arquivos (*.*)|*.*";
                dialogo.DefaultExt = "txt";
                dialogo.AddExtension = true;
                dialogo.FileName = "ConsultaCEP_" + cep.Replace("-", "") + ".txt";

                if (dialogo.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string conteudo = string.Format(
                    "Consulta de CEP{0}{0}CEP: {1}{0}UF: {2}{0}Localidade: {3}{0}Bairro/Distrito: {4}{0}Logradouro: {5}{0}Observação: {6}{0}{0}Fonte: ViaCEP",
                    Environment.NewLine,
                    txtCEP.Text,
                    txtEstado.Text,
                    txtCidade.Text,
                    txtBairro.Text,
                    txtLogradouro.Text,
                    txtObservacao.Text);

                try
                {
                    File.WriteAllText(dialogo.FileName, conteudo, Encoding.UTF8);
                    MessageBox.Show("Consulta exportada com sucesso.", "Exportação concluída", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (IOException excecao)
                {
                    LogTecnico.Registrar("Falha de entrada e saída ao exportar consulta", excecao);
                    MessageBox.Show("Não foi possível salvar o arquivo. Verifique o local escolhido e tente novamente.", "Erro na exportação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (UnauthorizedAccessException excecao)
                {
                    LogTecnico.Registrar("Acesso negado ao exportar consulta", excecao);
                    MessageBox.Show("Não há permissão para salvar nesse local. Escolha outra pasta e tente novamente.", "Acesso negado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void cmbCEPs_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarCepSelecionado();
        }

        private void cmbCEPs_SelectionChangeCommitted(object sender, EventArgs e)
        {
            AtualizarCepSelecionado();
        }

        private void AtualizarCepSelecionado()
        {
            ViaCepEndereco endereco = cmbCEPs.SelectedItem as ViaCepEndereco;
            if (endereco != null)
            {
                PreencherResultado(endereco);
            }
        }

        private static string SomenteNumeros(string valor)
        {
            return CepUtil.SomenteNumeros(valor);
        }

        private static T Desserializar<T>(string json)
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                return (T)serializer.ReadObject(stream);
            }
        }

        private static void ArredondarBotao(Button botao, int raio)
        {
            using (GraphicsPath caminho = new GraphicsPath())
            {
                int diametro = raio * 2;
                Rectangle area = new Rectangle(0, 0, botao.Width, botao.Height);

                caminho.AddArc(area.X, area.Y, diametro, diametro, 180, 90);
                caminho.AddArc(area.Right - diametro, area.Y, diametro, diametro, 270, 90);
                caminho.AddArc(area.Right - diametro, area.Bottom - diametro, diametro, diametro, 0, 90);
                caminho.AddArc(area.X, area.Bottom - diametro, diametro, diametro, 90, 90);
                caminho.CloseFigure();

                botao.Region = new Region(caminho);
            }
        }
    }

    [DataContract]
    internal class ViaCepEndereco
    {
        [DataMember(Name = "cep")] public string Cep { get; set; }
        [DataMember(Name = "logradouro")] public string Logradouro { get; set; }
        [DataMember(Name = "bairro")] public string Bairro { get; set; }
        [DataMember(Name = "complemento")] public string Complemento { get; set; }
        [DataMember(Name = "unidade")] public string Unidade { get; set; }
        [DataMember(Name = "localidade")] public string Localidade { get; set; }
        [DataMember(Name = "uf")] public string Uf { get; set; }
    }
}
