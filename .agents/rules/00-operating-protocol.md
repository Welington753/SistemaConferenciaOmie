# Antigravity Operating Protocol

## 1. Regra Geral de Planejamento
- Esclareça ambiguidades antes de escrever código.
- Use a skill de `brainstorming` em mudanças abertas ou estruturais.
- Use a skill de `writing-plans` antes de alterações com múltiplos passos.
- NUNCA implemente código antes da aprovação explícita do plano.
- As mudanças devem ser pequenas e cirúrgicas. Não adicione melhorias adjacentes não solicitadas.

## 2. Implementação e Verificação
- Distinga claramente a fase de diagnóstico da fase de implementação.
- Cada etapa precisa de um teste correspondente, comando ou comportamento verificável.
- Nunca declare sucesso em uma tarefa sem evidência recente obtida localmente (saída de logs, builds e testes).
- Pare imediatamente a execução quando uma verificação falhar.
- Nunca use "auto-proceed" diante de qualquer risco, ambiguidade ou operação potencialmente destrutiva.

## 3. Subagentes e Paralelismo
- **NÃO** utilize subagentes para codificação paralela no mesmo fluxo (paralelismo desligado por padrão).
- Revisores em modo somente-leitura (`readonly`) podem trabalhar em paralelo para verificação de diffs ou código.
- Implementadores só podem atuar em paralelo se os arquivos manipulados forem TOTALMENTE diferentes e não possuírem qualquer dependência cruzada.
- Nunca executar testes SQLite/WinForms simultaneamente.
- Nunca permita que dois agentes operem o mesmo `.csproj`, o mesmo banco ou formulário simultaneamente.
- Nenhum subagente tem permissão para fazer `git commit`. 
- Somente o agente controlador mestre integra as mudanças, revisa e comita de forma estritamente sequencial. Qualquer conflito ou sobreposição exige parada e reporte.
