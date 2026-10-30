using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AIQueryEngine.Clients;
using ModelContextProtocol.Server;


namespace AIQueryEngine.Tools
{
    public class SalesTools(SapApiClient sapApiClient)
    {
                    [McpServerTool, Description("""
            Consulta vendas no SAP. Use para qualquer pergunta sobre vendas, pedidos, faturamento ou clientes.
                
            Filtros disponíveis (todos opcionais, use apenas os que o usuário mencionar):
            - dataInicio / dataFim: quando pedir por período ("ontem", "janeiro", "essa semana")
            - codigoCliente: quando mencionar um cliente específico
            - filial: quando mencionar uma loja ou filial
            - status: ABERTO, FATURADO ou CANCELADO
                
            Exemplos do que o usuário pode perguntar:
            - "vendas de ontem" → só dataInicio e dataFim
            - "vendas do cliente 1234" → só codigoCliente
            - "pedidos em aberto de janeiro" → dataInicio, dataFim e status
            - "quanto o cliente X comprou esse mês na filial Y" → todos os filtros
            """)]
            public async Task<string> ConsultarVendas(
            [Description("Data inicial YYYYMMDD (opcional)")] string dataInicio = "",
            [Description("Data final YYYYMMDD (opcional)")] string dataFim = "",
            [Description("Código do cliente (opcional)")] string codigoCliente = "",
            [Description("Código da filial (opcional)")] string filial = "",
            [Description("Status: ABERTO, FATURADO, CANCELADO (opcional)")] string status = "")
            {
                try
                {
                    var condicoes = new List<string>();

                    // Converte YYYYMMDD → YYYY-MM-DD para o SAP
                    string FormatarData(string data) =>
                        data.Length == 8 ? $"{data[..4]}-{data[4..6]}-{data[6..8]}" : data;

                    if (!string.IsNullOrEmpty(dataInicio) && !string.IsNullOrEmpty(dataFim))
                        condicoes.Add($"T0.\"DocDate\" >= '{FormatarData(dataInicio)}' AND T0.\"DocDate\" <='{FormatarData(dataFim)}'");
                    else if (!string.IsNullOrEmpty(dataInicio))
                        condicoes.Add($"T0.\"DocDate\" = '{FormatarData(dataInicio)}'");              

                    if (!string.IsNullOrEmpty(codigoCliente))
                        condicoes.Add($"T0.\"CardCode\" = '{codigoCliente}'");

                    var where = condicoes.Any() ? string.Join(" AND ", condicoes) : "1=1";
                    
                    var query = $@"
            SELECT 
        COUNT(DISTINCT T0.""DocNum"") as ""QtdNotas"",
        SUM(T1.""LineTotal"") as ""ValorTotal"",
        COALESCE(SUM(T0.""DiscSum""), 0) as ""DescontoTotal""
    FROM OINV T0
    INNER JOIN INV1 T1 ON T0.""DocEntry"" = T1.""DocEntry""
    INNER JOIN OITM T4 ON T1.""ItemCode"" = T4.""ItemCode"" 
    WHERE {where} AND T0.""DocType"" = 'I' 
    AND (T4.""ItmsGrpCod"" = '104' OR T4.""ItmsGrpCod"" = '106')
    AND T0.""CANCELED"" = 'N'

        ";
                    Console.Error.WriteLine($"[ERRO] {query}");

                    var resultados = await sapApiClient.GetAsync(query);

                    Console.WriteLine($"[DEBUG] Tamanho da resposta do SAP (chars): {resultados?.Length ?? 0}");

                    return resultados ?? "[]";
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[ERRO] {ex}");
                    return $"Erro: {ex.Message}";
                }
                
            }

            public async Task<string> ConsultarVendasPorCliente(
        [Description("Data inicial YYYYMMDD (opcional)")] string dataInicio = "",
        [Description("Data final YYYYMMDD (opcional)")] string dataFim = "",
        [Description("Código do cliente (opcional)")] string codigoCliente = "",
        [Description("Código da filial (opcional)")] string filial = "",
        [Description("Status: ABERTO, FATURADO, CANCELADO (opcional)")] string status = "")
        {
            try
            {
                var condicoes = new List<string>();

                string FormatarData(string data) =>
                    data.Length == 8 ? $"{data[..4]}-{data[4..6]}-{data[6..8]}" : data;

                if (!string.IsNullOrEmpty(dataInicio) && !string.IsNullOrEmpty(dataFim))
                    condicoes.Add($"T0.\"DocDate\" >= '{FormatarData(dataInicio)}' AND T0.\"DocDate\" <='{FormatarData(dataFim)}'");
                else if (!string.IsNullOrEmpty(dataInicio))
                    condicoes.Add($"T0.\"DocDate\" = '{FormatarData(dataInicio)}'");

                if (!string.IsNullOrEmpty(codigoCliente))
                    condicoes.Add($"T0.\"CardCode\" = '{codigoCliente}'");

                var where = condicoes.Any() ? string.Join(" AND ", condicoes) : "1=1";

                // TOP 10 clientes por valor, pra evitar lista gigante indo pro prompt/resposta
                var query = $@"
            SELECT TOP 10
                T0.""CardCode"" as ""CodigoCliente"",
                T0.""CardName"" as ""NomeCliente"",
                COUNT(DISTINCT T0.""DocNum"") as ""QtdNotas"",
                SUM(T1.""LineTotal"") as ""ValorTotal""
            FROM OINV T0
            INNER JOIN INV1 T1 ON T0.""DocEntry"" = T1.""DocEntry""
            INNER JOIN OITM T4 ON T1.""ItemCode"" = T4.""ItemCode""
            WHERE {where} AND T0.""DocType"" = 'I' 
            AND (T4.""ItmsGrpCod"" = '104' OR T4.""ItmsGrpCod"" = '106') 
            AND T0.""CANCELED"" = 'N'
            GROUP BY T0.""CardCode"", T0.""CardName""
            ORDER BY SUM(T1.""LineTotal"") DESC
        ";

                Console.Error.WriteLine($"[DEBUG] Query clientes: {query}");

                var resultados = await sapApiClient.GetAsync(query);
                return resultados;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERRO] {ex}");
                return $"Erro: {ex.Message}";
            }
        }
    }
}