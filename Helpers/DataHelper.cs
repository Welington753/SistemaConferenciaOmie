using System;
using System.Globalization;

namespace SistemaConferenciaPedidos.Helpers
{
    public static class DataHelper
    {
        public static string SerializarData(DateTime? data)
        {
            if (!data.HasValue)
                return null;
            
            return data.Value.ToString("O", CultureInfo.InvariantCulture);
        }

        public static DateTime? DesserializarData(string dataStr)
        {
            if (string.IsNullOrWhiteSpace(dataStr))
                return null;

            if (DateTime.TryParseExact(dataStr, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dataIso))
                return dataIso;

            if (DateTime.TryParseExact(dataStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataAntiga))
                return dataAntiga;

            if (DateTime.TryParse(dataStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataGenerica))
                return dataGenerica;

            return null; // Strict null fallback
        }

        public static DateTime DesserializarDataObrigatoria(string dataStr)
        {
            var data = DesserializarData(dataStr);
            return data ?? DateTime.Now; // Fallback only for required fields like DataCriacao
        }
    }
}
