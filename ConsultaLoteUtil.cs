using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsultaCEPs
{
    public sealed class ResultadoValidacaoLote
    {
        public List<string> Ceps { get; set; }
        public int LinhasLidas { get; set; }
        public int Invalidos { get; set; }
        public int Duplicados { get; set; }
        public int Excedentes { get; set; }
    }

    public static class ConsultaLoteUtil
    {
        public static List<string> ExtrairCepsValidos(IEnumerable<string> linhas, int limite)
        {
            return Analisar(linhas, limite).Ceps;
        }

        public static ResultadoValidacaoLote Analisar(IEnumerable<string> linhas, int limite)
        {
            ResultadoValidacaoLote resultado = new ResultadoValidacaoLote
            {
                Ceps = new List<string>()
            };

            if (linhas == null || limite < 1)
            {
                return resultado;
            }

            HashSet<string> encontrados = new HashSet<string>();
            foreach (string linha in linhas)
            {
                resultado.LinhasLidas++;
                string cep = CepUtil.SomenteNumeros(linha);
                if (cep.Length != 8)
                {
                    resultado.Invalidos++;
                    continue;
                }

                if (!encontrados.Add(cep))
                {
                    resultado.Duplicados++;
                }
            }

            resultado.Excedentes = Math.Max(0, encontrados.Count - limite);
            resultado.Ceps = encontrados.Take(limite).ToList();
            return resultado;
        }
    }
}
