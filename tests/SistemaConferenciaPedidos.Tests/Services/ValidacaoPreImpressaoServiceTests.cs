using System;
using System.IO;
using Xunit;
using SistemaConferenciaPedidos.Services;
using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Repositories;
using System.Collections.Generic;
using SistemaConferenciaPedidos.Tests.Infrastructure;

namespace SistemaConferenciaPedidos.Tests.Services
{
    public class ValidacaoPreImpressaoServiceTests : IDisposable
    {
        private readonly SqliteTestFixture _fixture;
        private readonly PedidoRepositorySqlite _repository;

        public ValidacaoPreImpressaoServiceTests()
        {
            _fixture = new SqliteTestFixture();
            _repository = new PedidoRepositorySqlite();
            File.WriteAllText("dummy.pdf", "dummy");
        }

        public void Dispose()
        {
            if (File.Exists("dummy.pdf")) File.Delete("dummy.pdf");
            _fixture?.Dispose();
        }

        private PedidoConferencia CriarPedidoMercadoLivreParaValidacao(string codigoEtiqueta, string zplData, string caminhoZip)
        {
            var numeroPedido = "PED-" + Guid.NewGuid().ToString().Substring(0, 8);
            var pedido = new PedidoConferencia
            {
                NumeroPedidoCliente = numeroPedido,
                Marketplace = "Mercado Livre",
                CodigoEtiqueta = codigoEtiqueta,
                EtiquetaMarketplaceZpl = zplData,
                CaminhoZipImportacao = caminhoZip,
                DataCriacao = DateTime.Now,
                Status = "Pendente"
            };
            _repository.SalvarOuAtualizar(pedido);
            return pedido;
        }

        [Fact]
        public void ValidarAntesDaImpressao_MercadoLivre_CodigoValido_Permite()
        {
            var pdfProviderFake = new FakePdfTextProvider("Texto com rastreio Despachar 47519084004 do mercado livre");
            var service = new ValidacaoPreImpressaoService(_repository, null, pdfProviderFake);

            var pedido = CriarPedidoMercadoLivreParaValidacao("47519084004", "PDF_MELI|1", "dummy.pdf");
            Assert.NotNull(_repository.ObterPorNumero(pedido.NumeroPedidoCliente));

            var resultado = service.ValidarAntesDaImpressao(pedido);
            Assert.True(resultado.Valido);
        }

        [Fact]
        public void ValidarAntesDaImpressao_MercadoLivre_RastreioFragmentado_Permite()
        {
            var pdfProviderFake = new FakePdfTextProvider("Despachar 4 7 5 1 9 0 8 4 0 0 4 ok");
            var service = new ValidacaoPreImpressaoService(_repository, null, pdfProviderFake);

            var pedido = CriarPedidoMercadoLivreParaValidacao("47519084004", "PDF_MELI|1", "dummy.pdf");
            Assert.NotNull(_repository.ObterPorNumero(pedido.NumeroPedidoCliente));

            var resultado = service.ValidarAntesDaImpressao(pedido);
            Assert.True(resultado.Valido);
        }

        [Fact]
        public void ValidarAntesDaImpressao_MercadoLivre_RastreioDiferente_Bloqueia()
        {
            var pdfProviderFake = new FakePdfTextProvider("Despachar 47111222333");
            var service = new ValidacaoPreImpressaoService(_repository, null, pdfProviderFake);

            var pedido = CriarPedidoMercadoLivreParaValidacao("47519084004", "PDF_MELI|1", "dummy.pdf");
            Assert.NotNull(_repository.ObterPorNumero(pedido.NumeroPedidoCliente));

            var resultado = service.ValidarAntesDaImpressao(pedido);
            Assert.False(resultado.Valido);
            Assert.Contains("O código da etiqueta na página", resultado.MotivoBloqueio);
        }

        [Fact]
        public void ValidarAntesDaImpressao_MercadoLivre_NenhumRastreio_Bloqueia()
        {
            var pdfProviderFake = new FakePdfTextProvider("Sem rastreio nesta pagina");
            var service = new ValidacaoPreImpressaoService(_repository, null, pdfProviderFake);

            var pedido = CriarPedidoMercadoLivreParaValidacao("47519084004", "PDF_MELI|1", "dummy.pdf");
            Assert.NotNull(_repository.ObterPorNumero(pedido.NumeroPedidoCliente));

            var resultado = service.ValidarAntesDaImpressao(pedido);
            Assert.False(resultado.Valido);
            Assert.Contains("Código de rastreio não encontrado", resultado.MotivoBloqueio);
        }

        [Fact]
        public void ValidarAntesDaImpressao_MercadoLivre_DozeDigitos_Bloqueia()
        {
            var pdfProviderFake = new FakePdfTextProvider("Despachar 475190840040");
            var service = new ValidacaoPreImpressaoService(_repository, null, pdfProviderFake);

            var pedido = CriarPedidoMercadoLivreParaValidacao("47519084004", "PDF_MELI|1", "dummy.pdf");
            Assert.NotNull(_repository.ObterPorNumero(pedido.NumeroPedidoCliente));

            var resultado = service.ValidarAntesDaImpressao(pedido);
            Assert.False(resultado.Valido);
            Assert.Contains("Código de rastreio não encontrado", resultado.MotivoBloqueio);
        }

