using System.Threading;
using System.Threading.Tasks;

namespace SistemaConferenciaPedidos.Services
{
    public interface IOmiePedidoClient
    {
        Task<string> ListarPedidosAsync(int pagina = 1, CancellationToken cancellationToken = default);
        Task<string> ConsultarProdutoAsync(string codigo = "", string codigoProduto = "", CancellationToken cancellationToken = default);
    }
}
