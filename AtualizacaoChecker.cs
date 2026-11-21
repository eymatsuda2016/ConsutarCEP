using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace ConsultaCEPs
{
    internal static class AtualizacaoChecker
    {
        private const string UrlReleaseMaisRecente = "https://api.github.com/repos/eymatsuda2016/ConsutarCEP/releases/latest";
        private static readonly HttpClient cliente = CriarCliente();

        public static async Task<AtualizacaoDisponivel> VerificarAsync(Version versaoAtual)
        {
            using (HttpResponseMessage resposta = await cliente.GetAsync(UrlReleaseMaisRecente))
            {
                if (!resposta.IsSuccessStatusCode)
                {
                    return null;
                }

                using (Stream fluxo = await resposta.Content.ReadAsStreamAsync())
                {
                    DataContractJsonSerializer serializador = new DataContractJsonSerializer(typeof(ReleaseGitHub));
                    ReleaseGitHub release = serializador.ReadObject(fluxo) as ReleaseGitHub;
                    Version versaoRemota;

                    if (release == null || !TryParseVersion(release.TagName, out versaoRemota) || versaoRemota <= versaoAtual)
                    {
                        return null;
                    }

                    return new AtualizacaoDisponivel(versaoRemota, release.HtmlUrl);
                }
            }
        }

        private static HttpClient CriarCliente()
        {
            HttpClient novoCliente = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            novoCliente.DefaultRequestHeaders.UserAgent.ParseAdd("ConsultarCEP-UpdateChecker/1.0");
            return novoCliente;
        }

        private static bool TryParseVersion(string tag, out Version versao)
        {
            string texto = (tag ?? string.Empty).Trim();
            if (texto.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                texto = texto.Substring(1);
            }

            return Version.TryParse(texto, out versao);
        }

        [DataContract]
        private sealed class ReleaseGitHub
        {
            [DataMember(Name = "tag_name")]
            public string TagName { get; set; }

            [DataMember(Name = "html_url")]
            public string HtmlUrl { get; set; }
        }
    }

    internal sealed class AtualizacaoDisponivel
    {
        public AtualizacaoDisponivel(Version versao, string url)
        {
            Versao = versao;
            Url = url;
        }

        public Version Versao { get; private set; }
        public string Url { get; private set; }
    }
}
