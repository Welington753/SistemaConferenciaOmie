using System;

namespace SistemaConferenciaPedidos.Helpers
{
    public static class MarketplaceHelper
    {
        public static string NormalizarMarketplace(string marketplace)
        {
            string valor = (marketplace ?? "").Trim();

            if (valor.Equals("Mercado Livre", StringComparison.OrdinalIgnoreCase) ||
                valor.Equals("MLV", StringComparison.OrdinalIgnoreCase) ||
                valor.Equals("MLB", StringComparison.OrdinalIgnoreCase) ||
                valor.Equals("MELI", StringComparison.OrdinalIgnoreCase))
                return "MERCADO LIVRE";

            if (valor.Equals("Amazon", StringComparison.OrdinalIgnoreCase) ||
                valor.Equals("AMZ", StringComparison.OrdinalIgnoreCase))
                return "AMAZON";

            if (valor.Equals("Shopee", StringComparison.OrdinalIgnoreCase) ||
                valor.Equals("SHP", StringComparison.OrdinalIgnoreCase))
                return "SHOPEE";

            return valor.ToUpperInvariant();
        }

        public static int ObterOrdemMarketplace(string marketplace)
        {
            string mp = NormalizarMarketplace(marketplace);

            if (mp == "AMAZON")
                return 1;

            if (mp == "MERCADO LIVRE")
                return 2;

            if (mp == "SHOPEE")
                return 3;

            return 99;
        }
    }
}