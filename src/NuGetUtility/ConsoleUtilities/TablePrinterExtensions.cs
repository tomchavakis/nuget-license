using Utilities;

namespace NuGetUtility.ConsoleUtilities
{
    internal static class TablePrinterExtensions
    {
        public static TablePrinter Create(params string[] headings)
        {
            return new TablePrinter(headings);
        }

        public static TablePrinter FromValues<T>(this TablePrinter printer, IEnumerable<T> values,
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
