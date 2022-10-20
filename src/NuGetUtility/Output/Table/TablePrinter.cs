// ReSharper disable once CheckNamespace

using NuGetUtility.Output.Table;

namespace Utilities
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

        public void AddRow(string?[] row)
        {
            if (row.Length != _titles.Length)
            {
                throw new Exception(
                    $"Added row length [{row.Length}] is not equal to title row length [{_titles.Length}]");
            }

            var rowElements = row.Select(item => SplitToLines(item?.ToString() ?? string.Empty).ToArray()).ToArray();
            for (var i = 0; i < _titles.Length; i++)
            {
                var maxLineLength = rowElements[i].Max(line => line.Length);
                if (maxLineLength > _lengths[i])
                {
                    _lengths[i] = maxLineLength;
                }
            }
            _rows.Add(rowElements);
        }

        public async Task Print()
        {
            await using var writer = new StreamWriter(_stream, leaveOpen: true);

            await WriteSeparator(writer);
            await WriteRow(_titles.Select(t => new []{t}).ToArray(), writer);
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
                for (var i = 0; i < values.Length; i++)
                {
                    var printLine = values[line].Length > i ? values[line][i] : string.Empty;
                    await writer.WriteAsync("| ");
                    await writer.WriteAsync(printLine.PadRight(_lengths[i]));
                    await writer.WriteAsync(' ');

                }
                await writer.WriteLineAsync("|");
            }
        }

        private async Task WriteSeparator(TextWriter writer)
        {
            foreach (var l in _lengths)
            {
                await writer.WriteAsync("+-" + new string('-', l) + '-');
            }
            await writer.WriteLineAsync("+");
        }

        /// <summary>
        /// Credit: https://stackoverflow.com/a/23408020/1199089
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static IEnumerable<string> SplitToLines(string input)
        {
            using var reader = new StringReader(input);
            while (reader.ReadLine() is { } line)
            {
                yield return line;
            }
        }
    }
}
