using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;

namespace SistemaConferenciaPedidos.Services
{
    public class WindowsPrinterEnvironment : IPrinterEnvironment
    {
        public bool ImpressoraExiste(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) return false;
            
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                if (printer.Equals(nome, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ConfiguracaoValida(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) return false;
            
            var settings = new PrinterSettings { PrinterName = nome };
            return settings.IsValid;
        }

        public IEnumerable<string> ObterImpressorasInstaladas()
        {
            return PrinterSettings.InstalledPrinters.Cast<string>();
        }
    }
}
