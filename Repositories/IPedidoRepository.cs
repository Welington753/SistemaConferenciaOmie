using System;
using System.Collections.Generic;
using SistemaConferenciaPedidos.Models;

namespace SistemaConferenciaPedidos.Repositories
{
    public interface IPedidoRepository
    {
        List<PedidoConferencia> ObterTodos();
        PedidoConferencia ObterPorId(int id);
        PedidoConferencia ObterPorNumero(string numeroPedido);

        /// <summary>
        /// Retorna pedidos cujo dia operacional (DataPrevisao, ou DataCriacao se ausente)
        /// esteja dentro do intervalo [inicio, fimExclusivo).
        /// </summary>
        List<PedidoConferencia> ObterPorPeriodo(DateTime inicio, DateTime fimExclusivo, bool incluirOcultos = false);

        void SalvarOuAtualizar(PedidoConferencia pedido);

        void SalvarOuAtualizarVarios(List<PedidoConferencia> pedidos);
        void SalvarOuAtualizarPreservandoStatus(PedidoConferencia pedido);

        bool OcultarPedido(string numeroPedidoCliente);

        void Limpar();
        bool LimparPedidosPorDia(System.DateTime dataAlvo, bool restaurarRemovidos = false);
    }
}