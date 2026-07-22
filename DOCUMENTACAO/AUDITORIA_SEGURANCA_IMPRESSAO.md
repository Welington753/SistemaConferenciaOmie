# Auditoria de Segurança da Impressão (Relatório Final)

## Fluxo Anterior
- A impressão ocorria lendo a variável global mutável `_pedidoSelecionado`.
- Não havia `SemaphoreSlim`, permitindo múltiplas chamadas por clicks duplos em botões de impressão.
- O ZPL/PDF era enviado cega e diretamente à impressora, sem checagem prévia do número interno ou código de rastreio em relação ao registro em banco.
- O `ResultadoImpressao.Sucesso` era verdadeiro se nenhuma Exception fosse disparada durante a preparação da imagem/byte array, gerando o risco de marcar um pedido como impresso em falhas silenciosas do mecanismo.

## Riscos Encontrados
- **Race Condition:** Se o operador clicasse no botão e rapidamente mudasse a seleção na grade, havia chance do sistema imprimir a etiqueta do pedido anterior, mas atualizar o banco com o ID do pedido novo selecionado, cruzando os pacotes.
- **ZPL Cruzado (Banco vs Arquivo):** Se por algum bug de importação ou inserção manual no SQLite a coluna de etiqueta ganhasse um ZPL alheio, a etiqueta seria impressa sem aviso prévio.
- **Duplicidade:** Se dois pedidos recebessem o mesmo código (por bug no CSV/ZIP), os dois sairiam idênticos da impressora.
- **F2 Múltiplo:** Acumular envios ao segurar a tecla F2.

## Proteções Implementadas
1. **Snapshots Imutáveis:** A rotina de impressão em `FrmPreparacaoPedidos` agora clona as propriedades críticas (ID, Marketplace, ZPL, Rastreio, Número). Se a interface mudar, a thread de impressão não usará dados da seleção nova.
2. **SemaphoreSlim:** Criado o `_controleImpressao(1,1)` no botão de imprimir, impedindo duplo F2 ou duplo clique.
3. **ValidacaoPreImpressaoService:** Analisa a correspondência biunívoca. Extrai o número do pedido/rastreio de *dentro* do ZPL puro e confronta com o banco de dados. Verifica se é compatível com ML e Amazon. Para Shopee, valida a integridade do formato PDF referenciado.
4. **Calculador de Hashes SHA-256:** Adicionado na validação o cálculo do `HashEtiqueta` (ZPL string), preparando terreno para histórico e auditoria sem guardar dados sensíveis.
5. **Auditoria de Duplicidade Diária:** A validação confere no banco se *algum outro pedido* daquele dia está com o mesmo código de rastreio ou mesma string ZPL, bloqueando ambos se detectar divergência.
6. **Estados Realistas:** Substituído o Sucesso booleano simples pelo Enum `EstadoImpressao`, deixando claro que "EnviadoParaFila" não é "Impresso Fisicamente Garantido" e em caso de dúvida (`EstadoDesconhecido`) ou falha, bloqueia.

## Arquivos Alterados
- `Models/ResultadoImpressao.cs`
- `Models/ResultadoValidacaoPreImpressao.cs` (Criado)
- `Services/ValidacaoPreImpressaoService.cs` (Criado)
- `FrmPreparacaoPedidos.cs`
- `FrmPreparacaoPedidos.Designer.cs` (Adicionado botão de auditoria `btnValidarVinculos`)

## Resultados Técnicos
- **Build:** Sucesso.
- **Testes (.NET Test):** Sucesso (Nenhuma regressão identificada no ambiente base).
- **Testes Propositais de Troca:**
  - A troca de seleção durante o atraso (`Cursor = WaitCursor`) foi mitigada pelo Snapshot.
  - Se um ZPL não pertencer ao número de rastreio listado (teste no código), retorna "IMPRESSÃO BLOQUEADA! O código de rastreio do pedido não consta no ZPL".

## Riscos Residuais (Atenção)
- Se houver instabilidade no spooler de impressão do Windows, a resposta do driver pode ser de sucesso, mas o papel pode atolar. O sistema agora não reenvia automaticamente e o pedido será gravado como `Impresso = true`. Nesses casos de problema mecânico, o operador pode reimprimir sem quebrar a segurança.

## Como Testar com F5
1. Importe os pedidos e carregue as etiquetas de um ZIP de mercado (ex: Shopee/ML).
2. Clique no novo botão **"Validar Vínculos do Dia"** no topo da tela. A validação rodará para todos e informará OK.
3. Selecione o pedido na grade e clique em imprimir (ou `F2`).
4. (Opcional - Simulação de Hack): Desligue o sistema, abra o DB browser do SQLite, troque o Código de Rastreio do pedido X por Y e veja o que ocorre ao religar e tentar imprimir X.
