# Validação Final de Segurança de Impressão (Fail-Closed)

## BLOQUEADORES PARA PRODUÇÃO

- **Validação Shopee Incompleta (Risco Crítico):** Atualmente, o `ValidacaoPreImpressaoService` considera a etiqueta Shopee válida apenas por verificar se a string inicia com `PDF_SHOPEE|` e se contém as referências corretas. Ele **não** extrai o código BR de dentro do PDF físico em tempo de impressão, o que pode permitir impressão errada caso o arquivo PDF subjacente tenha sido alterado manualmente. Conforme instrução, este é um **bloqueador crítico**. A correção proposta é injetar o `ShopeePdfService` no validador para extrair a página na hora da impressão e confrontar o número.
- **Ausência de Testes Automatizados (Risco Crítico):** O projeto não possui um projeto de testes de unidade configurado (o `dotnet test` não detecta nenhum teste automatizado executado). Os cenários de troca proposital só podem ser validados manualmente no momento.

---

## PROTEÇÕES CONFIRMADAS

### 1. Build e Testes
- **Erros de Build:** 0
- **Warnings:** 109 (maioria referências nulas)
- **Quantidade total de testes:** 0 (Não existe projeto de testes automatizados configurado na solução).
- **Testes aprovados/reprovados/ignorados:** N/A.

### 2. Pontos de Entrada da Impressão
1. **Botão Imprimir Etiqueta:** `FrmPreparacaoPedidos.cs` > `btnImprimirEtiqueta_Click`.
2. **Tecla F2:** `FrmPreparacaoPedidos.cs` > `ProcessCmdKey` (que aciona `btnImprimirEtiqueta.PerformClick()`).
3. **Impressão por Produto:** `FrmPreparacaoPedidos.cs` > `btnImprimirPorProduto_Click` chama `ImprimirPedidoEncontradoPorProduto`, que chama `ImprimirPedidoSelecionado()`. No momento, `ImprimirPedidoSelecionado()` **NÃO passa pelo ValidacaoPreImpressaoService nem pelo Semaphore**. *(Identificado como falha técnica menor a ser corrigida na próxima fase)*.

### 3. Snapshot e Identidade do Pedido
- **Implementado e Confirmado.** Em `btnImprimirEtiqueta_Click`, a cópia isolada (Snapshot) é instanciada e alimentada com os dados exatos (ID, NúmeroPedido, ZPL, Rastreio). Toda a validação (`_validacaoPreImpressaoService.ValidarAntesDaImpressao(snapshot)`) e o envio (`_impressaoService.ImprimirPedido(snapshot, ...)`) acontecem sobre o snapshot.
- Se a interface (`CurrentRow` ou `_pedidoSelecionado`) for alterada rapidamente pelo usuário durante o travamento, o envio utilizará os dados imutáveis originais e marcará aquele pedido específico original como `Impresso = true` recarregando-o pelo Banco.

### 4. Validação Amazon
- **Confirmado.** O serviço normaliza o ZPL (`TextoHelper.SomenteLetrasENumeros`) e procura de forma exata o `NumeroPedidoCliente` (sem hifens) e o `CodigoEtiqueta` (rastreio TBR/AMZ). Apenas uma correspondência é permitida, e se ausente, falha e bloqueia.

### 5. Validação Mercado Livre
- **Confirmado.** O sistema isola o prefixo `20000` do ID do ML para checar no ZPL. O código de rastreio também é verificado diretamente na string de impressão (ZPL). Nenhuma aproximação ou uso de `Nome do Cliente` é feita.

### 6. Validação Shopee
- **Não Implementado / Implementado com Risco (CRÍTICO).** O serviço lê apenas o campo `EtiquetaMarketplaceZpl` e valida se existe a referência `PDF_SHOPEE|nomearquivo|pagina`. Ele não realiza a extração do código BR de dentro do arquivo para cruzar no instante H da impressão.

### 7. Hash SHA-256
- **Parcialmente Confirmado.** O Hash SHA-256 é corretamente calculado no `ValidacaoPreImpressaoService.CalcularHash` e embutido no `ResultadoValidacaoPreImpressao`. O conteúdo usado é a string ZPL inteira ou a referência em string do PDF Shopee. Contudo, o sistema ainda precisa ligar isso na tabela de Auditoria para armazenamento final persistente, já que no momento existe apenas em memória.

### 8. Duplicidades e Ambiguidades
- **Confirmado.** A regra `ValidarIntegridadeVinculacoesDoDia` varre os pedidos do dia, exclui cancelados, e se houver duplicidade do mesmo Código de Rastreio ou mesmo ZPL para pedidos diferentes, o `FirstOrDefault` encontra o conflito e trava a impressão devolvendo a mensagem "O Código de Rastreio está duplicado com o pedido: X".

### 9. SemaphoreSlim e Duplo Clique
- **Confirmado.** No fluxo de `btnImprimirEtiqueta_Click`, o `await _controleImpressao.WaitAsync(0);` garante que apenas a primeira tentativa entra no bloco `try`. Múltiplos toques ou cliques emitem "Uma impressão já está em andamento. Aguarde." O `Release()` ocorre estritamente dentro do bloco `finally`.

### 10. Resultado e Impresso
- **Confirmado.** O campo `Impresso = true` só é chamado e salvo no repositório **se e somente se** `validacao.Valido` for `true` E a impressora retornar sucesso ou que enfileirou. Não acontece em catch/finally.

### 11. Testes de Troca Proposital
- **Não Testado (Ausência de Testes Automatizados).** Os fluxos lógicos estão corretos no validador, mas a prova de fogo exige um projeto xUnit/NUnit com Mocks das strings ZPL que não está presente neste projeto.

### 12. Validar Vínculos do Dia
- **Confirmado.** O botão "Validar Vínculos do Dia" percorre toda a base, submetendo os pedidos silenciosamente ao `ValidacaoPreImpressaoService` (o mesmo usado para aprovar impressões) e emite um relatório sumário, sem alterar dados.

---
**PAUSA TÉCNICA:** 
Encontrei os bloqueadores reportados acima (Validação Shopee vazia de conteúdo e o método `ImprimirPedidoSelecionado()` usado por Produto furando o Semaphore/Validador). Aguardo orientações para corrigir essas rotas.
