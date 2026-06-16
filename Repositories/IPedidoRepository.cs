using SistemaConferenciaPedidos.Models;
using System.Collections.Generic;

namespace SistemaConferenciaPedidos.Repositories
{
    public interface IPedidoRepository
    {
        List<PedidoConferencia> ObterTodos();

        void SalvarOuAtualizar(PedidoConferencia pedido);

        void SalvarOuAtualizarVarios(List<PedidoConferencia> pedidos);
        void SalvarOuAtualizarPreservandoStatus(PedidoConferencia pedido);

        void Limpar();
    }
}