using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsultaCEPs
{
    public partial class FrmConsultaCEP : Form
    {
        private static readonly HttpClient clienteHttp = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public FrmConsultaCEP()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            InitializeComponent();
        }

        private void FrmConsultaCEP_Load(object sender, EventArgs e)
        {
            ArredondarBotao(btnConsultar, 12);
            ArredondarBotao(btnLimpar, 10);
            ArredondarBotao(btnSair, 10);
        }

        private void txtEstado_TextChanged(object sender, EventArgs e)
        {

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
                (string.IsNullOrWhiteSpace(estado) || string.IsNullOrWhiteSpace(cidade) || string.IsNullOrWhiteSpace(logradouro)))
            {
                MessageBox.Show("Para buscar por endereço, preencha UF, Cidade e Logradouro.", "Dados incompletos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!informouEndereco && !string.IsNullOrWhiteSpace(cep) && cep.Length != 8)
            {
                MessageBox.Show("O CEP deve conter 8 números.", "CEP inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnConsultar.Enabled = false;

            try
            {
                bool buscaPorEndereco = informouEndereco;
                string url = buscaPorEndereco
                    ? string.Format("https://viacep.com.br/ws/{0}/{1}/{2}/json/", Uri.EscapeDataString(estado), Uri.EscapeDataString(cidade), Uri.EscapeDataString(logradouro))
                    : string.Format("https://viacep.com.br/ws/{0}/json/", cep);

                string json = await clienteHttp.GetStringAsync(url);

                if (buscaPorEndereco)
                {
                    ViaCepEndereco[] enderecos = Desserializar<ViaCepEndereco[]>(json);

                    if (enderecos == null || enderecos.Length == 0)
                    {
                        LimparResultado();
                        MessageBox.Show("Rua inexistente ou endereço não encontrado.", "Endereço não encontrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    PreencherResultado(enderecos[0]);

                    if (enderecos.Length > 1)
                    {
                        MessageBox.Show("Foram encontrados vários endereços. O primeiro resultado foi exibido.", "Consulta concluída", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
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
            catch (TaskCanceledException)
            {
                MessageBox.Show("A consulta demorou demais e foi cancelada.", "Tempo esgotado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Não foi possível acessar o serviço de CEP. Verifique sua conexão com a internet.", "Erro de conexão", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao consultar o endereço: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConsultar.Enabled = true;
            }
        }

        private void btnLimpar_Click(object sender, EventArgs e)
        {
            txtCEP.Clear();
            LimparResultado();
            txtEstado.Focus();
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
        }

        private void LimparResultado()
        {
            txtEstado.Clear();
            txtCidade.Clear();
            txtBairro.Clear();
            txtLogradouro.Clear();
        }

        private static string SomenteNumeros(string valor)
        {
            StringBuilder numeros = new StringBuilder();
            foreach (char caractere in valor)
            {
                if (char.IsDigit(caractere)) numeros.Append(caractere);
            }
            return numeros.ToString();
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
        [DataMember(Name = "localidade")] public string Localidade { get; set; }
        [DataMember(Name = "uf")] public string Uf { get; set; }
    }
}
