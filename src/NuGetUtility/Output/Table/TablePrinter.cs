// ReSharper disable once CheckNamespace

using System.Text;

namespace Utilities
{
    /// <summary>
    ///     Credits: https://stackoverflow.com/a/54943087/1199089
    /// </summary>
    public class TablePrinter
    {
        private readonly List<int> _lengths;
        private readonly List<string[]> _rows = new List<string[]>();
        private readonly Stream _stream;
        private readonly string[] _titles;

        public TablePrinter(Stream stream, params string[] titles)
        {
            _stream = stream;
            _titles = titles;
            _lengths = titles.Select(t => t.Length).ToList();
        }

        public void AddRow(params object?[] row)
        {
            if (row.Length != _titles.Length)
            {
                throw new Exception(
                    $"Added row length [{row.Length}] is not equal to title row length [{_titles.Length}]");
            }

            _rows.Add(row.Select(o => o?.ToString() ?? "").ToArray());
            for (var i = 0; i < _titles.Length; i++)
            {
                if (_rows.Last()[i].Length > _lengths[i])
                {
                    _lengths[i] = _rows.Last()[i].Length;
                }
            }
        }

        public async Task Print()
        {
            await using var writer = new StreamWriter(_stream, leaveOpen: true, encoding: Encoding.UTF8);

            await WriteSeparator(writer);
            await WriteRow(_titles, writer);
            await WriteSeparator(writer);

            foreach (var row in _rows)
            {
                await WriteRow(row, writer);
            }

            await WriteSeparator(writer);
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
