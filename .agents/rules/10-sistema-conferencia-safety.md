# Sistema Conferencia Safety Rules

## Regras Rígidas do Projeto (SistemaConferenciaPedidos)

- **Proteção do Banco de Dados**: NUNCA abra, limpe ou altere o banco de dados operacional em baterias de testes. Testes DEVEM usar somente SQLite temporário (`Data Source=:memory:` ou diretórios temporários com GUID).
- **Proteção de Rede**: NUNCA acesse a rede real da Omie durante os testes. É obrigatório o uso de *fakes* (ex: `FakeOmiePedidoClient`) com contador de chamadas injetado.
- **Credenciais**: Nunca persista credenciais padrão no código fonte. Não exiba chaves de aplicativo (App Key), segredos (App Secret) ou senhas sensíveis em logs ou relatórios do console.
- **Integração de Impressão**: Os serviços de importação e impressão devem falhar de forma segura (fechada). NUNCA um pedido pode ser marcado como impresso no banco antes da confirmação real e final da impressão.
- **Regras de Etiquetagem**:
  - Não vincule etiquetas ambíguas.
  - O nome do cliente NÃO DEVE decidir a vinculação do produto.
  - Evite envio duplicado do mesmo pedido para a impressora.
  - Evite a seleção de produto incorreto provocada por falha silenciosa com o uso cego de `FirstOrDefault` na busca de EAN.
- **Regras de Negócio de Liberação**:
  - Nenhuma liberação universal de EAN pode ser autorizada globalmente.
  - A liberação administrativa e manual EXIGE senha válida, motivo selecionado, e deve gerar e registrar auditoria estrita no banco.
- **Múltiplos Marketplaces**:
  - Nenhuma alteração nos fluxos da Amazon, Shopee ou arquivos de formato ZPL pode ocorrer durante a execução de tarefas exclusivas para o Mercado Livre (e vice-versa).

## Testes UI Headless e WinForms
- Tarefas headless e pipelines CI **não podem abrir `MessageBox` ou modais reais**. O processo travará (hang). É obrigatório injetar delegates controlados (ex: `ActionMostrarAviso`, `FuncSolicitarSenha`) nos construtores ou métodos dos formulários para execução via xUnit.
- Tratamento de foco assíncrono e eventos de UI devem conter verificação rigorosa `!IsDisposed` e `Visible` antes de `BeginInvoke` ou `Invoke`.
- NUNCA execute duas suítes `dotnet test` simultaneamente no mesmo workspace.
- Comandos de testes e chamadas estáticas ao `Database.CaminhoBanco` não devem operar em paralelo ou concorrer em locks. A execução deve ser estritamente serializada.
