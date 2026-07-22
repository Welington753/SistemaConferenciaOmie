# Diagnóstico de Páginas Cruzadas do Mercado Livre

Abaixo está o relatório técnico detalhando por que 7 pedidos do Mercado Livre tiveram suas páginas associadas incorretamente, bem como a causa raiz do problema e como a correção implementada previne isso.

## Causa Raiz

A vinculação original de páginas do Mercado Livre importadas a partir de um PDF (ZPL/PDF_MELI) ocorria iterando os pedidos e buscando no PDF pelo `NumeroVenda` (16 dígitos).

1. O `NumeroVenda` era extraído pelo pacote PDF iterando as páginas e aplicando regex.
2. Contudo, em algumas páginas de etiqueta do Mercado Livre, o campo explícito "VENDA: 20000..." não está presente (apenas o Pack ID ou outras referências).
3. Quando a extração de `NumeroVenda` não conseguia um vínculo unívoco com a página, a rotina de montagem de dicionários `etiquetasPorVenda = ... GroupBy(NumeroVenda).First()` acabava por assumir a **primeira** página que calhasse ter aquele número correspondido via fallback ou páginas de resumo, descartando a página correta da etiqueta.
4. Isso gerava um *off-by-one* ou desalinhamento: o dicionário dizia que o Pedido A estava na página X, quando na verdade X pertencia ao Pedido B.

## A Solução Implementada

Para resolver este problema e evitar cruzamentos:

1. **Uso do `CodigoEtiqueta`:** Todo pedido importado do Omie ou ZPL já possui ou receberá um `CodigoEtiqueta` unívoco (ex: `47000...`).
2. **Nova Vinculação:** Durante a reimportação do arquivo PDF, em vez de confiarmos apenas no frágil `NumeroVenda`, extraímos o `CodigoEtiqueta` diretamente de cada página do PDF.
3. **Mapeamento Exato:** O sistema agora cruza o `CodigoEtiqueta` que já consta no banco para o pedido com o `CodigoEtiqueta` encontrado no PDF. Se houver exatamente **uma** correspondência, a associação é garantida.

Este método é imune a falhas de extração de Número de Venda ou Pack ID, pois o código de rastreio de 11 dígitos está sempre presente e de forma destacada no código de barras da etiqueta.
