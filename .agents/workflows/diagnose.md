# Workflow: /diagnose

**Entrada Necessária:** Mensagem de erro, falha de teste, travamento (hang) ou log isolado.

## Passos:
1. Validar estado do repositório com `git status --short`.
2. Rodar comando exato e isolado para reproduzir a falha (ex: `dotnet build` ou `dotnet test --filter <nome>`), coletando saída completa do terminal.
3. Inspecionar causas diretamente nos arquivos em modo read-only.
4. Caso haja travamento (hang), execute `Get-Process dotnet,testhost` via PowerShell para capturar a listagem de threads/bloqueios.

## Critérios de Parada:
O diagnóstico encerra imediatamente ao concluir a captura técnica completa do log/erro ou causa raiz. **Nenhuma correção ou edição** deve ser aplicada nesta etapa.

## Evidências Obrigatórias:
A captura exata do *stacktrace*, a linha exata de falha nos testes ou logs com saída de reprodução.

## Arquivos Permitidos e Proibidos:
- **Permitidos:** NENHUM para edição. Somente leitura permitida (log files, códigos fonte de verificação, saída terminal).
- **Proibidos:** Não salvar, não reverter, e não commitar quaisquer arquivos (C# ou configuração).

## Rollback:
Por ser estritamente read-only, não necessita de rollback em arquivos. Em caso de bloqueio `testhost.exe`, finalize-os para purgar locks pendentes.

## Relatório Final Esperado:
Causa raiz fundamentada do erro, incluindo a linha ou rotina falha e comandos exatos de diagnóstico operados.
