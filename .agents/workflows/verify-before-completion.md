# Workflow: /verify-before-completion

**Entrada Necessária:** Tarefa e funcionalidades listadas como implementadas e requerendo liberação/revisão final.

## Passos:
1. Executar um build completo limpo na raiz da solução (`dotnet build`).
2. Executar baterias completas de teste da infraestrutura WinForms e regra de negócio (`dotnet test --no-build --blame-hang-timeout 3m`).
3. Verificar vazamento local da ausência de segredos via inspeção com `git diff --check`.
4. Buscar por contadores ou locks ativos deixados pendentes (usar comandos de PowerShell para mapeamento de `testhost.exe`).

## Critérios de Parada:
O workflow NÃO avança para mensagem de sucesso se apenas 1 dos testes quebrar, apresentar travamento local (hang), ou logs exibirem falha de credenciais. Warnings destrutivos ou detecção de lixo em staging param a aprovação de finalização.

## Evidências Obrigatórias:
Obrigatório o reporte das saídas cruas de console: Contagem `Aprovados X, Falhos 0`, tempos de teste e prova material de finalização do processo assíncrono. Em caso de suíte sensível, exige o report de 2 execuções limpas consecutivas.

## Arquivos Permitidos e Proibidos:
- **Permitidos:** Ferramentas read-only do PowerShell, Git e chamadas de compilação CLI de diagnóstico.
- **Proibidos:** Nenhum auto-diagnóstico agressivo mascarando os resultados (ex: silenciar erros, forçar flags obscuras `Ignore` em testes que quebram). Nenhum arquivo C# fonte.

## Rollback:
Recuar no commit ou na aprovação de fechamento do Issue. Acionar novo ciclo de diagnóstico ou ajuste.

## Relatório Final Esperado:
Relatório listando as conferências finais completas, permitindo total confiança ao humano para solicitar a liberação via `git push` ou entrega e gerar ZIP.
