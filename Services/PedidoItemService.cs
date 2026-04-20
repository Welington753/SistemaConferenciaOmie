using SistemaConferenciaPedidos.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SistemaConferenciaPedidos.Services
{
    public class PedidoItemService
    {
        public List<ItemPedidoGrid> MontarItensDoPedido(string jsonPedido)
        {
            var itens = new List<ItemPedidoGrid>();

            if (string.IsNullOrWhiteSpace(jsonPedido))
                return itens;

            using var json = JsonDocument.Parse(jsonPedido);
            var root = json.RootElement;

            if (!root.TryGetProperty("det", out var detNode))
                return itens;

            foreach (var itemNode in detNode.EnumerateArray())
            {
                string descricao = "";
                string sku = "";
                string quantidade = "";
                string ean = "";

                if (itemNode.TryGetProperty("produto", out var produtoNode))
                {
                    if (produtoNode.TryGetProperty("descricao", out var descricaoNode))
                        descricao = LerValorComoTexto(descricaoNode);

                    if (produtoNode.TryGetProperty("codigo", out var codigoNode))
                        sku = LerValorComoTexto(codigoNode);

                    if (produtoNode.TryGetProperty("quantidade", out var quantidadeNode))
                        quantidade = LerValorComoTexto(quantidadeNode);

                    ean = ExtrairEanDoProdutoJson(produtoNode);
                }

                itens.Add(new ItemPedidoGrid
                {
                    Sku = sku,
                    Produto = descricao,
                    Quantidade = quantidade,
                    Ean = ean
                });
            }

            return itens;
        }

        public List<ItemPedidoGrid> ObterItensComEanObrigatorio(string jsonPedido)
        {
            var itens = new List<ItemPedidoGrid>();

            if (string.IsNullOrWhiteSpace(jsonPedido))
                return itens;

            try
            {
                using var json = JsonDocument.Parse(jsonPedido);
                var root = json.RootElement;

                if (!root.TryGetProperty("det", out var detNode))
                    return itens;

                foreach (var itemNode in detNode.EnumerateArray())
                {
                    if (!itemNode.TryGetProperty("produto", out var produtoNode))
                        continue;

                    string descricao = "";
                    string sku = "";
                    string quantidade = "";
                    string ean = "";

                    if (produtoNode.TryGetProperty("descricao", out var descricaoNode))
                        descricao = LerValorComoTexto(descricaoNode);

                    if (produtoNode.TryGetProperty("codigo", out var codigoNode))
                        sku = LerValorComoTexto(codigoNode);

                    if (produtoNode.TryGetProperty("quantidade", out var quantidadeNode))
                        quantidade = LerValorComoTexto(quantidadeNode);

                    ean = ExtrairEanDoProdutoJson(produtoNode);

                    if (string.IsNullOrWhiteSpace(ean))
                        continue;

                    int qtd = ConverterQuantidadeInteira(quantidade);

                    for (int i = 0; i < qtd; i++)
                    {
                        itens.Add(new ItemPedidoGrid
                        {
                            Sku = sku,
                            Produto = descricao,
                            Quantidade = "1",
                            Ean = ean
                        });
                    }
                }
            }
            catch
            {
            }

            return itens;
        }

        public string ExtrairEanDoProdutoJson(JsonElement produtoNode)
        {
            string[] propriedadesPossiveis =
            {
                "ean",
                "EAN",
                "gtin",
                "GTIN",
                "ean_gtin",
                "codigo_barras",
                "codigo_barras_nfe",
                "cod_barras",
                "cEAN"
            };

            foreach (string nome in propriedadesPossiveis)
            {
                if (produtoNode.TryGetProperty(nome, out var prop))
                {
                    string valor = LerValorComoTexto(prop);
                    valor = NormalizarEan(valor);

                    if (!string.IsNullOrWhiteSpace(valor))
                        return valor;
                }
            }

            return "";
        }

        public string NormalizarEan(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return "";

            string numeros = Regex.Replace(valor.Trim(), @"\D", "");

            if (numeros.Length == 8 || numeros.Length == 12 || numeros.Length == 13 || numeros.Length == 14)
                return numeros;

            return "";
        }

        public int ConverterQuantidadeInteira(string quantidade)
        {
            if (string.IsNullOrWhiteSpace(quantidade))
                return 1;

            quantidade = quantidade.Trim().Replace(".", ",");

            if (decimal.TryParse(quantidade, out decimal qtdDecimal))
            {
                if (qtdDecimal <= 0)
                    return 1;

                return (int)Math.Round(qtdDecimal, MidpointRounding.AwayFromZero);
            }

            return 1;
        }

        public string LerValorComoTexto(JsonElement elemento)
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
    }
}