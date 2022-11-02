namespace NuGetUtility.Output.Table
{
    /// <summary>
    ///     Credits: https://stackoverflow.com/a/54943087/1199089
    /// </summary>
    public class TablePrinter
    {
        private readonly int[] _lengths;
        private readonly List<string[][]> _rows = new List<string[][]>();
        private readonly Stream _stream;
        private readonly string[] _titles;

        public TablePrinter(Stream stream, IEnumerable<string> titles)
        {
            _stream = stream;
            _titles = titles.ToArray();
            _lengths = _titles.Select(t => t.Length).ToArray();
        }

        public void AddRow(object?[] row)
        {
            if (row.Length != _titles.Length)
            {
                throw new Exception(
                    $"Added row length [{row.Length}] is not equal to title row length [{_titles.Length}]");
            }

            var rowElements = row.Select(GetLines).ToArray();
            for (var i = 0; i < _titles.Length; i++)
            {
                var maxLineLength = rowElements[i].Any() ? rowElements[i].Max(line => line.Length) : 0;
                if (maxLineLength > _lengths[i])
                {
                    _lengths[i] = maxLineLength;
                }
            }
            _rows.Add(rowElements);
        }

        private string[] GetLines(object? lines)
        {
            if (lines is IEnumerable<object> enumerable)
            {
                return enumerable.Select(o => o.ToString() ?? string.Empty).ToArray();
            }
            return new[] { lines?.ToString() ?? string.Empty };
        }

        public async Task Print()
        {
            await using var writer = new StreamWriter(_stream, leaveOpen: true);

            await WriteSeparator(writer);
            await WriteRow(_titles, writer);
            await WriteSeparator(writer);

            foreach (var row in _rows)
            {
                await WriteRow(row, writer);
            }

            await WriteSeparator(writer);
        }

        private async Task WriteRow(string[][] values, TextWriter writer)
        {
            var maximumLines = values.Max(lines => lines.Length);
            for (var line = 0; line < maximumLines; line++)
            {
                await WriteRow(values.Select(v => v.Length > line ? v[line] : string.Empty).ToArray(), writer);
            }
        }

        private async Task WriteRow(string[] values, TextWriter writer)
        {
            for (var i = 0; i < values.Length; i++)
            {
                await writer.WriteAsync("| ");
                await writer.WriteAsync(values[i].PadRight(_lengths[i]));
                await writer.WriteAsync(' ');
            }
            await writer.WriteLineAsync("|");
        }

        private async Task WriteSeparator(TextWriter writer)
        {
            foreach (var l in _lengths)
            {
                await writer.WriteAsync("+-" + new string('-', l) + '-');
            }
            await writer.WriteLineAsync("+");
        }
    }
}
