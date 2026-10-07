using System;
using Xunit;
using SistemaConferenciaPedidos.Services;

namespace SistemaConferenciaPedidos.Tests.Services
{
    public class NormalizacaoRastreioTests
    {
        [Theory]
        [InlineData("Pack ID: 2000014591536129 XYZ 47798792847", "47798792847")] // nenhum espaço
        [InlineData("Pack ID: 2000014591536129 XYZ 477987 92847", "47798792847")] // um espaço
        [InlineData("Pack ID: 2000014591536129 XYZ 477987   92847", "47798792847")] // múltiplos espaços
        [InlineData("Pack ID: 2000014591536129 XYZ 477987\t92847", "47798792847")] // tab
        [InlineData("Pack ID: 2000014591536129 XYZ 477987\r92847", "")] // CR quebra linha com texto (nova regra fallback)
        [InlineData("Pack ID: 2000014591536129 XYZ 477987\n92847", "")] // LF quebra linha com texto (nova regra fallback)
        [InlineData("Pack ID: 2000014591536129 ... 477987\u00A092847", "47798792847")] // espaço não separável
        [InlineData("Texto sem rastreio valido", "")] // zero código: falha
        [InlineData("Pack ID: 2000014591536129 ... 47798792847 e outro igual 477987 92847", "47798792847")] // mesmo código repetido: um candidato
        [InlineData("Pack ID: 2000014591536129 ... 47798792847 e diferente 47123456789", "")] // dois códigos diferentes: ambiguidade
        [InlineData("Um numero maior que tem 11 digitos mas eh maior 1234779879284756", "")] // número maior contendo 11 dígitos
        [InlineData("Resumo L32XFMJDJ5JKJM5EVRB6MOBFTI", "")] // identificador alfanumérico do resumo
        [InlineData("Zeros preservados 47000000000", "47000000000")] // zeros preservados
        [InlineData("Um numero de 12 digitos espacado: 4 7 7 9 8 7 9 2 8 4 7 0", "")] // 12 dígitos com espaço (falso positivo)
        [InlineData("Um numero de 12 digitos colado: 477987928470", "")] // 12 dígitos colado
        [InlineData("Um numero de 12 digitos antes: 0 47798792847", "")] // 12 dígitos espacado antes
        // --- NOVOS: BUG COMPROVADO pagina 1 do PDF real ---
        [InlineData("477987   92847\r\n02:30", "47798792847")] // BUG: rastreio fragmentado + horário na próxima linha
        [InlineData("Despachar 477987   92847\r\n02:30", "47798792847")] // BUG com prefixo Despachar
        // --- NOVOS: 12 dígitos espaçados devem ser rejeitados ---
        [InlineData("4 7 7 9 8 7 9 2 8 4 7 0\r\nseguinte", "")] // 12 dígitos espaçados na mesma linha: rejeitar
        // --- NOVOS: dois rastreios diferentes ---
        [InlineData("47798792847\n47123456789", "")] // dois rastreios diferentes = ambiguidade
        // --- NOVOS: mesmo rastreio repetido ---
        [InlineData("47798792847\nDespachar 47798792847", "47798792847")] // mesmo rastreio repetido = candidato único
        // --- NOVOS: fallback multilinhas NÃO pode unir com linha não-numérica ---
        [InlineData("477987\n02:30", "")] // não une com linha de horário (contém ':')
        [InlineData("477987\nDespachar", "")] // não une com linha de texto

        // --- ETAPA 2: TESTES DIRECIONADOS ---
        [InlineData("47798792847\r\n47798792847", "47798792847")] // 1. Mesmo rastreio repetido em linhas diferentes
        [InlineData("477987   92847\r\n477987 92847", "47798792847")] // 2. Mesmo rastreio repetido com espaços
        [InlineData("47798792847\r\n47798225685", "")] // 3. Dois rastreios diferentes
        [InlineData("477987   92847\r\n02:30", "47798792847")] // 4. Código seguido por horário
        [InlineData("477987\t92847\r\n02:30", "47798792847")] // 5. Tabulação
        [InlineData("477987\u00A092847\r\n02:30", "47798792847")] // 6. NBSP
        [InlineData("4 7 7 9 8 7 9 2 8 4 7 9", "")] // 7. Doze dígitos espaçados
        [InlineData("Pack ID: 20000 14591536129", "")] // 8. Candidato de 16 dígitos de Venda/Pack ID
        public void Deve_Extrair_Codigo_Etiqueta_Corretamente(string textoBruto, string esperado)
        {
            var service = new MercadoLivrePdfService();
            string extraido = service.ExtrairCodigoEtiqueta(textoBruto);
            Assert.Equal(esperado, extraido);
        }
    }
}
