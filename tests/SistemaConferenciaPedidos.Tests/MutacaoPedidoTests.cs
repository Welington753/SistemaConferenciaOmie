using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Services;
using SistemaConferenciaPedidos.Tests.Infrastructure;
using SistemaConferenciaPedidos.Repositories;

namespace SistemaConferenciaPedidos.Tests
{
    public class FakePdfServiceEtapa5 : MercadoLivrePdfService
    {
        public bool RetornarInvalido { get; set; }
        public override (List<EtiquetaMercadoLivrePdf> Etiquetas, string CaminhoGerenciado) CarregarEtiquetas(string caminhoPdf)
        {
            var list = new List<EtiquetaMercadoLivrePdf>();
            if (RetornarInvalido)
            {
                list.Add(new EtiquetaMercadoLivrePdf {
                    Pagina = 1,
                    NumeroVenda = "2000012345678901",
                    CodigoEtiqueta = "INVALIDO"
                });
            }
            else
            {
                list.Add(new EtiquetaMercadoLivrePdf {
                    Pagina = 1,
                    NumeroVenda = "2000012345678901",
                    CodigoEtiqueta = "47712345678"
                });
            }
            return (list, caminhoPdf);
        }
    }

    public class MutacaoPedidoTests : IDisposable
    {
        private readonly SqliteTestFixture _fixture;
        private readonly PedidoRepositorySqlite _repo;

        public MutacaoPedidoTests()
        {
            _fixture = new SqliteTestFixture();
            _repo = new PedidoRepositorySqlite();
            var configRepo = new ConfiguracaoRepositorySqlite();
            configRepo.SalvarValor("OmieAppKey", "TEST_KEY");
            configRepo.SalvarValor("OmieAppSecret", "TEST_SECRET");
        }

        public void Dispose() => _fixture?.Dispose();

        [Fact]
        public async Task TesteA_FalhaExtracao_Nao_Deve_Destruir_Dados_Anteriores()
        {
            var p = new PedidoConferencia
            {
                NumeroPedidoCliente = "2000012345678901",
                Marketplace = "MERCADO LIVRE",
                Status = "Aguardando",

                // Valores sentinela
                CodigoEtiqueta = "12345",
                CaminhoZipImportacao = "C:\\antes.pdf",
                PaginaPdf = 99,
                EtiquetaMarketplaceZpl = "PDF_MELI|99",
                NomePdfNoZip = "old.pdf"
            };
            _repo.SalvarOuAtualizar(p);

            using var frm = new FrmPreparacaoPedidos();
            frm.ExibirMensagem = (msg, title, btns, icon) => { };
            frm.ServicoPdfMercadoLivre = new FakePdfServiceEtapa5 { RetornarInvalido = true };

            await frm.ImportarPdfMercadoLivreAsync("C:\\fake.pdf");

            var pDb = _repo.ObterPorNumero("2000012345678901");
            Assert.Equal("12345", pDb.CodigoEtiqueta);
            Assert.Equal("C:\\antes.pdf", pDb.CaminhoZipImportacao);
            Assert.Equal(99, pDb.PaginaPdf);
            Assert.Equal("PDF_MELI|99", pDb.EtiquetaMarketplaceZpl);
            Assert.Equal("old.pdf", pDb.NomePdfNoZip);
        }

        [Fact]
        public async Task TesteB_SucessoExtracao_Atualiza_Todos_Os_Campos_Juntos()
        {
            var p = new PedidoConferencia
            {
                NumeroPedidoCliente = "2000012345678901",
                Marketplace = "MERCADO LIVRE",
                Status = "Aguardando"
            };
            _repo.SalvarOuAtualizar(p);

            using var frm = new FrmPreparacaoPedidos();
            frm.ExibirMensagem = (msg, title, btns, icon) => { };
            frm.ServicoPdfMercadoLivre = new FakePdfServiceEtapa5 { RetornarInvalido = false };

            await frm.ImportarPdfMercadoLivreAsync("C:\\fake.pdf");

            var pDb = _repo.ObterPorNumero("2000012345678901");
            Assert.Equal("47712345678", pDb.CodigoEtiqueta);
            Assert.Equal("C:\\fake.pdf", pDb.CaminhoZipImportacao);
            Assert.Equal(1, pDb.PaginaPdf);
            Assert.Equal("PDF_MELI|1", pDb.EtiquetaMarketplaceZpl);
            Assert.Equal("", pDb.NomePdfNoZip);
            Assert.Equal("Etiqueta vinculada", pDb.Status);
        }
    }
}
