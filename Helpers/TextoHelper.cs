using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SistemaConferenciaPedidos.Helpers
{
    public static class TextoHelper
    {
        public static string NormalizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            string textoLower = texto.Trim().ToLowerInvariant();
            string textoNormalizado = textoLower.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in textoNormalizado)
            {
                UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(c);
                if (categoria != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string SomenteDigitos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            return new string(texto.Where(char.IsDigit).ToArray());
        }

        public static string SomenteLetrasENumeros(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            return new string(texto.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }

        public static string SomenteNumeros(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return "";

            return Regex.Replace(valor, @"\D", "");
        }
    }
}