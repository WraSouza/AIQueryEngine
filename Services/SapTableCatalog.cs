using ClosedXML.Excel;

namespace ServerMCP.Services
{
    public sealed class SapTableCatalog
    {
        private readonly HashSet<string> _tableCodes;
        private readonly string _catalogText;

        public SapTableCatalog(IWebHostEnvironment environment)
        {
            var filePath = Path.Combine(
                environment.ContentRootPath,
                "Data",
                "TABLES SAP B1 9.3.xlsx");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"Catálogo SAP B1 não encontrado: {filePath}");
            }

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);

            var rows = new List<TableEntry>();

            // A primeira linha contém os cabeçalhos.
            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var code = row.Cell(1).GetString().Trim();

                if (string.IsNullOrWhiteSpace(code))
                    continue;

                rows.Add(new TableEntry(
                    code,
                    row.Cell(2).GetString().Trim(),
                    row.Cell(3).GetString().Trim(),
                    row.Cell(4).GetString().Trim()));
            }

            if (rows.Count == 0)
            {
                throw new InvalidOperationException(
                    "O catálogo SAP B1 não contém tabelas.");
            }

            // A lista permitida contém códigos únicos.
            _tableCodes = rows
                .Select(x => x.Code)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Agrupa códigos repetidos, preservando as descrições.
            // Isso evita duplicações confusas no prompt do Gemini.
            _catalogText = string.Join(
                Environment.NewLine,
                rows
                    .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        var names = string.Join(
                            "; ",
                            g.Select(x => x.Name)
                             .Where(x => !string.IsNullOrWhiteSpace(x))
                             .Distinct(StringComparer.OrdinalIgnoreCase));

                        var descriptions = string.Join(
                            "; ",
                            g.Select(x => x.Description)
                             .Where(x => !string.IsNullOrWhiteSpace(x))
                             .Distinct(StringComparer.OrdinalIgnoreCase));

                        var keys = string.Join(
                            "; ",
                            g.Select(x => x.PrimaryKey)
                             .Where(x => !string.IsNullOrWhiteSpace(x))
                             .Distinct(StringComparer.OrdinalIgnoreCase));

                        return $"{g.Key} | {names} | {descriptions} | " +
                               $"Chave indicada: {keys}";
                    }));

            Console.WriteLine(
                $"Catálogo SAP B1 carregado: {rows.Count} registros, " +
                $"{_tableCodes.Count} códigos de tabela únicos.");
        }

        public bool ContainsTable(string tableCode)
        {
            return _tableCodes.Contains(tableCode);
        }

        public string GetCatalogText()
        {
            return _catalogText;
        }

        private sealed record TableEntry(
            string Code,
            string Name,
            string Description,
            string PrimaryKey);
    }
}
