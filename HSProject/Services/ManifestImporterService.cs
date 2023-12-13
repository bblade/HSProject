using ClosedXML.Excel;

using System.Text;

namespace HSProject.Services;
public class ManifestImporterService {
    public string Import(string path) {

        using var book = new XLWorkbook(path);

        var sheet = book.Worksheet(1);

        StringBuilder sb = new();


        foreach (var row in sheet.Rows()) {
            for (int i = 1; i <= 43; i++) {
                sb.Append(sheet.Cell(row.RowNumber(), i).Value);
                sb.Append(',');
            }
            sb.Length--;
            sb.Append('\n');
        }

        return sb.ToString();
    }
}