        [Fact]
        public void ValidarAntesDaImpressao_MercadoLivre_DoisRastreios_BloqueiaPorAmbiguidade()
        {
            var pdfProviderFake = new FakePdfTextProvider("47519084004 e 47123456789 ambiguidade");
            var service = new ValidacaoPreImpressaoService(_repository, null, pdfProviderFake);

            var pedido = CriarPedidoMercadoLivreParaValidacao("47519084004", "PDF_MELI|1", "dummy.pdf");
            Assert.NotNull(_repository.ObterPorNumero(pedido.NumeroPedidoCliente));

            var resultado = service.ValidarAntesDaImpressao(pedido);
            Assert.False(resultado.Valido);
        }

        [Fact]
        public void ValidarAntesDaImpressao_MercadoLivre_MesmoRastreioRepetido_Permite()
        {
            var pdfProviderFake = new FakePdfTextProvider("47519084004 repete na 47519084004");
            var service = new ValidacaoPreImpressaoService(_repository, null, pdfProviderFake);

            var pedido = CriarPedidoMercadoLivreParaValidacao("47519084004", "PDF_MELI|1", "dummy.pdf");
            Assert.NotNull(_repository.ObterPorNumero(pedido.NumeroPedidoCliente));

            var resultado = service.ValidarAntesDaImpressao(pedido);
            Assert.True(resultado.Valido);
        }

        [Fact]
        public void ValidarAntesDaImpressao_MercadoLivre_ArquivoInexistente_Bloqueia()
        {
            var pdfProviderFake = new FakePdfTextProvider("texto");
            var service = new ValidacaoPreImpressaoService(_repository, null, pdfProviderFake);

            var pedido = CriarPedidoMercadoLivreParaValidacao("47519084004", "PDF_MELI|1", null);
            Assert.NotNull(_repository.ObterPorNumero(pedido.NumeroPedidoCliente));

            var resultado = service.ValidarAntesDaImpressao(pedido);
            Assert.False(resultado.Valido);
            Assert.Contains("O arquivo PDF original não foi encontrado", resultado.MotivoBloqueio);
        }

        [Fact]
        public void ValidarAntesDaImpressao_MercadoLivre_PaginaForaDoIntervalo_Bloqueia()
        {
            var pdfProviderFake = new FakePdfTextProvider(null);
            var service = new ValidacaoPreImpressaoService(_repository, null, pdfProviderFake);

            var pedido = CriarPedidoMercadoLivreParaValidacao("47519084004", "PDF_MELI|10", "dummy.pdf");
            Assert.NotNull(_repository.ObterPorNumero(pedido.NumeroPedidoCliente));

            var resultado = service.ValidarAntesDaImpressao(pedido);
            Assert.False(resultado.Valido);
            Assert.Contains("não existe no PDF", resultado.MotivoBloqueio);
        }

        [Fact]
        public void ValidarAntesDaImpressao_MercadoLivre_RastreioFragmentadoComHorario_Permite()
        {
            // Simula exatamente o texto da página 1 do PDF real:
            // rastreio fragmentado com espaços + horário na linha seguinte
            var pdfProviderFake = new FakePdfTextProvider("Despachar 477987   92847\r\n02:30");
            var service = new ValidacaoPreImpressaoService(_repository, null, pdfProviderFake);

            var pedido = CriarPedidoMercadoLivreParaValidacao("47798792847", "PDF_MELI|1", "dummy.pdf");
            Assert.NotNull(_repository.ObterPorNumero(pedido.NumeroPedidoCliente));

            var resultado = service.ValidarAntesDaImpressao(pedido);
            Assert.True(resultado.Valido, $"Deveria ser valido. Motivo: {resultado.MotivoBloqueio}");
        }

        [Fact]
        public void ValidarAntesDaImpressao_MercadoLivre_CodigoIgualNaPagina_Permite()
        {
            // Código igual ao do pedido: deve permitir
            var pdfProviderFake = new FakePdfTextProvider("Despachar 47798792847");
            var service = new ValidacaoPreImpressaoService(_repository, null, pdfProviderFake);

            var pedido = CriarPedidoMercadoLivreParaValidacao("47798792847", "PDF_MELI|1", "dummy.pdf");
            var resultado = service.ValidarAntesDaImpressao(pedido);
            Assert.True(resultado.Valido, $"Deveria ser valido. Motivo: {resultado.MotivoBloqueio}");
        }

        [Fact]
        public void ValidarAntesDaImpressao_MercadoLivre_CodigoDiferenteNaPagina_Bloqueia()
        {
            // Código diferente do pedido: deve bloquear
            var pdfProviderFake = new FakePdfTextProvider("Despachar 47999999999");
            var service = new ValidacaoPreImpressaoService(_repository, null, pdfProviderFake);

            var pedido = CriarPedidoMercadoLivreParaValidacao("47798792847", "PDF_MELI|1", "dummy.pdf");
            var resultado = service.ValidarAntesDaImpressao(pedido);
            Assert.False(resultado.Valido);
            Assert.Contains("não corresponde", resultado.MotivoBloqueio, StringComparison.OrdinalIgnoreCase);
        }
    }

    internal class FakePdfTextProvider : IPdfTextProvider
    {
        private readonly string _texto;
        public FakePdfTextProvider(string texto) => _texto = texto;
        public string ExtrairTexto(string caminhoPdf, int pagina) => _texto;
    }
}
