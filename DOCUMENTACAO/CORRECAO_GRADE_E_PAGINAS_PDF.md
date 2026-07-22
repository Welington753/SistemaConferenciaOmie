# Relatório de Correção - Grade e PDFs

## 1. Grade de Preparação Vazia

**Problema:** A grade de preparação não estava exibindo os 56 pedidos do dia (embora eles existissem no banco de dados e fossem acusados pelo validador).

**Causa Raiz:** A query SQL de busca `ObterPorPeriodo` comparava a data convertida como string ISO 8601 (`2026-07-16T00:00:00.0000000+00:00`) contra o formato armazenado no SQLite (`2026-07-16 00:00:00`), resultando em falha na correspondência. Além disso, a importação armazenava datas nulas como `""` em vez de `NULL`, o que prejudicava os filtros SQL padrão.

**Solução Aplicada:**
* Corrigido o `PedidoSincronizacaoService.cs` para persistir campos nulos como `DBNull.Value`.
* Alterado o UPSERT para checar corretamente a nulidade com `IS NOT NULL`.
* Corrigida a query em `PedidoRepositorySqlite.cs` para utilizar a função nativa `date()` do SQLite, formatando os parâmetros em `yyyy-MM-dd`. Desta forma a filtragem é coerente, independente das variações dos formatos textuais de data.
* Cobertura via novos testes automatizados em `GradePreparacaoTests.cs`.

## 2. Mercado Livre: Páginas Cruzadas

**Problema:** Sete pedidos do ML estavam com associação incorreta de páginas de PDF, originando um problema circular no qual pedidos apontavam para etiquetas de outros clientes.

**Causa Raiz:** A rotina de reimportação de PDFs (`ImportarPdfMercadoLivre`) apoiava-se primariamente no `NumeroVenda` (16 dígitos). No ML, as páginas de etiqueta muitas vezes contêm apenas o Pack ID, ou a extração do Número de Venda falhava, fazendo com que a lógica de "primeiro que bater" assumisse páginas incorretas.

**Solução Aplicada:**
* Refatorada a lógica de correspondência para dar precedência absoluta ao **CodigoEtiqueta** (47XXXXXXXXX).
* Agora, a rotina cruza o código já conhecido do banco com os códigos extraídos diretamente do arquivo importado. Quando o código bate, a página é vinculada com precisão.
* Implementados testes em `MercadoLivrePaginaTests.cs` validando esse comportamento.

## 3. Shopee: PDF não localizado no ZIP

**Problema:** Quatro pedidos da Shopee seguiam sem localizar o seu respectivo PDF dentro do ZIP, mesmo constando no banco.

**Causa Raiz:** A rotina de cura de registros antigos localizava o arquivo no ZIP, atualizava o `NomePdfNoZip`, mas esquecia de atualizar a propriedade principal `EtiquetaMarketplaceZpl` (que ficava ainda retendo o nome arcaico do arquivo sem subpastas). Com isso, a pre-impressão continuava a procurar pelo caminho antigo e falhava com "PDF Ausente".

**Solução Aplicada:**
* Ajustado `VinculacaoEtiquetaService.cs` (cura Shopee) para usar comparação _case-insensitive_ do código de rastreio e usar fallback nos casos de única página correspondente.
* Adicionada a instrução de atualização efetiva da string da ZPL para que o pipeline de impressão encontre o arquivo no caminho completo do ZIP.
