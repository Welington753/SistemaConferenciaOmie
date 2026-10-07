using Xunit;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using SistemaConferenciaPedidos.Data;
using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Repositories;
using SistemaConferenciaPedidos.Services;
using System.Collections.Generic;

namespace SistemaConferenciaPedidos.Tests
{
    public class VinculacaoReimportacaoTests : IDisposable
    {
        private readonly string _tempDbPath;

        public VinculacaoReimportacaoTests()
        {
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"SistemaConferenciaPedidos_Test_{Guid.NewGuid()}.db");
            Database.CaminhoBanco = _tempDbPath;
            Database.Inicializar();

            var configRepo = new SistemaConferenciaPedidos.Repositories.ConfiguracaoRepositorySqlite();
            configRepo.SalvarValor("OmieAppKey", "TEST_KEY");
            configRepo.SalvarValor("OmieAppSecret", "TEST_SECRET");
        }

        public void Dispose()
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(_tempDbPath))
            {
                try
                {
                    File.Delete(_tempDbPath);
                }
                catch { }
            }
        }

        private class FakeMercadoLivrePdfService : MercadoLivrePdfService
        {
            public override (List<EtiquetaMercadoLivrePdf> Etiquetas, string CaminhoGerenciado) CarregarEtiquetas(string caminhoPdf)
            {
                var etiquetas = new List<EtiquetaMercadoLivrePdf>
                {
                    new EtiquetaMercadoLivrePdf { CodigoEtiqueta = "47798792847", NumeroVenda = "2000000000000001", Pagina = 1, PackId = "" }
                };
                return (etiquetas, "fake_managed_path.pdf");
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task Importar_Mesmo_PDF_Duas_Vezes_E_Idempotente()
        {
            var repo = new PedidoRepositorySqlite();
            repo.SalvarOuAtualizar(new PedidoConferencia {
                NumeroPedidoCliente = "2000000000000001",
                Marketplace = "Mercado Livre",
                Status = "Pendente"
            });

            var frm = new FrmPreparacaoPedidos();
            frm.ServicoPdfMercadoLivre = new FakeMercadoLivrePdfService();
            frm.ExibirMensagem = (msg, t, b, i) => { };

            await frm.ImportarPdfMercadoLivreAsync("dummy.pdf");

            var pedidoAtualizado = repo.ObterPorNumero("2000000000000001");
            Assert.Equal("Etiqueta vinculada", pedidoAtualizado.Status);
            Assert.Equal("PDF_MELI|1", pedidoAtualizado.EtiquetaMarketplaceZpl);
            Assert.Equal(1, pedidoAtualizado.PaginaPdf);

            // Re-importing should not fail or duplicate
            await frm.ImportarPdfMercadoLivreAsync("dummy.pdf");

            var pedidoReimportado = repo.ObterPorNumero("2000000000000001");
            Assert.Equal("Etiqueta vinculada", pedidoReimportado.Status);
        }

        [Fact]
        public async System.Threading.Tasks.Task Importar_VendaEPackConflitando_Bloqueia()
        {
            var repo = new PedidoRepositorySqlite();
            repo.SalvarOuAtualizar(new PedidoConferencia {
                NumeroPedidoCliente = "2000000000000001",
                Marketplace = "Mercado Livre",
                Status = "Pendente"
            });
            repo.SalvarOuAtualizar(new PedidoConferencia {
                NumeroPedidoCliente = "2000000000000002", // This normally wouldn't happen, but simulates conflict
                Marketplace = "Mercado Livre",
                Status = "Pendente"
            });

            var frm = new FrmPreparacaoPedidos();
            frm.ServicoPdfMercadoLivre = new FakeMercadoLivrePdfServiceConflict();
            frm.ExibirMensagem = (msg, t, b, i) => { };

            await frm.ImportarPdfMercadoLivreAsync("dummy2.pdf");

            var p1 = repo.ObterPorNumero("2000000000000001");
            var p2 = repo.ObterPorNumero("2000000000000002");

            // Assuming Frm doesn't process PACK if it's text-based here, wait, Frm matches by order number exactly.
            // If one order matches Venda and the other matches PackId on the SAME page, they conflict.
            Assert.Equal("PDF ambíguo", p2.Status); // The second one processed will be blocked
        }

        private class FakeMercadoLivrePdfServiceConflict : MercadoLivrePdfService
        {
            public override (List<EtiquetaMercadoLivrePdf> Etiquetas, string CaminhoGerenciado) CarregarEtiquetas(string caminhoPdf)
            {
                var etiquetas = new List<EtiquetaMercadoLivrePdf>
                {
                    new EtiquetaMercadoLivrePdf { CodigoEtiqueta = "47798792847", NumeroVenda = "2000000000000001", Pagina = 1, PackId = "2000000000000002" }
                };
                return (etiquetas, "fake_managed_path2.pdf");
            }
        }
    }
}
