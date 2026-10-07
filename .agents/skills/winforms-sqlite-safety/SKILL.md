---
name: winforms-sqlite-safety
description: Protege alterações e testes no SistemaConferenciaPedidos WinForms com SQLite, UI headless, Omie e conferência de EAN. Usar ao modificar formulários, banco, testes WinForms, integrações Omie, impressão ou vinculação de etiquetas.
---

# WinForms & SQLite Safety Protocol

## 1. Pré-Requisitos para Alterações
- Confirme que a branch é apropriada para a tarefa.
- Execute alterações prevendo testes unitários e com cobertura TDD (Test-Driven Development), especialmente para áreas de negócio críticas como EAN e Liberação Manual.

## 2. Isolamento de Banco de Dados
- **Isolamento Mandatório:** Nunca escreva no banco principal de produção ou banco de dados instanciado via UI.
- Testes que necessitem de integridade SQLite DEVEM usar memória (`Data Source=:memory:`) ou gerar arquivos atrelados a diretórios temporários na execução com sufixos únicos (ex: usando `Guid.NewGuid()`).
- **Prevenção de Locks e Concorrência:** Para evitar exceptions em lote (Acesso Negado ou arquivo aberto) durante testes que tocam o file system do Windows, adicione a chamada de purga `SqliteConnection.ClearAllPools()` na cláusula `finally` ou no método `Dispose()` das Fixtures/Testes.

## 3. Segurança UI em Testes (Headless WinForms)
- Pipelines de testes e processos acionados sem ambiente gráfico acoplado (como testes `xUnit` executados via terminal sem form) irão travar se diálogos nativos bloqueantes se abrirem.
- **NUNCA** chame `MessageBox.Show()`, modais bloqueantes ou requisições visuais de senha ativas sem utilizar a injeção de delegates na classe base ou propriedades interceptadoras (ex: `ActionMostrarAviso`, `FuncSolicitarSenha`). Em ambiente CLI o xUnit irá reter o processo e não terminar.
- Em manipulações assíncronas do ciclo de vida da UI (ex: refocar campo após a conferência): Use `!IsDisposed` e confira propriedades da base de renderização antes de expedir calls em `BeginInvoke`.
- Certifique auditoria C# para Liberação Administrativa, atestando seleção restrita do item correto.

## 4. Ordem e Serialização de Comandos
- NUNCA submeta comandos de `dotnet test` de suítes de formulários paralelamente. Testes WinForms baseados no mesmo handle estático tendem à colisão.
- Adicione parâmetros limitadores aos testes (`--blame-hang-timeout 3m`) durante a verificação de sanidade para impedir o consumo passivo dos recursos caso uma UI dialogue abra inadvertidamente.

## 5. Mock de Rede (Omie) e Resolução Cega
- Integrações que testam rotas Omie dependem restritamente da injeção de `FakeOmiePedidoClient` ou interfaces similares. Nenhuma requisição viva via internet é permitida na suíte técnica.
- Ao pesquisar produto na conferência, coíba buscas silenciadas como o `FirstOrDefault()` sem cláusulas estritas. Assegure que as regras de negócio filtram corretamente e tratam falhas de forma visível ao invés de atrelar conferências ambíguas a itens alheios.
