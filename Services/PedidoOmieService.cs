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

        public PedidoOmieService()
        {
            _omieService = new OmieService(
                "3967066699596",
                "85e0d23541afa63e481e20c67e7a289d",
                "https://app.omie.com.br/api/v1/produtos/pedido/");
        }

        public async Task<List<PedidoConferencia>> BuscarPedidosAsync(DateTime dataInicial, DateTime dataFinal)
        {
            var pedidosImportados = new List<PedidoConferencia>();

            string primeiraResposta = await _omieService.ListarPedidosAsync(1);
            using var primeiroJson = JsonDocument.Parse(primeiraResposta);
            var primeiroRoot = primeiroJson.RootElement;

            if (primeiroRoot.TryGetProperty("faultstring", out var faultInicial))
                throw new Exception("Erro da Omie: " + (faultInicial.GetString() ?? ""));

            if (!primeiroRoot.TryGetProperty("total_de_paginas", out var totalPaginasNode))
                throw new Exception("Não foi possível identificar o total de páginas.");

            int totalPaginas = totalPaginasNode.GetInt32();

            for (int pagina = totalPaginas; pagina >= 1; pagina--)
            {
                string resposta = await _omieService.ListarPedidosAsync(pagina);

                using var json = JsonDocument.Parse(resposta);
                var root = json.RootElement;

                if (root.TryGetProperty("faultstring", out var faultNode))
                    throw new Exception($"Erro da Omie na página {pagina}: {faultNode.GetString()}");

                if (!root.TryGetProperty("pedido_venda_produto", out var pedidosNode))
                    continue;

                bool encontrouPedidoNoPeriodo = false;
                bool paginaSoComDatasAntigas = true;

                foreach (var pedidoNode in pedidosNode.EnumerateArray())
                {
                    if (!pedidoNode.TryGetProperty("cabecalho", out var cabecalhoNode))
                        continue;

                    string dataPrevisao = "";
                    if (cabecalhoNode.TryGetProperty("data_previsao", out var dataPrevisaoNode))
                        dataPrevisao = LerValorComoTexto(dataPrevisaoNode);

                    DateTime? dataPedido = null;
                    if (DateTime.TryParse(dataPrevisao, out DateTime dataConvertida))
                        dataPedido = dataConvertida.Date;

                    if (dataPedido.HasValue)
                    {
                        if (dataPedido.Value >= dataInicial && dataPedido.Value <= dataFinal)
                        {
                            encontrouPedidoNoPeriodo = true;
                            paginaSoComDatasAntigas = false;
                        }
                        else if (dataPedido.Value > dataFinal)
                        {
                            paginaSoComDatasAntigas = false;
                        }
                    }

                    if (!cabecalhoNode.TryGetProperty("etapa", out var etapaNode))
                        continue;

                    int etapa = LerInteiro(etapaNode);
                    if (etapa != 60)
                        continue;

                    if (!dataPedido.HasValue || dataPedido.Value < dataInicial || dataPedido.Value > dataFinal)
                        continue;

                    string numeroPedido = "";
                    string numeroPedidoCliente = "";
                    string nomeCliente = "";
                    string marketplace = "";

                    if (cabecalhoNode.TryGetProperty("numero_pedido", out var numeroPedidoNode))
                        numeroPedido = LerValorComoTexto(numeroPedidoNode);

                    if (cabecalhoNode.TryGetProperty("origem_pedido", out var origemNode))
                        marketplace = TraduzMarketplace(LerValorComoTexto(origemNode));

                    if (pedidoNode.TryGetProperty("informacoes_adicionais", out var infoNode))
                    {
                        if (infoNode.TryGetProperty("numero_pedido_cliente", out var numeroClienteNode))
                            numeroPedidoCliente = LerValorComoTexto(numeroClienteNode);

                        if (infoNode.TryGetProperty("contato", out var contatoNode))
                            nomeCliente = LerValorComoTexto(contatoNode);
                    }

                    var pedido = new PedidoConferencia
                    {
                        CodigoEtiqueta = "",
                        NumeroPedidoCliente = string.IsNullOrWhiteSpace(numeroPedidoCliente) ? numeroPedido : numeroPedidoCliente,
                        NomeCliente = nomeCliente,
                        Marketplace = marketplace,
                        JsonItens = pedidoNode.ToString(),
                        EtiquetaMarketplaceZpl = "",
                        Status = "Importado"
                    };

                    pedidosImportados.Add(pedido);
                }

                if (!encontrouPedidoNoPeriodo && paginaSoComDatasAntigas && pedidosImportados.Count > 0)
                    break;
            }

            return pedidosImportados;
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