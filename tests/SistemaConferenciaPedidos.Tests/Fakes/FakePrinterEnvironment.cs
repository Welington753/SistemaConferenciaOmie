using System.Collections.Generic;
using SistemaConferenciaPedidos.Services;

namespace SistemaConferenciaPedidos.Tests.Fakes
{
    internal sealed class FakePrinterEnvironment : IPrinterEnvironment
    {
        public bool ConfiguracaoValida(string printerName) => true;
        public bool ImpressoraExiste(string printerName) => true;
        public IEnumerable<string> ObterImpressorasInstaladas() => new[] { "fake-thermal" };
    }
}
