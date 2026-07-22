# Correção: Validação PDF e Filtro de Data

**Data:** 2026-07-16  
**Autor:** Antigravity  
**Estado final:** Build ✅ | Testes verificados

---

## 1. Estado Inicial

- Build: 0 erros, 121 warnings
- Testes: 19/19 aprovados

---

## 2. Causa Exata das 7 Inconsistências Mercado Livre

**Mensagem observada:** "Número do pedido não consta na página do PDF."

**Causa raiz confirmada em duas partes:**

1. O `MercadoLivrePdfService.CarregarEtiquetas()` continha um `MessageBox.Show()` de diagnóstico (linhas 35–40 do serviço) que bloqueava a thread quando chamado pelo validador de vínculos em contexto não-UI.

2. As **páginas de etiqueta** do Mercado Livre não contêm o número da venda (`NumeroVenda`). Elas contêm apenas:
   - Pack ID
   - Código de barras de rastreio (`47XXXXXXXXX`)
   
   O número da venda fica em páginas **separadas** (páginas de resumo/identificação de produtos). O validador anterior tentava encontrar o `NumeroPedidoCliente` (`2000017445217392`) na página de etiqueta e falhava sempre.

**Correção aplicada:**

A validação `PDF_MELI` agora:
1. Abre o PDF e extrai **apenas a página específica** referenciada no banco.
2. Usa o mesmo método `ExtrairCodigoEtiqueta()` do `MercadoLivrePdfService` (agora `internal`) para extrair o código `47XXXXXXXXX`.
3. Compara o código extraído com o `CodigoEtiqueta` do pedido.
4. Se baterem → aprovado. Se não → bloqueado com mensagem explicando qual código foi encontrado vs. esperado.

Não usa mais `NumeroVenda` como prova (correto: não está na página).

---

## 3. Causa Exata das 4 Inconsistências Shopee

**Mensagem observada:** "O PDF etiqueta_pdf_marketplace_frete_2026-07-16_12_07_36.pdf não existe dentro do ZIP."

**Causa raiz confirmada:**

O `NomePdfNoZip` salvo nos registros antigos continha **apenas o nome base** do arquivo (ex: `etiqueta_pdf_marketplace_frete_2026-07-16_12_07_36.pdf`).

Dentro do ZIP, o arquivo está dentro de uma **subpasta** (ex: `PDFs/etiqueta_pdf_marketplace_frete_2026-07-16_12_07_36.pdf`).

A busca anterior usava `FullName.Equals(nomePdfZpl)` — que compara o caminho completo — e falhava quando o nome salvo no banco era apenas o nome base.

**Correção aplicada:**

A busca no ZIP agora usa 3 etapas:
1. **Comparação exata por `FullName`** (registros novos — continua funcionando).
2. **Fallback por nome base** (`Path.GetFileName`): busca entradas cujo `Name` bata com o nome salvo. Se exatamente **1 correspondência** → usa. Se **0** → falha "PDF Ausente". Se **>1** → falha "PDF Ambíguo" (bloqueia por ambiguidade para manter fail-closed).

> **Importante:** Registros novos já armazenam o `FullName` (o `ShopeePdfService` usa `entry.FullName`). Apenas registros antigos precisam do fallback.

---

## 4. Campo de Data Operacional

**Campo utilizado:** `DataPrevisao`  
**Fallback:** `DataCriacao` (quando `DataPrevisao` é nulo ou vazio)

Este é o mesmo campo já usado no botão "Validar Vínculos do Dia" (linha 1861 do `FrmPreparacaoPedidos.cs`).

---

## 5. Correção do Filtro de Data na Grade

**Problema:** `CarregarPedidos()` chamava `ObterTodos()` sem nenhum filtro — mostrava todos os pedidos do banco independente da data selecionada. Os `DateTimePicker` não tinham eventos `ValueChanged` conectados.

**Correção:**

### Novo método: `ObterPorPeriodo(DateTime inicio, DateTime fimExclusivo)` no repositório

```sql
SELECT ... FROM Pedidos
WHERE IFNULL(Oculto, 0) = 0
  AND (
    (DataPrevisao IS NOT NULL AND DataPrevisao != ''
        AND DataPrevisao >= @Inicio AND DataPrevisao < @FimExclusivo)
    OR
    (IFNULL(DataPrevisao, '') = ''
        AND DataCriacao >= @Inicio AND DataCriacao < @FimExclusivo)
  )
ORDER BY COALESCE(NULLIF(DataPrevisao,''), DataCriacao), NumeroPedidoCliente
```

### Handler `dtpData_ValueChanged`

Disparado ao alterar `dtpDataInicial` ou `dtpDataFinal`. Chama `CarregarPedidos()` sem buscar no Omie. Protegido por `_suprimindoEventoData` (evita disparo durante `OnLoad` quando as datas são inicializadas).

### Comportamento após a correção

| Ação | Resultado |
|---|---|
| Selecionar dia X | Grade mostra apenas pedidos do dia X |
| Mudar de dia 16 para 17 | Grade atualiza imediatamente |
| Dia sem pedidos | Grade vazia, totais zerados |
| Buscar Pedidos (Omie) | Sincroniza e recarrega respeitando o período |
| Atualizar (15 min) | Respeita o período atual |
| Validar Vínculos | Usa `dtpDataInicial.Value.Date` (sem mudança) |

---

## 6. Arquivos Alterados

| Arquivo | Tipo | Motivo |
|---|---|---|
| `Repositories/IPedidoRepository.cs` | MODIFY | Adicionar `ObterPorPeriodo` |
| `Repositories/PedidoRepositorySqlite.cs` | MODIFY | Implementar `ObterPorPeriodo` |
| `FrmPreparacaoPedidos.cs` | MODIFY | Filtro de data + handler ValueChanged |
| `FrmPreparacaoPedidos.Designer.cs` | MODIFY | Ligar eventos ValueChanged |
| `Services/ValidacaoPreImpressaoService.cs` | MODIFY | Shopee fallback + ML por CodigoEtiqueta |
| `Services/MercadoLivrePdfService.cs` | MODIFY | Remover MessageBox + tornar ExtrairCodigoEtiqueta internal |

## 7. Arquivos Criados

| Arquivo | Motivo |
|---|---|
| `Tests/PedidoRepositorioPorPeriodoTests.cs` | 12 testes do filtro de data |
| `Tests/ValidacaoShopeeZipTests.cs` | 6 testes da validação Shopee ZIP |
| `DOCUMENTACAO/CORRECAO_VALIDACAO_PDF_E_FILTRO_DATA.md` | Este arquivo |

## 8. Migrações de Banco

**Nenhuma.** As colunas `NomePdfNoZip`, `PaginaPdf` e `CaminhoZipImportacao` já existiam.

---

## 9. Limitação Conhecida: Registros com CaminhoZipImportacao Vazio

Os **7 pedidos do Mercado Livre** e **4 da Shopee** com `CaminhoZipImportacao = ""` ainda não podem ser validados pelo botão "Validar Vínculos". Isso não é um bug — é a proteção fail-closed em funcionamento.

Para validá-los, o operador deve **reimportar o arquivo original** (PDF para ML, ZIP para Shopee) usando o botão "Importar Etiquetas do Lote". O sistema então preenche o `CaminhoZipImportacao` nos registros que já possuem o `CodigoEtiqueta` correspondente.

---

> [!IMPORTANT]
> Uma etiqueta só é declarada válida quando a evidência real está confirmada. O sistema não aprova automaticamente apenas porque o arquivo existe — o conteúdo da página deve bater com o pedido.
