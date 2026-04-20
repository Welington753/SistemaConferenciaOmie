using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SistemaConferenciaPedidos.Services
{
    public class OmieService
    {
        private readonly string _appKey;
        private readonly string _appSecret;
        private readonly string _url;

        public OmieService(string appKey, string appSecret, string url)
        {
            _appKey = appKey;
            _appSecret = appSecret;
            _url = url;
        }

        public async Task<string> ListarPedidosAsync(int pagina = 1, CancellationToken cancellationToken = default)
        {
            var body = new
            {
                call = "ListarPedidos",
                param = new object[]
                {
                    new
                    {
                        pagina = pagina,
                        registros_por_pagina = 100,
                        apenas_importado_api = "N"
                    }
                },
                app_key = _appKey,
                app_secret = _appSecret
            };

            return await PostAsync(body, cancellationToken);
        }

        public async Task<string> ConsultarProdutoAsync(string codigo = "", string codigoProduto = "", CancellationToken cancellationToken = default)
        {
            var body = new
            {
                call = "ConsultarProduto",
                param = new object[]
                {
                    new
                    {
                        codigo = codigo ?? "",
                        codigo_produto = codigoProduto ?? ""
                    }
                },
                app_key = _appKey,
                app_secret = _appSecret
            };

            return await PostAsync(body, cancellationToken);
        }

        private async Task<string> PostAsync(object body, CancellationToken cancellationToken = default)
        {
            var handler = new HttpClientHandler()
            {
                UseCookies = false
            };

            using (var client = new HttpClient(handler))
            {
                string json = JsonSerializer.Serialize(body);

                var request = new HttpRequestMessage(HttpMethod.Post, _url);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                request.Headers.Clear();
                request.Headers.Add("Accept", "application/json");
                request.Content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var response = await client.SendAsync(request, cancellationToken);
                var resposta = await response.Content.ReadAsStringAsync(cancellationToken);

                return resposta;
            }
        }
    }
}