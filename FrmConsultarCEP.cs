using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
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
        private readonly Dictionary<string, DateTime> datasCacheRespostas = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> ordemCacheRespostas = new List<string>();
        private bool atualizandoHistorico;
        private CancellationTokenSource cancelamentoConsulta;
        private static readonly string arquivoHistorico = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ConsultarCEP",
            "historico-ceps.txt");
        private static readonly string arquivoEscalaFonte = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ConsultarCEP",
            "escala-fonte.txt");
        private float escalaFonte = 1.0f;
        private const float EscalaFonteMinima = 0.85f;
        private const float EscalaFonteMaxima = 1.25f;
        private const float TamanhoFonteBase = 8.25f;
        private static readonly TimeSpan ValidadeCache = TimeSpan.FromMinutes(10);

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

            VerificarAtualizacaoAsync();
        }

        private async void VerificarAtualizacaoAsync()
        {
            try
            {
                Version versaoAtual = typeof(FrmConsultaCEP).Assembly.GetName().Version;
                AtualizacaoDisponivel atualizacao = await AtualizacaoChecker.VerificarAsync(versaoAtual);
                if (atualizacao == null || IsDisposed || string.IsNullOrWhiteSpace(atualizacao.Url))
                {
                    return;
                }

                DialogResult resposta = MessageBox.Show(
                    string.Format("A versão {0} do Consultar CEP está disponível. Deseja abrir a página de atualização?", atualizacao.Versao),
                    "Atualização disponível",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (resposta == DialogResult.Yes)
                {
                    Process.Start(atualizacao.Url);
                }
            }
            catch (Exception excecao)
            {
                LogTecnico.Registrar("Não foi possível verificar atualizações", excecao);
            }
        }

        private void FrmConsultaCEP_FormClosed(object sender, FormClosedEventArgs e)
        {
            SalvarEscalaFonte();
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
            ConfigurarAcessibilidade();
            ArredondarBotao(btnConsultar, 12);
            ArredondarBotao(btnConsultaLote, 12);
            ArredondarBotao(btnLimpar, 10);
            ArredondarBotao(btnSair, 10);
            ConfigurarEfeitoHover(btnConsultar, Color.FromArgb(38, 115, 170), Color.FromArgb(63, 145, 205));
            ConfigurarEfeitoHover(btnConsultaLote, Color.FromArgb(72, 137, 104), Color.FromArgb(95, 164, 126));
            ConfigurarEfeitoHover(btnLimpar, Color.FromArgb(92, 155, 213), Color.FromArgb(124, 181, 230));
            ConfigurarEfeitoHover(btnSair, Color.FromArgb(27, 64, 96), Color.FromArgb(48, 94, 130));
            ConfigurarEfeitoHover(btnCopiarCep, Color.FromArgb(63, 145, 205), Color.FromArgb(92, 169, 220));
            ConfigurarEfeitoHover(btnExportar, Color.FromArgb(72, 137, 104), Color.FromArgb(95, 164, 126));
            ConfigurarEfeitoHover(btnDiminuirFonte, Color.FromArgb(92, 155, 213), Color.FromArgb(124, 181, 230));
            ConfigurarEfeitoHover(btnAumentarFonte, Color.FromArgb(38, 115, 170), Color.FromArgb(63, 145, 205));
            ConfigurarEfeitoHover(btnConfiguracoes, Color.FromArgb(27, 64, 96), Color.FromArgb(48, 94, 130));
            btnCopiarCep.Enabled = false;
            btnExportar.Enabled = false;
            CarregarHistorico();
            CarregarEscalaFonte();
            AplicarEscalaFonte();
            AtualizarLimitesMaximizacao();
            btnConsultar.Image = CriarImagemBotaoConsultar();
            btnConsultar.ImageAlign = ContentAlignment.MiddleLeft;
            btnConsultar.TextImageRelation = TextImageRelation.ImageBeforeText;
            txtCEP.Focus();
        }

        private void ConfigurarAcessibilidade()
        {
            txtCEP.AccessibleName = "Busca rápida por CEP";
            txtCEP.AccessibleDescription = "Informe um CEP com oito números para realizar uma consulta rápida.";
            txtEstado.AccessibleName = "UF";
            txtEstado.AccessibleDescription = "Informe a sigla do estado com duas letras.";
            txtCidade.AccessibleName = "Cidade";
            txtCidade.AccessibleDescription = "Informe a cidade do endereço a ser consultado.";
            txtLogradouro.AccessibleName = "Logradouro";
            txtLogradouro.AccessibleDescription = "Informe o nome da rua, avenida ou outro logradouro.";
            cmbCEPs.AccessibleName = "CEPs encontrados";
            cmbCEPs.AccessibleDescription = "Selecione um CEP quando houver mais de um resultado.";
            cmbHistorico.AccessibleName = "Histórico de CEPs";
            cmbHistorico.AccessibleDescription = "Selecione uma consulta realizada anteriormente.";
            txtBairro.AccessibleName = "Bairro ou distrito";
            txtBairro.AccessibleDescription = "Resultado preenchido automaticamente pela consulta.";
            txtObservacao.AccessibleName = "Observação do CEP";
            txtObservacao.AccessibleDescription = "Detalhes adicionais do CEP selecionado.";
            btnConsultar.AccessibleName = "Consultar CEP";
            btnConsultar.AccessibleDescription = "Executa a consulta do CEP ou do endereço informado; durante uma consulta, cancela a operação.";
            btnConsultaLote.AccessibleName = "Consulta em lote";
            btnConsultaLote.AccessibleDescription = "Consulta vários CEPs a partir de um arquivo de texto e exporta os resultados em CSV.";
            btnCopiarCep.AccessibleName = "Copiar CEP";
            btnCopiarCep.AccessibleDescription = "Copia o CEP selecionado para a área de transferência.";
            btnExportar.AccessibleName = "Exportar consulta";
            btnExportar.AccessibleDescription = "Salva o resultado em TXT, CSV ou Excel.";
            btnDiminuirFonte.AccessibleName = "Diminuir tamanho da fonte";
            btnAumentarFonte.AccessibleName = "Aumentar tamanho da fonte";
            btnConfiguracoes.AccessibleName = "Configurações";
            btnConfiguracoes.AccessibleDescription = "Abre as configurações da aplicação.";
            btnLimpar.AccessibleName = "Limpar formulário";
            btnSair.AccessibleName = "Sair da aplicação";

            txtCEP.TabIndex = 0;
            btnConsultar.TabIndex = 1;
            btnConsultaLote.TabIndex = 2;
            groupBox1.TabIndex = 3;
            txtEstado.TabIndex = 0;
            txtCidade.TabIndex = 1;
            txtLogradouro.TabIndex = 2;
            cmbCEPs.TabIndex = 3;
            cmbHistorico.TabIndex = 4;
            txtBairro.TabStop = false;
            txtObservacao.TabStop = false;
            btnCopiarCep.TabIndex = 4;
            btnExportar.TabIndex = 5;
            btnLimpar.TabIndex = 6;
            btnSair.TabIndex = 7;
            btnDiminuirFonte.TabIndex = 8;
            btnAumentarFonte.TabIndex = 9;
            btnConfiguracoes.TabIndex = 10;
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
            SalvarEscalaFonte();
        }

        private void btnAumentarFonte_Click(object sender, EventArgs e)
        {
            escalaFonte = Math.Min(EscalaFonteMaxima, escalaFonte + 0.05f);
            AplicarEscalaFonte();
            SalvarEscalaFonte();
        }

        private void btnConfiguracoes_Click(object sender, EventArgs e)
        {
            using (FrmConfiguracoes configuracoes = new FrmConfiguracoes(escalaFonte))
            {
                if (configuracoes.ShowDialog(this) == DialogResult.OK)
                {
                    escalaFonte = configuracoes.EscalaSelecionada;
                    AplicarEscalaFonte();
                    SalvarEscalaFonte();
                }
            }
        }

        private void CarregarEscalaFonte()
        {
            try
            {
                if (!File.Exists(arquivoEscalaFonte))
                {
                    return;
                }

                float escalaSalva;
                string conteudo = File.ReadAllText(arquivoEscalaFonte).Trim();
                if (float.TryParse(conteudo, NumberStyles.Float, CultureInfo.InvariantCulture, out escalaSalva))
                {
                    escalaFonte = Math.Max(EscalaFonteMinima, Math.Min(EscalaFonteMaxima, escalaSalva));
                }
            }
            catch (Exception excecao)
            {
                LogTecnico.Registrar("Não foi possível carregar o tamanho da fonte", excecao);
            }
        }

        private void SalvarEscalaFonte()
        {
            try
            {
                string pasta = Path.GetDirectoryName(arquivoEscalaFonte);
                Directory.CreateDirectory(pasta);
                File.WriteAllText(arquivoEscalaFonte, escalaFonte.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception excecao)
            {
                LogTecnico.Registrar("Não foi possível salvar o tamanho da fonte", excecao);
            }
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
            if (cancelamentoConsulta != null)
            {
                cancelamentoConsulta.Cancel();
                return;
            }

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
            cancelamentoConsulta = new CancellationTokenSource();
            CancellationToken token = cancelamentoConsulta.Token;
            btnConsultar.Enabled = true;
            btnConsultar.Text = "Cancelar";
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
                if (TentarObterDoCache(url, out json))
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

                    using (HttpResponseMessage resposta = await clienteHttp.GetAsync(url, token))
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
                if (token.IsCancellationRequested)
                {
                    LogTecnico.Registrar("Consulta cancelada pelo usuário", excecao);
                    MessageBox.Show("A consulta foi cancelada.", "Consulta cancelada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    LogTecnico.Registrar("Consulta cancelada por tempo esgotado", excecao);
                    MessageBox.Show("A consulta demorou demais e foi cancelada.", "Tempo esgotado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
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
                if (cancelamentoConsulta != null)
                {
                    cancelamentoConsulta.Dispose();
                    cancelamentoConsulta = null;
                }

                btnConsultar.Enabled = true;
                btnConsultar.Text = textoBotaoConsultar;
                btnConsultar.Cursor = Cursors.Hand;
            }
        }

        private async void btnConsultaLote_Click(object sender, EventArgs e)
        {
            if (cancelamentoConsulta != null)
            {
                cancelamentoConsulta.Cancel();
                return;
            }

            using (OpenFileDialog abrir = new OpenFileDialog())
            {
                abrir.Title = "Selecionar arquivo com CEPs";
                abrir.Filter = "Arquivo de texto (*.txt)|*.txt|Todos os arquivos (*.*)|*.*";
                abrir.CheckFileExists = true;

                if (abrir.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                ResultadoValidacaoLote validacao = ConsultaLoteUtil.Analisar(
                    File.ReadAllLines(abrir.FileName, Encoding.UTF8), 100);
                List<string> ceps = validacao.Ceps;

                if (ceps.Count == 0)
                {
                    MessageBox.Show(
                        string.Format("Nenhum CEP válido foi encontrado no arquivo.\n\nLinhas lidas: {0}\nInválidos: {1}", validacao.LinhasLidas, validacao.Invalidos),
                        "Consulta em lote", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (validacao.Invalidos > 0 || validacao.Duplicados > 0 || validacao.Excedentes > 0)
                {
                    string aviso = string.Format(
                        "Foram encontrados {0} CEP(s) válido(s) para consulta.\n\nInválidos ignorados: {1}\nDuplicados ignorados: {2}",
                        ceps.Count, validacao.Invalidos, validacao.Duplicados);

                    if (validacao.Excedentes > 0)
                    {
                        aviso += string.Format("\nLimite de 100 CEPs: {0} excedente(s) ignorado(s).", validacao.Excedentes);
                    }

                    MessageBox.Show(aviso, "Validação do arquivo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                List<ResultadoConsultaLote> resultados = new List<ResultadoConsultaLote>();
                cancelamentoConsulta = new CancellationTokenSource();
                CancellationToken token = cancelamentoConsulta.Token;
                btnConsultaLote.Enabled = true;
                btnConsultar.Enabled = false;
                btnConsultaLote.Cursor = Cursors.WaitCursor;
                btnConsultaLote.Text = "Cancelar lote";
                Stopwatch cronometroLote = Stopwatch.StartNew();

                try
                {
                    for (int indice = 0; indice < ceps.Count; indice++)
                    {
                        btnConsultaLote.Text = string.Format("Lote {0}/{1}", indice + 1, ceps.Count);
                        btnConsultaLote.Refresh();
                        resultados.Add(await ConsultarCepParaLoteAsync(ceps[indice], token));
                    }

                    using (SaveFileDialog salvar = new SaveFileDialog())
                    {
                        salvar.Title = "Salvar resultados da consulta em lote";
                        salvar.Filter = "Arquivo CSV (*.csv)|*.csv|Arquivo Excel (*.xlsx)|*.xlsx";
                        salvar.DefaultExt = "csv";
                        salvar.AddExtension = true;
                        salvar.FileName = "ConsultasCEP_Lote.csv";

                        if (salvar.ShowDialog() == DialogResult.OK)
                        {
                            bool exportarXlsx = salvar.FilterIndex == 2;
                            string caminhoExportacao = exportarXlsx
                                ? Path.ChangeExtension(salvar.FileName, "xlsx")
                                : Path.ChangeExtension(salvar.FileName, "csv");

                            if (exportarXlsx)
                            {
                                ExportarXlsx(caminhoExportacao, CriarLinhasXlsxLote(resultados));
                            }
                            else
                            {
                                File.WriteAllText(caminhoExportacao, CriarConteudoCsvLote(resultados), Encoding.UTF8);
                            }

                            LogTecnico.Registrar(exportarXlsx
                                ? "Consulta em lote exportada em XLSX"
                                : "Consulta em lote exportada em CSV");
                        }
                    }

                    cronometroLote.Stop();
                    EstatisticasConsultaLote estatisticas = ConsultaLoteUtil.CalcularEstatisticas(
                        resultados.Select(resultado => resultado.Status));
                    MessageBox.Show(
                        string.Format(
                            "Consulta em lote concluída.\n\nTotal processado: {0}\nEndereços encontrados: {1}\nCEPs não encontrados: {2}\nErros de consulta: {3}\nTempo total: {4:0.0} segundo(s)",
                            estatisticas.Total,
                            estatisticas.Encontrados,
                            estatisticas.NaoEncontrados,
                            estatisticas.Erros,
                            cronometroLote.Elapsed.TotalSeconds),
                        "Resumo da consulta em lote", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (OperationCanceledException excecao)
                {
                    LogTecnico.Registrar("Consulta em lote cancelada pelo usuário", excecao);
                    MessageBox.Show("A consulta em lote foi cancelada.", "Consulta cancelada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception excecao)
                {
                    LogTecnico.Registrar("Erro na consulta em lote", excecao);
                    MessageBox.Show("Não foi possível concluir a consulta em lote. Verifique o arquivo e tente novamente.", "Erro na consulta em lote", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (cancelamentoConsulta != null)
                    {
                        cancelamentoConsulta.Dispose();
                        cancelamentoConsulta = null;
                    }

                    btnConsultaLote.Enabled = true;
                    btnConsultar.Enabled = true;
                    btnConsultaLote.Text = "Consulta em lote";
                    btnConsultaLote.Cursor = Cursors.Hand;
                }
            }
        }

        private async Task<ResultadoConsultaLote> ConsultarCepParaLoteAsync(string cep, CancellationToken token)
        {
            string cepFormatado = cep.Insert(5, "-");
            string url = string.Format("https://viacep.com.br/ws/{0}/json/", cep);
            string json;

            try
            {
                if (!TentarObterDoCache(url, out json))
                {
                    if (!NetworkInterface.GetIsNetworkAvailable())
                    {
                        return ResultadoConsultaLote.Erro(cepFormatado, "Sem conexão");
                    }

                    using (HttpResponseMessage resposta = await clienteHttp.GetAsync(url, token))
                    {
                        resposta.EnsureSuccessStatusCode();
                        json = await resposta.Content.ReadAsStringAsync();
                    }

                    ArmazenarNoCache(url, json);
                }

                ViaCepEndereco endereco = Desserializar<ViaCepEndereco>(json);
                if (endereco == null || string.IsNullOrWhiteSpace(endereco.Cep))
                {
                    return ResultadoConsultaLote.Erro(cepFormatado, "Não encontrado");
                }

                return new ResultadoConsultaLote
                {
                    Cep = endereco.Cep,
                    Status = "OK",
                    Uf = endereco.Uf,
                    Localidade = endereco.Localidade,
                    Bairro = endereco.Bairro,
                    Logradouro = endereco.Logradouro,
                    Observacao = string.IsNullOrWhiteSpace(endereco.Complemento) ? string.Empty : endereco.Complemento
                };
            }
            catch (TaskCanceledException excecao)
            {
                if (token.IsCancellationRequested)
                {
                    throw;
                }

                LogTecnico.Registrar("CEP em lote expirou por tempo esgotado", excecao);
                return ResultadoConsultaLote.Erro(cepFormatado, "Tempo esgotado");
            }
            catch (HttpRequestException excecao)
            {
                LogTecnico.Registrar("CEP em lote falhou por erro de conexão", excecao);
                return ResultadoConsultaLote.Erro(cepFormatado, "Erro de conexão");
            }
            catch (Exception excecao)
            {
                LogTecnico.Registrar("CEP em lote falhou por erro inesperado", excecao);
                return ResultadoConsultaLote.Erro(cepFormatado, "Erro inesperado");
            }
        }

        private static string CriarConteudoCsvLote(IEnumerable<ResultadoConsultaLote> resultados)
        {
            string[] cabecalho = { "CEP", "Status", "UF", "Localidade", "Bairro/Distrito", "Logradouro", "Observação" };
            StringBuilder csv = new StringBuilder(EscaparLinhaCsv(cabecalho));
            foreach (ResultadoConsultaLote resultado in resultados)
            {
                string[] valores = { resultado.Cep, resultado.Status, resultado.Uf, resultado.Localidade, resultado.Bairro, resultado.Logradouro, resultado.Observacao };
                csv.AppendLine();
                csv.Append(EscaparLinhaCsv(valores));
            }

            return csv.ToString() + Environment.NewLine;
        }

        private static IEnumerable<string[]> CriarLinhasXlsxLote(IEnumerable<ResultadoConsultaLote> resultados)
        {
            foreach (ResultadoConsultaLote resultado in resultados)
            {
                yield return new[]
                {
                    resultado.Cep,
                    resultado.Status,
                    resultado.Uf,
                    resultado.Localidade,
                    resultado.Bairro,
                    resultado.Logradouro,
                    resultado.Observacao
                };
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
            datasCacheRespostas[chave] = DateTime.UtcNow;
            ordemCacheRespostas.Remove(chave);
            ordemCacheRespostas.Add(chave);

            while (ordemCacheRespostas.Count > 20)
            {
                string chaveMaisAntiga = ordemCacheRespostas[0];
                ordemCacheRespostas.RemoveAt(0);
                cacheRespostas.Remove(chaveMaisAntiga);
                datasCacheRespostas.Remove(chaveMaisAntiga);
            }

            LogTecnico.Registrar("Resposta armazenada no cache recente");
        }

        private bool TentarObterDoCache(string chave, out string resposta)
        {
            DateTime dataArmazenamento;
            if (cacheRespostas.TryGetValue(chave, out resposta) &&
                datasCacheRespostas.TryGetValue(chave, out dataArmazenamento))
            {
                if (DateTime.UtcNow - dataArmazenamento <= ValidadeCache)
                {
                    return true;
                }

                cacheRespostas.Remove(chave);
                datasCacheRespostas.Remove(chave);
                ordemCacheRespostas.Remove(chave);
                resposta = null;
                LogTecnico.Registrar("Entrada do cache expirada após 10 minutos");
                return false;
            }

            resposta = null;
            return false;
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
                dialogo.Filter = "Arquivo de texto (*.txt)|*.txt|Arquivo CSV (*.csv)|*.csv|Arquivo Excel (*.xlsx)|*.xlsx|Todos os arquivos (*.*)|*.*";
                dialogo.DefaultExt = "txt";
                dialogo.AddExtension = true;
                dialogo.FileName = "ConsultaCEP_" + cep.Replace("-", "") + ".txt";

                if (dialogo.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string caminhoExportacao = dialogo.FileName;
                if (dialogo.FilterIndex == 2)
                {
                    caminhoExportacao = Path.ChangeExtension(caminhoExportacao, "csv");
                }
                else if (dialogo.FilterIndex == 3)
                {
                    caminhoExportacao = Path.ChangeExtension(caminhoExportacao, "xlsx");
                }

                bool exportarCsv = string.Equals(Path.GetExtension(caminhoExportacao), ".csv", StringComparison.OrdinalIgnoreCase);
                bool exportarXlsx = string.Equals(Path.GetExtension(caminhoExportacao), ".xlsx", StringComparison.OrdinalIgnoreCase);
                string conteudo = exportarCsv
                    ? CriarConteudoCsv()
                    : string.Format(
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
                    if (exportarXlsx)
                    {
                        ExportarXlsx(caminhoExportacao);
                    }
                    else
                    {
                        File.WriteAllText(caminhoExportacao, conteudo, Encoding.UTF8);
                    }

                    LogTecnico.Registrar(exportarXlsx ? "Consulta exportada em XLSX" : exportarCsv ? "Consulta exportada em CSV" : "Consulta exportada em TXT");
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

        private string CriarConteudoCsv()
        {
            string[] cabecalho = { "CEP", "UF", "Localidade", "Bairro/Distrito", "Logradouro", "Observação" };
            string[] valores = { txtCEP.Text, txtEstado.Text, txtCidade.Text, txtBairro.Text, txtLogradouro.Text, txtObservacao.Text };
            return EscaparLinhaCsv(cabecalho) + Environment.NewLine + EscaparLinhaCsv(valores) + Environment.NewLine;
        }

        private static string EscaparLinhaCsv(IEnumerable<string> valores)
        {
            return string.Join(";", valores.Select(EscaparCampoCsv));
        }

        private static string EscaparCampoCsv(string valor)
        {
            string campo = valor ?? string.Empty;
            return "\"" + campo.Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " ") + "\"";
        }

        private void ExportarXlsx(string caminho)
        {
            string[] cabecalho = { "CEP", "UF", "Localidade", "Bairro/Distrito", "Logradouro", "Observação" };
            string[] valores = { txtCEP.Text, txtEstado.Text, txtCidade.Text, txtBairro.Text, txtLogradouro.Text, txtObservacao.Text };

            ExportarXlsx(caminho, cabecalho, new[] { valores });
        }

        private static void ExportarXlsx(string caminho, IEnumerable<string[]> linhas)
        {
            string[] cabecalho = { "CEP", "Status", "UF", "Localidade", "Bairro/Distrito", "Logradouro", "Observação" };
            ExportarXlsx(caminho, cabecalho, linhas);
        }

        private static void ExportarXlsx(string caminho, string[] cabecalho, IEnumerable<string[]> linhas)
        {
            using (FileStream arquivo = new FileStream(caminho, FileMode.Create, FileAccess.Write))
            using (ZipArchive planilha = new ZipArchive(arquivo, ZipArchiveMode.Create))
            {
                AdicionarEntradaXlsx(planilha, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                    "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                    "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                    "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                    "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                    "</Types>");
                AdicionarEntradaXlsx(planilha, "_rels/.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                    "</Relationships>");
                AdicionarEntradaXlsx(planilha, "xl/workbook.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                    "<sheets><sheet name=\"Consulta CEP\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
                AdicionarEntradaXlsx(planilha, "xl/_rels/workbook.xml.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                    "</Relationships>");

                StringBuilder planilhaXml = new StringBuilder(
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
                AdicionarLinhaXlsx(planilhaXml, 1, cabecalho);
                int numeroLinha = 2;
                foreach (string[] linha in linhas)
                {
                    AdicionarLinhaXlsx(planilhaXml, numeroLinha++, linha);
                }
                planilhaXml.Append("</sheetData></worksheet>");
                AdicionarEntradaXlsx(planilha, "xl/worksheets/sheet1.xml", planilhaXml.ToString());
            }
        }

        private static void AdicionarLinhaXlsx(StringBuilder xml, int numeroLinha, IEnumerable<string> valores)
        {
            xml.Append("<row r=\"").Append(numeroLinha).Append("\">");
            int numeroColuna = 0;
            foreach (string valor in valores)
            {
                string coluna = ((char)('A' + numeroColuna)).ToString();
                xml.Append("<c r=\"").Append(coluna).Append(numeroLinha).Append("\" t=\"inlineStr\"><is><t>");
                xml.Append(EscaparXml(valor));
                xml.Append("</t></is></c>");
                numeroColuna++;
            }

            xml.Append("</row>");
        }

        private static string EscaparXml(string valor)
        {
            return (valor ?? string.Empty)
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static void AdicionarEntradaXlsx(ZipArchive planilha, string nome, string conteudo)
        {
            ZipArchiveEntry entrada = planilha.CreateEntry(nome);
            using (StreamWriter escritor = new StreamWriter(entrada.Open(), new UTF8Encoding(false)))
            {
                escritor.Write(conteudo);
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

    internal class ResultadoConsultaLote
    {
        public string Cep { get; set; }
        public string Status { get; set; }
        public string Uf { get; set; }
        public string Localidade { get; set; }
        public string Bairro { get; set; }
        public string Logradouro { get; set; }
        public string Observacao { get; set; }

        public static ResultadoConsultaLote Erro(string cep, string status)
        {
            return new ResultadoConsultaLote { Cep = cep, Status = status };
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
