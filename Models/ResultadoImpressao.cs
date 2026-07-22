using System;

namespace SistemaConferenciaPedidos.Models
{
    public enum StatusResultadoImpressao
    {
        ValidacaoFalhou,
        ImpressoraNaoEncontrada,
        ArquivoAusente,
        ConteudoInvalido,
        FalhaAoPreparar,
        FalhaAoEnviar,
        EnviadoParaFila,
        EstadoDesconhecido
    }

    public class ResultadoImpressao
    {
        public bool Sucesso { get; set; }
        public StatusResultadoImpressao Status { get; set; }
        public string Mensagem { get; set; } = "";
        public Exception? ErroTecnico { get; set; }
        public string Impressora { get; set; } = "";
        public string TipoEtiqueta { get; set; } = "";
        public bool PodeTentarNovamente { get; set; }

        public static ResultadoImpressao Falha(StatusResultadoImpressao status, string mensagem, Exception erro = null, bool tentarNovamente = true)
        {
            return new ResultadoImpressao
            {
                Sucesso = false,
                Status = status,
                Mensagem = mensagem,
                ErroTecnico = erro,
                PodeTentarNovamente = tentarNovamente
            };
        }

        public static ResultadoImpressao Desconhecido(string mensagem, Exception erro = null)
        {
            return new ResultadoImpressao
            {
                Sucesso = false,
                Status = StatusResultadoImpressao.EstadoDesconhecido,
                Mensagem = mensagem,
                ErroTecnico = erro,
                PodeTentarNovamente = false
            };
        }

        public static ResultadoImpressao Ok(string tipoEtiqueta, string impressora = null)
        {
            return new ResultadoImpressao
            {
                Sucesso = true,
                Status = StatusResultadoImpressao.EnviadoParaFila,
                Mensagem = "Trabalho aceito pelo mecanismo de impressão.",
                TipoEtiqueta = tipoEtiqueta,
                Impressora = impressora,
                PodeTentarNovamente = false
            };
        }
    }
}
