# Correção Definitiva: Falsos Positivos Mercado Livre e Shopee

## Resumo do Problema Original
O botão "Validar Vínculos do Dia" apresentou 11 inconsistências:
- **4 da Shopee**: O PDF não existia dentro do ZIP ou ZIP ausente. Isso acontecia pois registros antigos foram salvos antes do sistema armazenar o `CaminhoZipImportacao`. Ao tentar importar novamente o arquivo, o sistema silenciosamente ignorava o pedido pois ele já tinha o status de "Etiqueta vinculada", impedindo que a referência do ZIP fosse atualizada.
- **7 do Mercado Livre**: "Número do pedido não consta no ZPL". Isso ocorreu porque os pedidos do Mercado Livre que usam etiquetas no formato PDF estavam salvos no campo ZPL apenas com um ponteiro "PDF_MELI|3". O validador tentava ler o número do pedido *dentro dessa pequena string de ponteiro*, causando falha, já que o conteúdo real do PDF nunca era extraído nem analisado.

## Correções Implementadas

### 1. Migração Incremental (Idempotente)
Adicionadas duas novas colunas na tabela `Pedidos` de forma incremental e idempotente para salvar referências completas e definitivas de arquivos PDF:
- `NomePdfNoZip` (TEXT)
- `PaginaPdf` (INTEGER)
*(O campo `CaminhoZipImportacao` foi reaproveitado e passa a guardar o caminho do PDF direto do ML)*

### 2. Shopee: Cura Segura de Registros Antigos
No arquivo `VinculacaoEtiquetaService.cs`, adicionada lógica específica para **permitir que um pedido já vinculado seja curado**. Se o pedido já possui etiqueta mas está com `CaminhoZipImportacao` vazio, a reimportação é autorizada a atualizá-lo **somente se**:
1. O rastreio bater exatamente.
2. O pedido/página for comprovado.
3. Aquela página do PDF não estiver associada a nenhum outro pedido (proteção fail-closed).

### 3. Mercado Livre: Importação Segura e Validação de PDF
1. No `FrmPreparacaoPedidos.cs` (`ImportarPdfMercadoLivre`), a vinculação de pedidos ML via PDF agora preenche corretamente o caminho completo, em vez de deixar vazio, salvando tudo no banco. Adicionada a mesma **cura segura** de registros antigos, que preenche arquivos e páginas faltantes.
2. No `ValidacaoPreImpressaoService.cs`, adicionada uma rota específica (`else if (marketplaceNormalizado == "MERCADO LIVRE" && etiqueta.StartsWith("PDF_MELI|"))`) que:
   - Abre o PDF pelo caminho salvo no pedido.
   - Extrai fisicamente o texto da página usando `PdfPig`.
   - Gera o Hash real de segurança SHA-256 em cima da imagem ou texto.
   - Analisa se o número da venda e o código de rastreio existem de verdade dentro dessa página do documento.

## Como proceder agora?

**1. Reimportar os lotes antigos (Cura Segura):**
Para as 4 etiquetas da Shopee e 7 do Mercado Livre antigas, basta usar os botões "Importar ZIP Shopee" ou "Importar PDF/ZIP Meli" utilizando os mesmos arquivos que você usou originalmente. O sistema detectará as associações antigas incompletas e preencherá as colunas novas, mantendo a proteção administrativa (Impresso, Conferido, etc.) inalterada.

**2. Revalidar o dia:**
Após efetuar essa importação complementar ("cura"), clique novamente no botão "Validar Vínculos do Dia". As 11 inconsistências passarão a constar como válidas (ou continuarão bloqueadas se você anexar o arquivo errado).
