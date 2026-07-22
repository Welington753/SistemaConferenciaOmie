# Checklist - Conferência "Todos os pedidos do dia"

Este checklist deve ser executado para validar se a funcionalidade de conferência de pedidos não impressos está correta.

1. [ ] Buscar pedidos.
2. [ ] Importar e vincular etiquetas.
3. [ ] Não imprimir um pedido de teste.
4. [ ] Confirmar na preparação que CodigoEtiqueta está preenchido para o pedido não impresso.
5. [ ] Abrir a tela de conferência.
6. [ ] Selecionar "Somente pedidos impressos" na combobox.
7. [ ] Bipar o código do pedido não impresso de teste.
8. [ ] Confirmar que **não é encontrado**.
9. [ ] Selecionar "Todos os pedidos do dia" na combobox.
10. [ ] Bipar novamente o mesmo código de etiqueta.
11. [ ] Confirmar que o pedido **é encontrado e conferido com sucesso**.
12. [ ] Confirmar no banco de dados ou painel que o status **Impresso continua false** e DataImpressao continua nula.
13. [ ] Fechar a aplicação e abrir novamente.
14. [ ] Selecionar "Todos os pedidos do dia".
15. [ ] Confirmar que o mesmo CodigoEtiqueta continua salvo (pedido pode ser achado sem precisar reimportar/vincular).
16. [ ] Testar um pedido que já está impresso (deve funcionar normalmente em ambos os modos).
17. [ ] Testar um pedido removido administrativamente (não deve ser encontrado em nenhum modo).
18. [ ] Testar um pedido de outro dia (não deve ser encontrado em nenhum modo).
