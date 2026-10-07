using System;
using System.Drawing;
using System.Threading.Tasks;
using Xunit;
using SistemaConferenciaPedidos;
using SistemaConferenciaPedidos.Repositories;
using SistemaConferenciaPedidos.Tests.Fakes;
using SistemaConferenciaPedidos.Models;
using SistemaConferenciaPedidos.Services;
using SistemaConferenciaPedidos.Tests.Infrastructure;

namespace SistemaConferenciaPedidos.Tests
{
    public class FrmPreparacaoPedidosTests : IDisposable
    {
        private readonly SqliteTestFixture _fixture;
        private readonly string _tempDbPath;

        public FrmPreparacaoPedidosTests()
        {
            _fixture = new SqliteTestFixture();
        }

        public void Dispose()
        {
            _fixture?.Dispose();
        }

        [Fact]
        public async Task ExecutarImpressaoSeguraAsync_10ChamadasF2Rapidas_ApenasUmaExecucao()
        {
            var configRepo = new SistemaConferenciaPedidos.Repositories.ConfiguracaoRepositorySqlite();
            configRepo.SalvarValor("OmieAppKey", "TEST_KEY");
            configRepo.SalvarValor("OmieAppSecret", "TEST_SECRET");

            var frm = new FrmPreparacaoPedidos();
            frm.Size = new System.Drawing.Size(1024, 768);
            var dummy = frm.Handle;

            int messageBoxCalls = 0;
            frm.ExibirMensagem = (msg, title, btns, icon) => { Console.WriteLine("MSG: " + msg); messageBoxCalls++; };

            // Bloqueador: mantém a primeira impressão travada até liberarmos explicitamente
            var bloqueio = new System.Threading.Tasks.TaskCompletionSource<bool>();
            var fakeImpressao = new FakeImpressaoServiceBloqueante(bloqueio.Task);
            var fakeValidador = new FakeValidacaoPreImpressaoServiceComSucesso();

            frm.ServicoImpressao = fakeImpressao;
            frm.ValidadorPreImpressao = fakeValidador;

            var repo = new PedidoRepositorySqlite();
            var pedido = new PedidoConferencia {
                NumeroPedidoCliente = "F2TEST",
                Marketplace = "Mercado Livre",
                CaminhoZipImportacao = "caminho.pdf",
                EtiquetaMarketplaceZpl = "PDF_MELI|1",
                Impresso = false
            };
            repo.SalvarOuAtualizar(pedido);
            pedido = repo.ObterPorNumero("F2TEST");

            // Dispara as 10 tasks simultaneamente antes de aguardar qualquer uma
            var tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                tasks[i] = Task.Run(() => frm.ExecutarImpressaoSeguraAsync(pedido, OrigemSolicitacaoImpressao.F2));
            }

            // Aguarda a primeira task começar a impressão (bloqueada no fake)
            await Task.Delay(50);

            // Libera o bloqueio — só a primeira deve executar
            bloqueio.SetResult(true);

            await Task.WhenAll(tasks);

            Assert.Equal(1, fakeValidador.Chamadas);
            Assert.Equal(1, fakeImpressao.Chamadas);
            Assert.Equal(0, messageBoxCalls);

            var atualizado = repo.ObterPorNumero("F2TEST");
            Assert.True(atualizado.Impresso);
        }

        [Fact]
        public async Task ExecutarImpressaoSeguraAsync_PedidoJaImpresso_NormalFlow_IgnoraSilenciosamente()
        {
            var configRepo = new SistemaConferenciaPedidos.Repositories.ConfiguracaoRepositorySqlite();
            configRepo.SalvarValor("OmieAppKey", "TEST_KEY");
            configRepo.SalvarValor("OmieAppSecret", "TEST_SECRET");

            var frm = new FrmPreparacaoPedidos();
            var dummy = frm.Handle;
            int messageBoxCalls = 0;
            frm.ExibirMensagem = (msg, title, btns, icon) => { Console.WriteLine("MSG: " + msg); messageBoxCalls++; };

            var fakeImpressao = new FakeImpressaoServiceComSucesso();
            var fakeValidador = new FakeValidacaoPreImpressaoServiceComSucesso();

            frm.ServicoImpressao = fakeImpressao;
            frm.ValidadorPreImpressao = fakeValidador;

            var repo = new PedidoRepositorySqlite();
            var pedido = new PedidoConferencia {
                NumeroPedidoCliente = "PRINTED",
                Impresso = true,
                CaminhoZipImportacao = "caminho.pdf",
                EtiquetaMarketplaceZpl = "PDF_MELI|1"
            };
            repo.SalvarOuAtualizar(pedido);
            pedido = repo.ObterPorNumero("PRINTED");

            await frm.ExecutarImpressaoSeguraAsync(pedido, OrigemSolicitacaoImpressao.F2);

            Assert.Equal(0, fakeValidador.Chamadas);
            Assert.Equal(0, messageBoxCalls);
        }

