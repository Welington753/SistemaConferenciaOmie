using System;
using System.Collections.Generic;
using System.Linq;
using SistemaConferenciaPedidos.Models;

namespace SistemaConferenciaPedidos.Services
{
    public static class PedidoRepository
    {
        private static readonly List<PedidoConferencia> pedidos = new List<PedidoConferencia>();

        public static void Limpar()
        {
            pedidos.Clear();
        }

        public static void AdicionarOuAtualizar(PedidoConferencia pedido)
        {
            if (pedido == null)
                return;

            string chave = (pedido.NumeroPedidoCliente ?? "").Trim();

            var existente = pedidos.FirstOrDefault(p =>
                (p.NumeroPedidoCliente ?? "").Trim().Equals(chave, StringComparison.OrdinalIgnoreCase));

            if (existente == null)
            {
                pedidos.Add(pedido);
                return;
            }

            existente.CodigoEtiqueta = pedido.CodigoEtiqueta;
            existente.NumeroPedidoCliente = pedido.NumeroPedidoCliente;
            existente.NomeCliente = pedido.NomeCliente;
            existente.Marketplace = pedido.Marketplace;
            existente.JsonItens = pedido.JsonItens;
            existente.EtiquetaMarketplaceZpl = pedido.EtiquetaMarketplaceZpl;
            existente.Status = pedido.Status;
            existente.DataCriacao = pedido.DataCriacao;
            existente.DataConferencia = pedido.DataConferencia;
        }

        public static void Adicionar(PedidoConferencia pedido)
        {
            if (pedido != null)
                pedidos.Add(pedido);
        }

        public static List<PedidoConferencia> ObterTodos()
        {
            return pedidos.ToList();
        }

        public static PedidoConferencia BuscarPorCodigo(string codigo)
        {
            return pedidos.FirstOrDefault(p =>
                string.Equals(p.CodigoEtiqueta ?? "", codigo ?? "", StringComparison.OrdinalIgnoreCase));
        }


        public static void AdicionarOuAtualizarPreservandoStatus(PedidoConferencia pedidoNovo)
        {
            if (pedidoNovo == null)
                return;

            var existente = pedidos.FirstOrDefault(p =>
                 string.Equals(
                    (p.NumeroPedidoCliente ?? "").Trim(),
                    (pedidoNovo.NumeroPedidoCliente ?? "").Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (existente == null)
            {
                pedidos.Add(pedidoNovo);
                return;
            }

            // Atualiza apenas os dados vindos da Omie
            existente.NomeCliente = pedidoNovo.NomeCliente;
            existente.Marketplace = pedidoNovo.Marketplace;
            existente.JsonItens = pedidoNovo.JsonItens;

           
        }

        public static void MarcarComoConferido(string codigo)
        {
            var pedido = BuscarPorCodigo(codigo);

            if (pedido != null)
            {
                pedido.Status = "Conferido";
                pedido.DataConferencia = DateTime.Now;
            }
        }
    }
}