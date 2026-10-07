using Xunit;
using SistemaConferenciaPedidos.Services;
using SistemaConferenciaPedidos.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System;
using System.Reflection;

namespace SistemaConferenciaPedidos.Tests
{
    public class ValidacaoEanTests
    {
        private object CriarItemAnonymous(string produto, string sku, string ean)
        {
            return new { Produto = produto, Sku = sku, Ean = ean };
        }

        private FrmValidacaoEan CriarForm(params object[] itens)
        {
            var form = new FrmValidacaoEan(itens, "123", "Cliente Teste");
            
            return form;
        }

        private void SimularLeitura(FrmValidacaoEan form, string codigo)
        {
            var txt = (TextBox)form.GetType().GetField("txtLeitura", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
            txt.Text = codigo;
            var method = form.GetType().GetMethod("ProcessarLeitura", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(form, null);
        }

        private List<object> ObterItensDoForm(FrmValidacaoEan form)
        {
            var field = form.GetType().GetField("_itens", BindingFlags.NonPublic | BindingFlags.Instance);
            var itens = field.GetValue(form) as System.Collections.IEnumerable;
            var result = new List<object>();
            foreach (var i in itens) result.Add(i);
            return result;
        }

        private bool IsConferido(object item)
        {
            return (bool)item.GetType().GetProperty("Conferido").GetValue(item);
        }

        [Fact]
        public void LeituraCorreta_ConfereApenasUmaUnidade()
        {
            var form = CriarForm(
                CriarItemAnonymous("Prod 1", "SKU1", "12345"),
                CriarItemAnonymous("Prod 1", "SKU1", "12345")
            );

            SimularLeitura(form, "12345");

            var itens = ObterItensDoForm(form);
            Assert.True(IsConferido(itens[0]));
            Assert.False(IsConferido(itens[1]));
        }

        [Fact]
        public void QuantidadeDois_ExigeDuasLeituras()
        {
            var form = CriarForm(
                CriarItemAnonymous("Prod 1", "SKU1", "12345"),
                CriarItemAnonymous("Prod 1", "SKU1", "12345")
            );

            SimularLeitura(form, "12345");
            SimularLeitura(form, "12345");

            var itens = ObterItensDoForm(form);
            Assert.True(IsConferido(itens[0]));
            Assert.True(IsConferido(itens[1]));
        }

        [Fact]
        public void EanIncorreto_NaoConfere()
        {
            var form = CriarForm(CriarItemAnonymous("Prod 1", "SKU1", "12345"));
            SimularLeitura(form, "99999");
            
            var itens = ObterItensDoForm(form);
            Assert.False(IsConferido(itens[0]));
        }

        [Fact]
        public void CodigoUniversalAntigo_NaoLiberaTodos()
        {
            var form = CriarForm(CriarItemAnonymous("Prod 1", "SKU1", "12345"));
            
            var avisos = new List<(string Mensagem, string Titulo)>();
            form.GetType().GetProperty("ActionMostrarAviso", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Action<string, string>((m, t) => { avisos.Add((m, t)); }));

            SimularLeitura(form, "0051000012517");
            
            var itens = ObterItensDoForm(form);
            Assert.Empty(avisos);
            Assert.False(IsConferido(itens[0]));
        }

        [Fact]
        public void LiberacaoManual_SemSenhaValida_PermaneceBloqueada()
        {
            var form = CriarForm(CriarItemAnonymous("Prod 1", "SKU1", "12345"), CriarItemAnonymous("Prod 2", "SKU2", "999"));
            
            var avisos = new List<(string Mensagem, string Titulo)>();
            form.GetType().GetProperty("ActionMostrarAviso", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Action<string, string>((m, t) => { avisos.Add((m, t)); }));

            int authRequests = 0;
            form.GetType().GetProperty("FuncSolicitarAutorizacao", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Func<IWin32Window, bool>(w => { 
                authRequests++; 
                var method = form.GetType().GetProperty("ActionMostrarAviso", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form) as Action<string, string>;
                method?.Invoke("Senha inválida.", "Aviso");
                return false; 
            }));

            form.GetType().GetProperty("FuncSolicitarMotivo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Func<IWin32Window, string>(w => { throw new Xunit.Sdk.XunitException("O motivo não deveria ser solicitado após senha inválida."); }));

            var lst = (ListBox)form.GetType().GetField("lstPendentes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
            lst.SelectedIndex = 0;

            var btn = (Button)form.GetType().GetField("btnLiberacaoManual", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
            var clickMethod = form.GetType().GetMethod("BtnLiberacaoManual_Click", BindingFlags.NonPublic | BindingFlags.Instance);
            clickMethod.Invoke(form, new object[] { btn, EventArgs.Empty });

            var itens = ObterItensDoForm(form);
            Assert.Equal(1, authRequests);
            Assert.Single(avisos);
            Assert.Contains("senha", avisos[0].Mensagem, StringComparison.OrdinalIgnoreCase); 
            Assert.False(IsConferido(itens[0]));
            Assert.False(IsConferido(itens[1]));
            Assert.NotEqual(DialogResult.OK, form.DialogResult);
        }

        [Fact]
        public void LiberacaoManual_SemMotivo_PermaneceBloqueada()
        {
            var form = CriarForm(CriarItemAnonymous("Prod 1", "SKU1", "12345"), CriarItemAnonymous("Prod 2", "SKU2", "999"));
            
            var avisos = new List<(string Mensagem, string Titulo)>();
            form.GetType().GetProperty("ActionMostrarAviso", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Action<string, string>((m, t) => { avisos.Add((m, t)); }));

            int authRequests = 0;
            form.GetType().GetProperty("FuncSolicitarAutorizacao", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Func<IWin32Window, bool>(w => { authRequests++; return true; }));
            
            int motiveRequests = 0;
            form.GetType().GetProperty("FuncSolicitarMotivo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Func<IWin32Window, string>(w => { motiveRequests++; return "   "; }));

            var lst = (ListBox)form.GetType().GetField("lstPendentes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
            lst.SelectedIndex = 0;

            var btn = (Button)form.GetType().GetField("btnLiberacaoManual", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
            var clickMethod = form.GetType().GetMethod("BtnLiberacaoManual_Click", BindingFlags.NonPublic | BindingFlags.Instance);
            clickMethod.Invoke(form, new object[] { btn, EventArgs.Empty });

            var itens = ObterItensDoForm(form);
            Assert.Equal(1, authRequests);
            Assert.Equal(1, motiveRequests);
            Assert.Single(avisos);
            Assert.Contains("motivo", avisos[0].Mensagem, StringComparison.OrdinalIgnoreCase);
            Assert.False(IsConferido(itens[0]));
            Assert.False(IsConferido(itens[1]));
            Assert.NotEqual(DialogResult.OK, form.DialogResult);
        }

        [Fact]
        public void LiberacaoManual_Autorizada_RegistraAuditoria()
        {
            // Precisamos do banco para a auditoria
            using var fixture = new Infrastructure.SqliteTestFixture();

            var form = CriarForm(CriarItemAnonymous("Prod 1", "SKU1", "12345"), CriarItemAnonymous("Prod 2", "SKU2", "999"));
            
            var avisos = new List<(string Mensagem, string Titulo)>();
            form.GetType().GetProperty("ActionMostrarAviso", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Action<string, string>((m, t) => { avisos.Add((m, t)); }));

            int authRequests = 0;
            form.GetType().GetProperty("FuncSolicitarAutorizacao", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Func<IWin32Window, bool>(w => { authRequests++; return true; }));
            
            int motiveRequests = 0;
            form.GetType().GetProperty("FuncSolicitarMotivo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Func<IWin32Window, string>(w => { motiveRequests++; return "Item danificado"; }));

            // Simula seleção do primeiro item na lista para liberar somente ele
            var lst = (ListBox)form.GetType().GetField("lstPendentes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
            lst.SelectedIndex = 0;

            var btn = (Button)form.GetType().GetField("btnLiberacaoManual", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
            var clickMethod = form.GetType().GetMethod("BtnLiberacaoManual_Click", BindingFlags.NonPublic | BindingFlags.Instance);
            clickMethod.Invoke(form, new object[] { btn, EventArgs.Empty });

            var itens = ObterItensDoForm(form);
            Assert.Equal(1, authRequests);
            Assert.Equal(1, motiveRequests);
            Assert.Empty(avisos);
            Assert.True(IsConferido(itens[0]));
            Assert.False(IsConferido(itens[1])); // O segundo não pode ser liberado!

            // Verifica se a auditoria foi salva
            using var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={SistemaConferenciaPedidos.Data.Database.CaminhoBanco}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM AuditoriaAdministrativa WHERE Acao LIKE '%LIBERACAO_MANUAL_EAN%' AND Acao LIKE '%Item danificado%' AND Acao LIKE '%12345%'";
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            Assert.Equal(1, count);
        }
        [Fact]
        public void LiberacaoManual_Autorizada_ConfereSomenteUmaUnidadeDoItemSelecionado()
        {
            using var fixture = new Infrastructure.SqliteTestFixture();

            var form = CriarForm(
                CriarItemAnonymous("Prod 1", "SKU1", "12345"),
                CriarItemAnonymous("Prod 1", "SKU1", "12345"),
                CriarItemAnonymous("Prod 2", "SKU2", "999")
            );
            
            var avisos = new List<(string Mensagem, string Titulo)>();
            form.GetType().GetProperty("ActionMostrarAviso", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Action<string, string>((m, t) => { avisos.Add((m, t)); }));

            int authRequests = 0;
            form.GetType().GetProperty("FuncSolicitarAutorizacao", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Func<IWin32Window, bool>(w => { authRequests++; return true; }));
            
            int motiveRequests = 0;
            form.GetType().GetProperty("FuncSolicitarMotivo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(form, new Func<IWin32Window, string>(w => { motiveRequests++; return "Item ilegível"; }));

            var lst = (ListBox)form.GetType().GetField("lstPendentes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
            lst.SelectedIndex = 0; // Seleciona o primeiro "Prod 1"

            var btn = (Button)form.GetType().GetField("btnLiberacaoManual", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
            var clickMethod = form.GetType().GetMethod("BtnLiberacaoManual_Click", BindingFlags.NonPublic | BindingFlags.Instance);
            clickMethod.Invoke(form, new object[] { btn, EventArgs.Empty });

            var itens = ObterItensDoForm(form);
            
            Assert.Equal(1, authRequests);
            Assert.Equal(1, motiveRequests);
            Assert.Empty(avisos);
            Assert.True(IsConferido(itens[0]));
            Assert.False(IsConferido(itens[1])); // O segundo Prod 1 deve continuar pendente
            Assert.False(IsConferido(itens[2])); // O Prod 2 deve continuar pendente
            Assert.NotEqual(DialogResult.OK, form.DialogResult); // O pedido não foi integralmente concluído

            using var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={SistemaConferenciaPedidos.Data.Database.CaminhoBanco}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM AuditoriaAdministrativa";
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            Assert.Equal(1, count); // Exatamente uma auditoria foi criada, nenhuma liberação universal
        }
    }
}


