using SistemaConferenciaPedidos.Models;

namespace SistemaConferenciaPedidos.Services
{
    public interface IValidacaoPreImpressaoService
    {
        ResultadoValidacaoPreImpressao ValidarAntesDaImpressao(PedidoConferencia snapshotPedido, string caminhoZipShopee = null);
    }
}
