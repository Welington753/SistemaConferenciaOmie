using System;

namespace SistemaConferenciaPedidos.Models
{
    public enum StatusValidacaoPreImpressao
    {
        Confirmado,
        NaoConfirmado,
        Invalido
    }

    public class ResultadoValidacaoPreImpressao
    {
        public bool Valido => Status == StatusValidacaoPreImpressao.Confirmado;
        public StatusValidacaoPreImpressao Status { get; set; }
        public string Mensagem { get; set; }
        public string MotivoBloqueio { get; set; }
        public string PedidoId { get; set; }
        public string NumeroPedido { get; set; }
        public string Marketplace { get; set; }
        public string CodigoEtiqueta { get; set; }
        public string NumeroPedidoEncontradoNaEtiqueta { get; set; }
        public string CodigoEncontradoNaEtiqueta { get; set; }
        public string HashEtiqueta { get; set; }
        public int QuantidadeCorrespondencias { get; set; }

        public static ResultadoValidacaoPreImpressao Aprovado(
            string pedidoId, string numeroPedido, string marketplace, string codigoEtiqueta, string hash)
        {
            return new ResultadoValidacaoPreImpressao
            {
                Status = StatusValidacaoPreImpressao.Confirmado,
                Mensagem = "Validação Aprovada",
                PedidoId = pedidoId,
                NumeroPedido = numeroPedido,
                Marketplace = marketplace,
                CodigoEtiqueta = codigoEtiqueta,
                HashEtiqueta = hash,
                QuantidadeCorrespondencias = 1
            };
        }

        public static ResultadoValidacaoPreImpressao AprovadoComAviso(
            string pedidoId, string numeroPedido, string marketplace, string codigoEtiqueta, string hash, string mensagem)
        {
            var res = Aprovado(pedidoId, numeroPedido, marketplace, codigoEtiqueta, hash);
            res.Mensagem = mensagem;
            return res;
        }

        public static ResultadoValidacaoPreImpressao Falha(string motivo, string mensagem = "Validação Falhou", string pedidoId = "", StatusValidacaoPreImpressao status = StatusValidacaoPreImpressao.Invalido)
        {
            return new ResultadoValidacaoPreImpressao
            {
                Status = status,
                MotivoBloqueio = motivo,
                Mensagem = mensagem,
                PedidoId = pedidoId
            };
        }
    }
}
