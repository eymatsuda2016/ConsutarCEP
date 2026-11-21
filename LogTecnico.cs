using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ConsultaCEPs
{
    internal static class LogTecnico
    {
        private static readonly object sincronizacao = new object();

        private static string CaminhoArquivo
        {
            get
            {
                string pasta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ConsultarCEP",
                    "Logs");
                return Path.Combine(pasta, "consultar-cep.log");
            }
        }

        public static void Registrar(string mensagem)
        {
            Registrar(mensagem, null);
        }

        public static void Registrar(string mensagem, Exception excecao)
        {
            try
            {
                lock (sincronizacao)
                {
                    string pasta = Path.GetDirectoryName(CaminhoArquivo);
                    Directory.CreateDirectory(pasta);

                    StringBuilder registro = new StringBuilder();
                    registro.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    registro.Append(" | ");
                    registro.Append(mensagem);

                    if (excecao != null)
                    {
                        registro.Append(" | ");
                        registro.Append(excecao.GetType().Name);
                        registro.Append(": ");
                        registro.Append(excecao.Message);
                        registro.AppendLine();
                        registro.Append(excecao.StackTrace);
                    }

                    File.AppendAllText(CaminhoArquivo, registro + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception erroAoRegistrar)
            {
                Debug.WriteLine("Falha ao gravar log técnico: " + erroAoRegistrar.Message);
            }
        }
    }
}
