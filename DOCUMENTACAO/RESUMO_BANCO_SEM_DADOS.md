# Resumo Estatístico do Banco de Dados

Documento gerado em: 2026-07-17 08:18:49

## 1. Contagem de Registros por Tabela

| Tabela | Registros |
| :--- | :--- |
| Pedidos | 111 |
| Configuracoes | 4 |
| AuditoriaAdministrativa | 5 |

## 2. Índices Configurados

| Índice | Tabela Relacionada |
| :--- | :--- |
| idx_pedidos_numeropedidocliente | Pedidos |
| idx_pedidos_marketplace | Pedidos |
| idx_pedidos_codigoetiqueta | Pedidos |
| idx_pedidos_impresso | Pedidos |
| idx_pedidos_dataconferencia | Pedidos |
| idx_pedidos_oculto | Pedidos |
| idx_pedidos_dataprevisao | Pedidos |

## 3. Estado das Migrações e Colunas Recentes
- **Pedidos:** Colunas adicionadas recentemente para paginação de PDF do Mercado Livre e Shopee:
  - `CaminhoZipImportacao` (TEXT)
  - `NomePdfNoZip` (TEXT)
  - `PaginaPdf` (INTEGER)
- **AuditoriaAdministrativa:** Tabela criada para auditoria de ações (reimpressões, exclusões, liberação).
