using System.Collections.Generic;

namespace SistemaConferenciaPedidos.Services
{
    public interface IPrinterEnvironment
    {
        bool ImpressoraExiste(string nome);
        bool ConfiguracaoValida(string nome);
        IEnumerable<string> ObterImpressorasInstaladas();
    }
}
