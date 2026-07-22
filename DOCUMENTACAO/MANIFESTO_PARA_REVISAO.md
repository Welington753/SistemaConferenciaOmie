# Manifesto de Projeto para Revisão Técnica

Este documento fornece um panorama completo do projeto `SistemaConferenciaPedidos` preparado para a revisão técnica externa.

---

## 1. Identificação do Projeto e Geração
- **Nome do Projeto:** `SistemaConferenciaPedidos`
- **Caminho Original da Pasta:** `C:\Users\User\Documents\Visual Studio 18\SistemaConferenciaPedidos`
- **Data e Hora de Geração:** 2026-07-17 às 08:25

---

## 2. Estrutura Geral das Pastas
O pacote foi estruturado mantendo a seguinte organização:
- `SistemaConferenciaPedidos/` (Projeto principal WinForms)
  - `Data/` (Código de persistência e banco de dados)
  - `DOCUMENTACAO/` (Relatórios de segurança, checklists e resultados do build)
  - `Helpers/` (Classes utilitárias de normalização e comparação)
  - `Models/` (Entidades e estruturas de dados de transporte)
  - `Properties/` (Metadados do projeto Windows Forms)
  - `Repositories/` (Camada de acesso a dados SQLite)
  - `Services/` (Regras de negócio, validações, serviços de etiquetas e APIs)
- `SistemaConferenciaPedidos.Tests/` (Projeto de testes automatizados xUnit)
- `SistemaOmie.Shared/` (Dependência de integração com API Omie)
- `SistemaConferenciaPedidos.slnx` (Arquivo de solução Visual Studio)

---

## 3. Componentes do Sistema

### 3.1. Arquivos de Solução e Projetos
- `SistemaConferenciaPedidos.slnx` (Arquivo de Solução XML)
- `SistemaConferenciaPedidos/SistemaConferenciaPedidos.csproj`
- `SistemaConferenciaPedidos.Tests/SistemaConferenciaPedidos.Tests.csproj`
- `SistemaOmie.Shared/SistemaOmie.Shared.csproj`

### 3.2. Principais Telas (Windows Forms)
- `FrmConferencia.cs`: Tela principal de conferência de itens do pedido.
- `FrmPreparacaoPedidos.cs`: Dashboard operacional para preparação e triagem de pedidos.
- `FrmBuscarPedidoPorProduto.cs`: Pesquisa rápida de pedidos por descrição ou código de produto.
- `FrmSenhaAdministrativa.cs`: Interface de autorização e segurança de ações de admin.
- `FrmValidacaoEan.cs`: Interface de bipagem e validação rápida de código de barras.
- `FrmAdministracao.cs`: Tela administrativa do sistema (limpezas e auditorias).

### 3.3. Principais Serviços (Services)
- `AdminAuthService.cs`: Hashing seguro de senha com PBKDF2/SHA256.
- `ConferenciaService.cs`: Fluxos de validação de conferência e status dos pedidos.
- `ValidacaoPreImpressaoService.cs`: Motor de validação fail-closed que analisa páginas de PDF e vínculos.
- `VinculacaoEtiquetaService.cs`: Processamento de PDF/ZPL de marketplaces (Mercado Livre, Shopee, Amazon).
- `ImpressaoService.cs`: Integração de impressão de etiquetas em PDF/ZPL.
- `EtiquetaService.cs`: Utilitários de leitura, parse e processamento de dados binários ZPL/PDF.

### 3.4. Principais Repositories
- `PedidoRepositorySqlite.cs`: Implementação das queries de manipulação de pedidos (incluindo UPSERT e buscas por período).
- `ConfiguracaoRepositorySqlite.cs`: Persistência de parâmetros locais.

### 3.5. Principais Models
- `PedidoConferencia.cs`
- `ResultadoValidacaoPreImpressao.cs`
- `ResultadoImpressao.cs`
- `OmieConfig.cs`

---

## 4. Banco de Dados e Migrações
O sistema utiliza SQLite como banco de dados embarcado. A inicialização e as migrações são executadas programaticamente através da classe `Database.cs`, com suporte para inserção incremental de novas tabelas (ex: `AuditoriaAdministrativa`) e novas colunas (ex: `CaminhoZipImportacao`, `NomePdfNoZip`, `PaginaPdf`).

---

## 5. Dependências NuGet
- `BinaryKits.Zpl.Viewer` (1.3.1)
- `Microsoft.Data.Sqlite` (10.0.7)
- `PdfiumViewer` (2.13.0)
- `PdfPig` (0.1.14)
- `PDFsharp` (6.2.4)
- `SkiaSharp` (3.119.2)
- `Tesseract` (5.2.0 - OCR para leitura de etiquetas)
- `ZXing.Net` (0.16.11)
- `Moq` (4.20.72) (Testes)
- `xunit` (2.9.3) (Testes)

---

## 6. Lista de Documentações Existentes
1. `AUDITORIA_SEGURANCA_IMPRESSAO.md`
2. `CHECKLIST_CONFERENCIA_TODOS_DO_DIA.md`
3. `CHECKLIST_SEGURANCA_IMPRESSAO.md`
4. `CORRECAO_GRADE_E_PAGINAS_PDF.md`
5. `CORRECAO_PDF_MELI_E_ZIP_SHOPEE.md`
6. `CORRECAO_VALIDACAO_PDF_E_FILTRO_DATA.md`
7. `DIAGNOSTICO_PAGINAS_MELI_CRUZADAS.md`
8. `PLANO_MELHORIAS_SEGURAS.md`
9. `VALIDACAO_FINAL_FAIL_CLOSED.md`
10. `RESULTADO_BUILD_PARA_REVISAO.md` (Novo)
11. `SCHEMA_BANCO_ATUAL.sql` (Novo)
12. `RESUMO_BANCO_SEM_DADOS.md` (Novo)
13. `SEGREDOS_REMOVIDOS.md` (Novo)

---

## 7. Resultados do Build e Testes
- **Compilação:** Aprovada (0 erros, 128 warnings).
- **Testes Automatizados:** 44 de 44 testes aprovados (100% de sucesso).
