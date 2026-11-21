using System.Collections.Generic;
using System.Linq;

namespace ConsultaCEPs
{
    public static class ConsultaLoteUtil
    {
        public static List<string> ExtrairCepsValidos(IEnumerable<string> linhas, int limite)
        {
            if (linhas == null || limite < 1)
            {
                return new List<string>();
            }

            return linhas
                .Select(CepUtil.SomenteNumeros)
                .Where(cep => cep.Length == 8)
                .Distinct()
                .Take(limite)
                .ToList();
        }
    }
}
