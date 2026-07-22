# Relatório de Segredos e Credenciais Removidos

Este documento registra as credenciais e informações confidenciais que foram sanitizadas e substituídas por placeholders no pacote de revisão técnica.

---

## 1. Credenciais Sanitizadas no Código Fonte

- **Arquivo:** `SistemaConferenciaPedidos/Services/PedidoOmieService.cs`
  - **Tipo de Segredo:** Omie AppKey
    - **Ação:** Substituído por `"[REMOVIDO_PARA_REVISAO]"` na cópia compactada.
  - **Tipo de Segredo:** Omie AppSecret
    - **Ação:** Substituído por `"[REMOVIDO_PARA_REVISAO]"` na cópia compactada.

---

## 2. Credenciais e Bancos de Dados Excluídos

- **Arquivo de Banco de Dados (`sistema_conferencia.db`):**
  - **Tipo de Segredo:** Chaves de configuração salvas na tabela `Configuracoes`, histórico de auditorias e hashes/salts da senha administrativa do sistema.
  - **Ação:** Todos os arquivos de banco de dados SQLite (`.db`, `.db-wal`, `.db-shm`) foram excluídos do pacote compactado. Apenas o arquivo `SCHEMA_BANCO_ATUAL.sql` e a estrutura das tabelas foram preservados.
