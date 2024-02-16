using ClosedXML.Excel;

using System.Text;

namespace HSProject.Services;
public static class ExcelExtensions {
    public static string ToCsv(this XLCellValue value) {
        string str = value.ToString();
        return StringToCsv(str);
    }

    public static string ToCsv(this string value) {
        return StringToCsv(value);
    }

    private static string StringToCsv(string value) {
        bool mustQuote =
            value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\r') ||
            value.Contains('\n');

        if (mustQuote) {
            StringBuilder sb = new();
            sb.Append('"');
            foreach (char nextChar in value) {
                sb.Append(nextChar);
                if (nextChar == '"') {
                    sb.Append('"');
                }
            }
            sb.Append('"');
            return sb.ToString();
        }

        return value;
    }
}
