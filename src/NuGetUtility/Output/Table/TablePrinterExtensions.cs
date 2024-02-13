// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

namespace NuGetUtility.Output.Table
{
    internal static class TablePrinterExtensions
    {
        public static TablePrinter Create(Stream stream, params string[] headings)
        {
            return new TablePrinter(stream, headings);
        }
        public static TablePrinter Create(Stream stream, IEnumerable<string> headings)
        {
            return new TablePrinter(stream, headings);
        }

        public static TablePrinter FromValues<T>(this TablePrinter printer,
            IEnumerable<T> values,
            Func<T, IEnumerable<object?>> formatter)
        {
            foreach (T? value in values)
            {
                printer.AddRow(formatter(value).ToArray());
            }

            return printer;
        }
    }
}
