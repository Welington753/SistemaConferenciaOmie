# Testing & Git Policy

## 1. Regras do Repositório Git
- **Investigação Antes da Alteração**: Sempre verifique a branch atual, o hash e o status de rastreamento com `git status --short` e `git rev-parse HEAD` antes de alterar código.
- **Não Destruição**: NUNCA use `git reset --hard` para reverter, não use `git clean` de forma descuidada ou descarte alterações não mescladas sem aprovação.
- **Checkpoint de Risco**: Faça um commit (checkpoint) local antes de executar grandes operações de refatoração ou comandos muito destrutivos.
- **Rastreabilidade**:
  - Somente o Agente Controlador mestre gera commits (subagentes operacionais não executam git add/commit diretamente).
  - Faça commits isolados, pequenos e focados na tarefa técnica resolvida. Todos devem ser reversíveis.
  - Revise arquivos usando `git diff --check` antes de aprovar o commit final.
- **Higiene e Ignorados**: NUNCA versão ou empacote (`git add`) arquivos artefatos: pastas `bin/`, `obj/`, diretórios de cache `TestResults/`, arquivos ZIP criados durante auditoria, ou logs com credenciais e chaves API vazadas.

## 2. Garantias e Verificações (Tests)
- Nunca conclua uma alteração de código sem antes garantir as checagens com sucesso no terminal (`dotnet build` e `dotnet test`).
- Em cenários críticos (mudanças de base SQLite, refatorações da injeção de dependência Omie ou WinForms UI): É mandatório testar executando a suíte de ponta a ponta duas vezes com sucesso sem intermitências.
- **Proteção contra Hang (Travamentos)**: 
  - Sempre adicione parâmetros de diagnóstico em testes susceptíveis ao UI blocking, como `--blame-hang` e `--blame-hang-timeout 3m`.
  - Se um hang ou loop infinito ocorrer, não lance de imediato outro `dotnet test`. Antes de repetir, utilize PowerShell (`Get-Process dotnet,testhost` e `Get-CimInstance Win32_Process`) para detectar processos retidos e, se necessário para desbloqueio do compilador/banco, encerre puramente os `testhost.exe` travados no background.
- O relatório de conclusão deverá expor comandos utilizados, o status exato das checagens (ex: número de testes executados, sucesso, contagem de threads).