        [Fact]
        public async Task ExecutarImpressaoSeguraAsync_FalhaNaImpressao_ReverteOuMantemImpressoFalse()
        {
            var configRepo = new SistemaConferenciaPedidos.Repositories.ConfiguracaoRepositorySqlite();
            configRepo.SalvarValor("OmieAppKey", "TEST_KEY");
            configRepo.SalvarValor("OmieAppSecret", "TEST_SECRET");

            var frm = new FrmPreparacaoPedidos();
            var dummy = frm.Handle;
            int messageBoxCalls = 0;
            frm.ExibirMensagem = (msg, title, btns, icon) => { Console.WriteLine("MSG: " + msg); messageBoxCalls++; };

            var fakeImpressao = new FakeImpressaoServiceComFalha();
            var fakeValidador = new FakeValidacaoPreImpressaoServiceComSucesso();

            frm.ServicoImpressao = fakeImpressao;
            frm.ValidadorPreImpressao = fakeValidador;

            var repo = new PedidoRepositorySqlite();
            var pedido = new PedidoConferencia {
                NumeroPedidoCliente = "FAILTEST",
                Marketplace = "Mercado Livre",
                Impresso = false,
                CaminhoZipImportacao = "caminho.pdf",
                EtiquetaMarketplaceZpl = "PDF_MELI|1"
            };
            repo.SalvarOuAtualizar(pedido);
            pedido = repo.ObterPorNumero("FAILTEST");

            await frm.ExecutarImpressaoSeguraAsync(pedido, OrigemSolicitacaoImpressao.F2);

            Assert.Equal(1, fakeImpressao.Chamadas);

            var atualizado = repo.ObterPorNumero("FAILTEST");
            Assert.False(atualizado.Impresso);
            Assert.Equal(1, messageBoxCalls);
        }
    }

    internal class FakeImpressaoServiceComSucesso : ImpressaoService
    {
        public int Chamadas = 0;
        public FakeImpressaoServiceComSucesso() : base(new FakePrinterEnvironment(), new FakePdfRendererService()) { }
        public override ResultadoImpressao ImprimirPedido(PedidoConferencia pedido, string caminhoZip, string caminhoPdfMercadoLivre)
        {
            Chamadas++;
            return ResultadoImpressao.Ok("ZPL", "fake");
        }
    }

    internal class FakeImpressaoServiceComFalha : ImpressaoService
    {
        public int Chamadas = 0;
        public FakeImpressaoServiceComFalha() : base(new FakePrinterEnvironment(), new FakePdfRendererService()) { }
        public override ResultadoImpressao ImprimirPedido(PedidoConferencia pedido, string caminhoZip, string caminhoPdfMercadoLivre)
        {
            Chamadas++;
            return ResultadoImpressao.Falha(StatusResultadoImpressao.FalhaAoEnviar, "Fake fail");
        }
    }

    internal class FakeValidacaoPreImpressaoServiceComSucesso : IValidacaoPreImpressaoService
    {
        public int Chamadas = 0;
        public ResultadoValidacaoPreImpressao ValidarAntesDaImpressao(PedidoConferencia snapshotPedido, string caminhoZipShopee = null)
        {
            Chamadas++;
            return ResultadoValidacaoPreImpressao.Aprovado("PID", "F2TEST", "Mercado Livre", "COD", "HASH");
        }
    }

    /// <summary>
    /// Fake que bloqueia na <paramref name="bloqueio"/> task antes de retornar,
    /// permitindo verificar que apenas 1 chamada entra enquanto as outras 9 são
    /// recusadas pelo SemaphoreSlim.
    /// </summary>
    internal class FakeImpressaoServiceBloqueante : ImpressaoService
    {
        private readonly System.Threading.Tasks.Task _bloqueio;
        public int Chamadas = 0;

        public FakeImpressaoServiceBloqueante(System.Threading.Tasks.Task bloqueio)
            : base(new FakePrinterEnvironment(), new FakePdfRendererService())
        {
            _bloqueio = bloqueio;
        }

        public override ResultadoImpressao ImprimirPedido(
            PedidoConferencia pedido,
            string caminhoZip,
            string caminhoPdfMercadoLivre)
        {
            System.Threading.Interlocked.Increment(ref Chamadas);
            // Bloqueia a thread até o teste liberar o bloqueio
            _bloqueio.GetAwaiter().GetResult();
            return ResultadoImpressao.Ok("ZPL", "fake-bloqueante");
        }
    }
}
