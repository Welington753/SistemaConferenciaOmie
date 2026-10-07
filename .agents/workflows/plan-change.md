# Workflow: /plan-change

**Entrada Necessária:** Especificação técnica abstrata, bug-report ou pedido bruto da tarefa de melhoria/modificação.

## Passos:
1. Validar e mapear a árvore de arquivos e dependências ligadas à tarefa alvo.
2. Identificar possíveis interpretações ambíguas no pedido.
3. Projetar o escopo do plano contendo: Objetivo de cada passo, Arquivo exato de manipulação, Dependências cruzadas, Comandos de Verificação locais para aquele step (TDD sugerido) e critérios de rollback.
4. Escrever o plano no formato de checklist (`- [ ]`) em um artefato do Antigravity (`implementation_plan.md` / `docs/plans`).

## Critérios de Parada:
O workflow DEVE PARAR obrigatoriamente assim que a documentação do plano for finalizada e as perguntas em "Open Questions" criadas. **É estritamente obrigatório aguardar a aprovação expressa do usuário humano antes de avançar para implementação.**

## Evidências Obrigatórias:
O artefato de Plano de Implementação formatado na UI com checkboxes para tracking de tarefas.

## Arquivos Permitidos e Proibidos:
- **Permitidos:** Apenas leitura na base de código de produção e criação ou modificação exclusiva de arquivos de artefato de documentação (`implementation_plan.md`, `walkthrough.md`, `task.md` etc).
- **Proibidos:** Nenhuma alteração ativa ou destrutiva nos códigos do repositório (`.cs`, `.csproj`, SQLite). Nenhuma execução agressiva.

## Rollback:
As decisões de plano podem ser facilmente revertidas na documentação a pedido do usuário revisor. 

## Relatório Final Esperado:
Mensagem objetiva indicando a conclusão da compilação do plano, listagem dos riscos descobertos e as decisões técnicas que aguardam aprovação para a virada de fase.
