using ClosedXML.Excel;

using System.Text;

namespace HSProject.Services;
public static class ExcelExtensions {
    public static string ToCsv(this XLCellValue value) {

        string str = value.ToString();

        bool mustQuote =
            str.Contains(',') ||
            str.Contains('"') ||
            str.Contains('\r') ||
            str.Contains('\n');

        if (mustQuote) {
            StringBuilder sb = new();
            sb.Append('"');
            foreach (char nextChar in str) {
                sb.Append(nextChar);
                if (nextChar == '"') {
                    sb.Append('"');
                }
            }
            sb.Append('"');
            return sb.ToString();
        }

        return str;
    }
}
