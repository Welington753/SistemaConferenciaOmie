using Xunit;
using SistemaConferenciaPedidos.Services;
using SistemaConferenciaPedidos.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SistemaConferenciaPedidos.Tests
{
    public class FakeOmiePedidoClient : IOmiePedidoClient
    {
        public bool ThrowsTimeout { get; set; } = false;
        public int TotalChamadas { get; private set; } = 0;

        public Task<string> ListarPedidosAsync(int pagina = 1, CancellationToken cancellationToken = default)
        {
            TotalChamadas++;
            if (ThrowsTimeout)
            {
                throw new TaskCanceledException("Timeout simulado da rede.");
            }
            return Task.FromResult("{\"total_de_paginas\": 0}");
        }

        public Task<string> ConsultarProdutoAsync(string codigo = "", string codigoProduto = "", CancellationToken cancellationToken = default)
        {
            TotalChamadas++;
            if (ThrowsTimeout)
            {
                throw new TaskCanceledException("Timeout simulado da rede.");
            }
            return Task.FromResult("{}");
        }
    }

    public class CredencialOmieTests : IDisposable
    {
        private readonly Infrastructure.SqliteTestFixture _fixture;
        private ConfiguracaoRepositorySqlite _config;

        public CredencialOmieTests()
        {
            _fixture = new Infrastructure.SqliteTestFixture();
            _config = new ConfiguracaoRepositorySqlite();
            // Limpa as credenciais para o teste
            _config.SalvarValor("OmieAppKey", "");
            _config.SalvarValor("OmieAppSecret", "");
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }

        [Fact]
        public void CredencialAusente_BloqueiaAntesDaRede()
        {
            var fakeOmie = new FakeOmiePedidoClient();
            var exception = Assert.Throws<InvalidOperationException>(() => new PedidoOmieService(fakeOmie));
            Assert.Equal("Credenciais Omie não configuradas.", exception.Message);
            Assert.Equal(0, fakeOmie.TotalChamadas);
        }

        [Fact]
        public void AppKeyAusente_Bloqueia()
        {
            _config.SalvarValor("OmieAppSecret", "secret");
            var fakeOmie = new FakeOmiePedidoClient();
            var exception = Assert.Throws<InvalidOperationException>(() => new PedidoOmieService(fakeOmie));
            Assert.Equal("Credenciais Omie não configuradas.", exception.Message);
            Assert.Equal(0, fakeOmie.TotalChamadas);
        }

        [Fact]
        public void AppSecretAusente_Bloqueia()
        {
            _config.SalvarValor("OmieAppKey", "key");
            var fakeOmie = new FakeOmiePedidoClient();
            var exception = Assert.Throws<InvalidOperationException>(() => new PedidoOmieService(fakeOmie));
            Assert.Equal("Credenciais Omie não configuradas.", exception.Message);
            Assert.Equal(0, fakeOmie.TotalChamadas);
        }

        [Fact]
        public void CredenciaisAusentes_NaoSaoPersistidasComoPadrao()
        {
            // O SqliteTestFixture cria um banco de dados novo a cada instância
            using var fixture = new Infrastructure.SqliteTestFixture();
            var configLocal = new ConfiguracaoRepositorySqlite();
            var key = configLocal.ObterValor("OmieAppKey");
            var secret = configLocal.ObterValor("OmieAppSecret");

            // Nenhuma credencial deve vir configurada no banco de dados limpo
            Assert.True(string.IsNullOrEmpty(key));
            Assert.True(string.IsNullOrEmpty(secret));

            var fakeOmie = new FakeOmiePedidoClient();
            var exception = Assert.Throws<InvalidOperationException>(() => new PedidoOmieService(fakeOmie));
            Assert.Equal("Credenciais Omie não configuradas.", exception.Message);
            Assert.Equal(0, fakeOmie.TotalChamadas);
        }
    }

    public class TimeoutTests : IDisposable
    {
        private readonly Infrastructure.SqliteTestFixture _fixture;

        public TimeoutTests()
        {
            _fixture = new Infrastructure.SqliteTestFixture();
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }

        [Fact]
        public async Task TimeoutSimulado_ETratadoCorretamente()
        {
            var config = new ConfiguracaoRepositorySqlite();
            config.SalvarValor("OmieAppKey", "fake_key");
            config.SalvarValor("OmieAppSecret", "fake_secret");

            var fakeClient = new FakeOmiePedidoClient { ThrowsTimeout = true };
            var service = new PedidoOmieService(fakeClient);

            await Assert.ThrowsAsync<TaskCanceledException>(() => service.BuscarPedidosAsync(DateTime.Today, DateTime.Today));
        }
    }
}


