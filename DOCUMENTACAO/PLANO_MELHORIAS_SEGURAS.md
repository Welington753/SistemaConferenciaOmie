# Plano de Melhorias Seguras - SistemaConferenciaPedidos

## 1. Arquivos Encontrados
- **Interface**: `FrmPreparacaoPedidos.cs` (muito extenso, ~67KB), `FrmConferencia.cs`, `FrmValidacaoEan.cs`, `FrmSenhaAdministrativa.cs`.
- **Serviços / Repositórios (existentes ou a serem verificados/criados)**: O projeto possui as pastas `Services`, `Repositories` e `Models`. Verificamos a necessidade de separar a lógica do `FrmPreparacaoPedidos.cs` gradativamente.
- **Configuração**: `SistemaConferenciaPedidos.csproj`, `Program.cs`.

## 2. Métodos Relacionados (Observados nos Avisos)
- `CarregarPedidos(string numeroPedidoParaRestaurar = null)`
- `CarregarItensDoPedidoAsync(string jsonPedido)`
- Métodos não isolados de leitura, impressão e acesso a banco de dados dentro dos formulários (indicado pelo tamanho do arquivo de UI).

## 3. Problemas Confirmados
- O sistema apresenta 83 avisos (*warnings*) de compilação, em grande parte relacionados a referências possivelmente nulas e conversões anuláveis no `FrmPreparacaoPedidos.cs`.
- O pacote `SQLitePCLRaw.lib.e_sqlite3` possui alerta de vulnerabilidade (versão 2.1.11).
- O pacote `PdfiumViewer` foi restaurado com um *target* que pode não ser 100% compatível nativamente sem ressalvas (aviso `NU1701`).
- Alta concentração de regras de negócio na interface (UI).

## 4. Riscos
- Alterar o esquema do banco de dados (SQLite) e corromper os dados históricos.
- Modificar o fluxo de ZPL/PDF e quebrar a impressão atual, que está em produção.
- Introduzir regressão na leitura de etiquetas (Marketplaces variados com regras específicas).

## 5. Ordem das Alterações
Conforme exigido, trabalharemos em fases rigorosas:
- **Fase 1**: Backup e Segurança do Banco (Serviço de backup antes de ações perigosas).
- **Fase 2**: Sincronização Segura com o Omie (Atualização transacional de pedidos).
- **Fase 3**: Resultado Real da Impressão (Retorno de status fiel sobre a impressão).
- **Fase 4**: Conferência Somente de Pedidos Válidos.
- **Fase 5**: Status Corretos de Vinculação.
- **Fase 6**: Isolamento por Marketplace.
- **Fase 7**: Múltiplos PDFs da Shopee.
- **Fase 8**: Correção do Repositório e Datas.
- **Fase 9**: Área Administrativa (Nova aba de gestão).
- **Fase 10**: Proteção das Credenciais do Omie (DPAPI).
- **Fase 11**: Organização do Código (Refatoração final).

## 6. Resultado do Build Inicial
- **Erros**: 0
- **Avisos**: 83
- **Status**: Compilando com sucesso (Tempo decorrido: ~4.9s). O projeto está apto para iniciar a Fase 1.
