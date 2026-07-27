using SistemaConferenciaPedidos.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SistemaConferenciaPedidos.Services
{
    public class PedidoOmieService
    {
        private readonly OmieService _omieService;
        internal Func<int, Task<string>> _omieServiceListarPedidosOverride;

        public PedidoOmieService()
        {
        }

        public async Task<ResultadoBuscaOmie> BuscarPedidosAsync(DateTime dataInicial, DateTime dataFinal, Action<string> onProgress = null, System.Threading.CancellationToken cancellationToken = default, ModoBuscaOmie modoBusca = ModoBuscaOmie.Rapida, int? paginaInicial = null)
        {
            var config = new Repositories.ConfiguracaoRepositorySqlite();
            string appKey = config.ObterValor("OmieAppKey", "")?.Trim();
            string appSecret = config.ObterValor("OmieAppSecret", "")?.Trim();

            if (string.IsNullOrWhiteSpace(appKey) || string.IsNullOrWhiteSpace(appSecret) ||
                appKey == "[REMOVIDO]" || appKey == "[REMOVIDO_PARA_AUDITORIA]" || appKey == "********" ||
                appSecret == "[REMOVIDO]" || appSecret == "[REMOVIDO_PARA_AUDITORIA]" || appSecret == "********")
            {
                throw new Exception("A integração Omie não está configurada. Informe a App Key e o App Secret na Administração.");
            }

            var omieServiceLocal = new OmieService(
                appKey,
                appSecret,
                "https://app.omie.com.br/api/v1/produtos/pedido/");

            var resultado = new ResultadoBuscaOmie();

            string json = await FetchPageWithRetryAsync(paginaInicial ?? 1, 0, 0, onProgress, cancellationToken, omieServiceLocal);
            using var primeiroJson = JsonDocument.Parse(json);
            var primeiroRoot = primeiroJson.RootElement;

            if (primeiroRoot.TryGetProperty("faultstring", out var faultInicial))
                throw new Exception("Erro da Omie: " + (faultInicial.GetString() ?? ""));

            if (!primeiroRoot.TryGetProperty("total_de_paginas", out var totalPaginasNode))
                throw new Exception("Não foi possível identificar o total de páginas.");

            int totalPaginas = totalPaginasNode.GetInt32();

            int paginaAtual = paginaInicial ?? totalPaginas;
            bool possuiPedidosAnterioresAData = false;

            for (int pagina = paginaAtual; pagina >= 1; pagina--)
            {
                if (modoBusca == ModoBuscaOmie.Rapida && (resultado.PaginasConsultadas >= 5 || resultado.PedidosBrutos >= 500))
                    break;

                resultado.UltimaPaginaConsultada = pagina;
                resultado.PaginasConsultadas++;

                string resposta = await FetchPageWithRetryAsync(pagina, totalPaginas, resultado.PedidosBrutos, onProgress, cancellationToken, omieServiceLocal);

                using var jsonDoc = JsonDocument.Parse(resposta);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("faultstring", out var faultNode))
                    throw new Exception($"Erro da Omie na página {pagina}: {faultNode.GetString()}");

                if (!root.TryGetProperty("pedido_venda_produto", out var pedidosNode))
                    continue;

                foreach (var pedidoNode in pedidosNode.EnumerateArray())
                {
                    resultado.PedidosBrutos++;

                    if (!pedidoNode.TryGetProperty("cabecalho", out var cabecalhoNode))
                        continue;

                    string numeroPedido = "";
                    string numeroPedidoCliente = "";
                    string nomeCliente = "";
                    string marketplace = "";
                    string etapaStr = "";
                    
                    if (cabecalhoNode.TryGetProperty("numero_pedido", out var numeroPedidoNode))
                        numeroPedido = LerValorComoTexto(numeroPedidoNode);

                    if (cabecalhoNode.TryGetProperty("etapa", out var etapaStrNode))
                        etapaStr = LerValorComoTexto(etapaStrNode);

                    if (cabecalhoNode.TryGetProperty("origem_pedido", out var origemNode))
                        marketplace = TraduzMarketplace(LerValorComoTexto(origemNode));

                    if (pedidoNode.TryGetProperty("informacoes_adicionais", out var infoNode))
                    {
                        if (infoNode.TryGetProperty("numero_pedido_cliente", out var numeroClienteNode))
                            numeroPedidoCliente = LerValorComoTexto(numeroClienteNode);

                        if (infoNode.TryGetProperty("contato", out var contatoNode))
                            nomeCliente = LerValorComoTexto(contatoNode);
                    }

                    string codigoPedidoClienteEfetivo = string.IsNullOrWhiteSpace(numeroPedidoCliente) ? numeroPedido : numeroPedidoCliente;

                    string dataPrevisao = "";
                    if (cabecalhoNode.TryGetProperty("data_previsao", out var dataPrevisaoNode))
                        dataPrevisao = LerValorComoTexto(dataPrevisaoNode);

                    DateTime? dataPedido = null;
                    if (DateTime.TryParse(dataPrevisao, out DateTime dataConvertida))
                        dataPedido = dataConvertida.Date;
                        
                    string motivoExclusao = "";
                    bool valido = true;

                    int etapa = LerInteiro(etapaStrNode);

                    if (etapa != 60)
                    {
                        motivoExclusao = $"Etapa diferente da esperada (Esperado: 60, Atual: {etapaStr})";
                        valido = false;
                    }
                    else if (!dataPedido.HasValue)
                    {
                        motivoExclusao = $"DataPrevisao nula ou inválida ({dataPrevisao})";
                        valido = false;
                    }
                    else if (dataPedido.Value < dataInicial)
                    {
                        possuiPedidosAnterioresAData = true;
                        motivoExclusao = $"Data fora do período (Previsão: {dataPedido.Value:dd/MM/yyyy}, Período: {dataInicial:dd/MM/yyyy} - {dataFinal:dd/MM/yyyy})";
                        valido = false;
                    }
                    else if (dataPedido.Value > dataFinal)
                    {
                        motivoExclusao = $"Data fora do período (Previsão: {dataPedido.Value:dd/MM/yyyy}, Período: {dataInicial:dd/MM/yyyy} - {dataFinal:dd/MM/yyyy})";
                        valido = false;
                    }

                    if (!valido)
                    {
                        resultado.Descartados.Add(new PedidoDescartadoOmie
                        {
                            NumeroPedidoCliente = codigoPedidoClienteEfetivo,
                            CodigoPedidoOmie = numeroPedido,
                            DataPrevisao = dataPrevisao,
                            Etapa = etapaStr,
                            Status = "Descartado",
                            Origem = LerValorComoTexto(origemNode),
                            MarketplaceDetectado = marketplace,
                            MotivoExclusao = motivoExclusao,
                            PaginaDescartado = pagina
                        });
                        continue;
                    }

                    var pedido = new PedidoConferencia
                    {
                        CodigoEtiqueta = "",
                        NumeroPedidoCliente = codigoPedidoClienteEfetivo,
                        NomeCliente = nomeCliente,
                        Marketplace = marketplace,
                        JsonItens = pedidoNode.ToString(),
                        EtiquetaMarketplaceZpl = "",
                        Status = "Importado",
                        DataPrevisao = dataPedido
                    };

                    resultado.PedidosValidos.Add(pedido);
                }
            }

            if (modoBusca == ModoBuscaOmie.Rapida && !possuiPedidosAnterioresAData && resultado.UltimaPaginaConsultada > 1)
            {
                resultado.LimiteAtingido = true;
            }

            return resultado;
        }

        private async Task<string> FetchPageWithRetryAsync(int pagina, int totalPaginas, int pedidosAnalisados, Action<string> onProgress, System.Threading.CancellationToken cancellationToken, OmieService omieServiceLocal)
        {
            int tentativas = 0;
            while (tentativas < 3)
            {
                tentativas++;
                try
                {
                    if (onProgress != null)
                    {
                        if (totalPaginas == 0)
                            onProgress($"Consultando página {pagina} — tentativa {tentativas}");
                        else
                            onProgress($"Consultando página {pagina} de {totalPaginas} — {pedidosAnalisados}/500 pedidos analisados");
                    }
                    
                    string resposta;
                    if (_omieServiceListarPedidosOverride != null)
                        resposta = await _omieServiceListarPedidosOverride(pagina);
                    else
                        resposta = await omieServiceLocal.ListarPedidosAsync(pagina, cancellationToken);

                    using var json = JsonDocument.Parse(resposta);
                    if (json.RootElement.TryGetProperty("faultstring", out var faultNode))
                    {
                        string erro = faultNode.GetString() ?? "";
                        if (erro.Contains("Já existe uma requisição desse método sendo executada"))
                        {
                            if (tentativas >= 3)
                                throw new Exception($"A Omie não respondeu na página {pagina} após 3 tentativas.");

                            int delaySeconds = tentativas == 1 ? 30 : 60;
                            if (onProgress != null)
                                onProgress($"A Omie ainda está processando a página {pagina}. Nova tentativa em {delaySeconds} segundos...");
                            
                            await Task.Delay(delaySeconds * 1000);
                            continue;
                        }
                    }

                    return resposta;
                }
                catch (TaskCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw;

                    if (tentativas >= 3)
                        throw new Exception($"A Omie não respondeu na página {pagina} após 3 tentativas.");

                    int delaySeconds = 30;
                    if (onProgress != null)
                        onProgress($"A Omie ainda está processando a página {pagina}. Nova tentativa em {delaySeconds} segundos...");
                        
                    await Task.Delay(delaySeconds * 1000);
                }
            }
            return null;
        }

        private string LerValorComoTexto(JsonElement elemento)
        {
            switch (elemento.ValueKind)
            {
                case JsonValueKind.String:
                    return elemento.GetString() ?? "";
                case JsonValueKind.Number:
                    return elemento.ToString();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return elemento.GetBoolean().ToString();
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return "";
                default:
                    return elemento.ToString();
            }
        }

        private int LerInteiro(JsonElement elemento)
        {
            switch (elemento.ValueKind)
            {
                case JsonValueKind.Number:
                    return elemento.GetInt32();
                case JsonValueKind.String:
                    if (int.TryParse(elemento.GetString(), out int valor))
                        return valor;
                    return 0;
                default:
                    return 0;
            }
        }

        private string TraduzMarketplace(string codigo)
        {
            switch ((codigo ?? "").Trim().ToUpperInvariant())
            {
                case "SHP":
                    return "Shopee";
                case "AMZ":
                    return "Amazon";
                case "MLV":
                case "MLB":
                case "MELI":
                    return "Mercado Livre";
                default:
                    return codigo;
            }
        }
    }
}