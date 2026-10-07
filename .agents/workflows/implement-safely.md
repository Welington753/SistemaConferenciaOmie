# Workflow: /implement-safely

**Entrada Necessária:** Um Plano de Implementação previamente gerado e **aprovado**.

## Passos:
1. Validar o ambiente com `git branch` e `git status --short`.
2. Executar uma única tarefa/etapa por vez baseada estritamente nas check-boxes do plano aprovado.
3. Usar a abordagem de TDD nas codificações de comportamento: implementar ou adaptar o teste, comprovar falha, efetuar o patch C#, comprovar correção de comportamento.
4. Rodar a verificação de `dotnet build` e a verificação local (`dotnet test`) de imediato para a unidade tocada.

## Critérios de Parada:
Não force a continuidade de etapas sequenciais sobre uma etapa falha. Se o build ou teste quebrar em um cenário não coberto pelo plano aprovado, estanque a implementação. Atualize a listagem de tarefas com o resultado real parcial.

## Evidências Obrigatórias:
Sucesso isolado e repetido nas rotinas recém-adicionadas (comandos locais no terminal e capturas de diff curtas e limpas mostrando que nenhuma rotina paralela foi afetada desnecessariamente).

## Arquivos Permitidos e Proibidos:
- **Permitidos:** Somente as classes de teste e códigos `.cs` nomeados expressamente no plano.
- **Proibidos:** Operações destrutivas não estipuladas. Commits automáticos em cadeia (`git commit -am`) desassociados do acompanhamento. Nenhuma alteração cruzada.

## Rollback:
Uso de `git restore <file>` no arquivo alterado da etapa local caso o teste ou build não alcance êxito imediato no fim da fase cirúrgica. 

## Relatório Final Esperado:
Demonstrar a evolução da *task list* recém operada e notificar a disponibilidade de diff local final, encaminhando o usuário para a validação.
