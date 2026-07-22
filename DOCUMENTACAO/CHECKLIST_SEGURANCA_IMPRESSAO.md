# Checklist - Segurança na Impressão e Fail-Closed

1. [ ] **Validar os Vínculos**: Importar um lote real com ML, Shopee e Amazon. Clicar em "Validar vínculos do dia". Conferir se todos os pedidos deram OK.
2. [ ] **Testar Impressão Única**: Escolher um pedido (ML, Amazon, Shopee), verificar o número. Imprimir. Confirmar se a etiqueta corresponde.
3. [ ] **Troca Durante a Impressão**: Selecionar pedido A. Clicar em Imprimir. Muito rapidamente, antes de terminar, clicar no pedido B. Confirmar que a impressora imprimiu apenas a etiqueta de A, e o status de A ficou `Impresso = true`.
4. [ ] **Testar Concorrência (F2 Rápido)**: Pressionar F2 muito rápido seguidamente (3 vezes) num pedido. Apenas UMA etiqueta deve sair, e o sistema deve ignorar ou alertar que a impressão está em andamento.
5. [ ] **Troca de ZPL Proposital**: Pelo banco de dados (SQLite), colar o ZPL do pedido X no registro do pedido Y. No sistema, tentar imprimir Y. Confirmar que o sistema exibe "IMPRESSÃO BLOQUEADA" devido a número/rastreio incompatível, e *não imprime*.
6. [ ] **Duplicidade**: Pelo banco de dados, colocar o mesmo `CodigoEtiqueta` em 2 pedidos não impressos no dia. Clicar no botão "Validar vínculos do dia". Ambos devem acusar inconsistência. Tentar imprimir qualquer um. Deve bloquear.
7. [ ] **Clientes Homônimos**: Testar a impressão de dois clientes com nome idêntico (ex: "Maria da Silva") e CPFs/Endereços diferentes. Garantir que as validações passam sem misturar etiquetas.
8. [ ] **Falha no Spooler**: Se a impressora não for encontrada (estado desconhecido/erro de envio), confirmar que o pedido continua como `Impresso = false`.
