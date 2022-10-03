using Utilities;

namespace NuGetUtility.Output.Table
{
    internal static class TablePrinterExtensions
    {
        public static TablePrinter Create(Stream stream, params string[] headings)
        {
            return new TablePrinter(stream, headings);
        }

        public static TablePrinter FromValues<T>(this TablePrinter printer,
            IEnumerable<T> values,
            Func<T, object[]> formatter)
        {
            foreach (var value in values)
            {
                printer.AddRow(formatter(value));
            }

            return printer;
        }
    }
}
