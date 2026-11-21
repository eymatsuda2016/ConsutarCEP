using System.Text;

namespace ConsultaCEPs
{
    public static class CepUtil
    {
        public static string SomenteNumeros(string valor)
        {
            StringBuilder numeros = new StringBuilder();
            if (string.IsNullOrEmpty(valor))
            {
                return string.Empty;
            }

            foreach (char caractere in valor)
            {
                if (char.IsDigit(caractere))
                {
                    numeros.Append(caractere);
                }
            }

            return numeros.ToString();
        }

        public static bool EhValido(string valor)
        {
            return SomenteNumeros(valor).Length == 8;
        }

        public static string Formatar(string valor)
        {
            string cep = SomenteNumeros(valor);
            return cep.Length == 8 ? cep.Insert(5, "-") : valor;
        }
    }
}
