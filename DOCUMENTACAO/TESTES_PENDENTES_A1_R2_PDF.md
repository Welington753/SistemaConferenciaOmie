# Testes Pendentes (A1-R2) - Escopo PDF e Vinculação

As especificações abaixo foram retiradas da suíte de testes automáticos para evitar falso-positivo (Assert.True(true)), visto que seu comportamento depende da futura refatoração das lógicas de PDF e vinculação.

## Especificações

- **Mercado Livre PDF:** A rotina de extração e processamento específico do formato do Mercado Livre.
- **Página duplicada:** O PDF lido possui uma página duplicada; o sistema deve ignorar e não associar duas vezes ao mesmo pedido.
- **Venda/Pack ID ambíguo:** Múltiplas etiquetas apontam para cenários de incerteza que não podem escolher a primeira opção cegamente.
- **Pedido cancelado:** Pedidos ocultos ou cancelados no Omie/Banco não devem sofrer alteração ou vinculação.
- **Pedido de outra data:** A vinculação deve ignorar pedidos que pertençam a processamentos de dias anteriores.
- **Renderização incompatível:** A extração não deve corromper ao encarar PDFs de fontes estranhas.
- **Cópia atômica:** A importação e leitura do PDF deve garantir a atomicidade ao copiar o arquivo temporário.
- **Código extraído diferente:** Se a etiqueta do PDF difere da etiqueta original, o que fazer?
- **Duplo F2 entre processos:** Prevenir concorrência de pressionamento de teclas no WinForms.
- **Conferência com código duplicado:** Se a mesma etiqueta está em dois pedidos, bloquear ambos.
- **Impressora incorreta:** Tratamento de erros ao tentar usar uma impressora que não existe ou não está online.
- **Shopee e ZPL:** Etiqueta ZPL ou da Shopee deve ser preservada.
