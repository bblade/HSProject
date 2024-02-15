using ClosedXML.Excel;

using HSProject.ErrorHandling;
using HSProject.Models;

using System.Text;

namespace HSProject.Services;
public class ManifestImporterService(ILogger<ManifestExporterService> logger) {

    const string goodsFilename = @"goods.csv";
    const string parcelsFilename = @"parcels.csv";
    const string manifestFilename = @"manifest.csv";

    string format = "error";

    StringBuilder? parcelsBuilder;
    StringBuilder? goodsBuilder;
    StringBuilder? manifestBuilder;

    public async Task<ManifestImportOutputDto> Import(string path, string manifestId) {

        parcelsBuilder = new();
        goodsBuilder = new();
        manifestBuilder = new();

        using XLWorkbook book = new(path);
        IXLWorksheet sheet = book.Worksheet(1);


        format = CheckFormat(sheet);

        if (format == "error") {
            throw new ManifestInvalidException();
        }

        if (format != "f17") {
            ImportF46(sheet, manifestId);
        } else {
            throw new NotImplementedException();
        }

        string? folderPath = Path.GetDirectoryName(path);

        if (string.IsNullOrWhiteSpace(folderPath)) {
            folderPath = Path.GetTempPath();
        }

        string goodsPath = Path.Combine(folderPath, goodsFilename);
        string parcelsPath = Path.Combine(folderPath, parcelsFilename);
        string manifestPath = Path.Combine(folderPath, manifestFilename);

        logger.LogInformation("Manifest processed, writing files");

        await File.WriteAllTextAsync(goodsPath, goodsBuilder.ToString());
        await File.WriteAllTextAsync(parcelsPath, parcelsBuilder.ToString());
        await File.WriteAllTextAsync(manifestPath, manifestBuilder.ToString());
        
        logger.LogInformation("Files written ok");

        return new ManifestImportOutputDto(format, goodsPath, parcelsPath, manifestPath);
    }

    private void ImportF46(IXLWorksheet sheet, string manifestId) {

        if (manifestBuilder == null) {
            throw new Exception();
        }

        int lastRow = sheet.RowsUsed().Last().RowNumber();

        List<string> parcelBarcodes = sheet
            .Range($"C2:C{lastRow}")
            .CellsUsed()
            .Select(c => c.Value.ToString())
            .Distinct()
            .ToList();

        foreach (string barcode in parcelBarcodes.Where(b => !string.IsNullOrWhiteSpace(b))) {
            ProcessParcel(barcode,sheet, manifestId);
        }

        sheet.Cell(1, 47).SetValue("manifest_id");
        sheet.Cell(1, 48).SetValue("parcel_id");

        foreach (IXLRow? row in sheet.Rows()) {
            ProcessGoods(row);
        }

        manifestBuilder.Append(manifestId);
        manifestBuilder.Append(',');

        /*
         * 1 (A) Company Name
         * 2 (B) Agreement No
         * 41 (AO) Unused?
         * 44 (AR) Unused?
         */
        manifestBuilder
            .Append(sheet.Cell(2, 1).Value.ToCsv())
            .Append(',')
            .Append(sheet.Cell(2, 2).Value.ToCsv())
            .Append(',')
            .Append(sheet.Cell(2, 41).Value.ToCsv())
            .Append(',');

        string value44;
        if (format == "f44") {
            value44 = string.Empty;
        } else {
            value44 = sheet.Cell(2, 44).Value.ToCsv();
        }
        manifestBuilder.Append(value44);        
    }

    private string CheckFormat(IXLWorksheet sheet) {
        int columns = sheet.ColumnsUsed().Count();
        logger.LogInformation($"Detected {columns} columns");
        return columns switch {
            43 => "f44",
            44 => "f44",
            45 => "f46",
            46 => "f46",
            17 => "f17",
            _ => "error"
        };
    }

    private void ProcessGoods(IXLRow? row) {

        if (row == null) {
            return;
        }

        if (row.RowNumber() == 1) {
            return;
        }

        if (string.IsNullOrWhiteSpace(row.Cell(3).Value.ToString())) {
            return;
        }

        if (goodsBuilder == null) {
            throw new Exception();
        }

        /*
         * 22 (V) No
         * 23 (W) Name
         * 24 (X) Quantity
         * 25—26 (Y—Z) Weight
         * 27—28 (AA—AB) Value
         */
        for (int i = 22; i <= 28; i++) {
            goodsBuilder
                .Append(row.Cell(i).Value.ToCsv())
                .Append(',');
        }

        string hsCode;
        if (format == "f44") {
            hsCode = row.Cell(44).Value.ToCsv();
        } else {
            hsCode = row.Cell(46).Value.ToCsv();
        }

        goodsBuilder.Append(hsCode);

        /*
         * Manifest Id
         * Parcel Id
         */
        for (int i = 47; i <= 48; i++) {
            goodsBuilder
                .Append(',')
                .Append(row.Cell(i).Value.ToCsv());
            
        }
        goodsBuilder.AppendLine();
    }

    private void ProcessParcel(string barcode, IXLWorksheet sheet, string manifestId) {

        if (parcelsBuilder == null) {
            logger.LogError("Parcels builder not initialized");
            throw new Exception();
        }

        IXLRow? parcelRow = sheet.Rows()
                .Where(r => 
                    r.Cell(3).Value.ToString() == barcode)
                .FirstOrDefault();

        if (parcelRow == null) {
            return;
        }

        string parcelId = "DD041-" + Guid.NewGuid().ToString().ToUpperInvariant();

        parcelsBuilder
            .Append(parcelId)
            .Append(',')
            .Append(manifestId)
            .Append(',')
            .Append(barcode)
            .Append(',');

        /* 
         * 4 (D) Sender Name
         * 5—13 (E—M) Receiver Name, Address
         * 14 (N) Receiver Phone
         * 15—16 (O—P) Tariff
         * 17—18 (Q—R) Weight
         * 19 (S) Type of service
         * 20—21 (T—U) COD
         */
        for (int i = 4; i <= 21; i++) {
            parcelsBuilder
                .Append(parcelRow.Cell(i).Value.ToCsv())
                .Append(',');
        }

        /* 
         * 29 (AC) Category
         * 30 (AD) Comments
         * 31 (AE) Invoice ID
         * 32 — 40 (AF—AN) Sender Address
         */
        for (int i = 29; i <= 40; i++) {
            parcelsBuilder
                .Append(parcelRow.Cell(i).Value.ToCsv())
                .Append(',');
        }

        /* 
         * 42 (AP) Delivery Lot
         * 43 (AQ) Delivery Code
         */
        parcelsBuilder
            .Append(parcelRow.Cell(42).Value.ToCsv())
            .Append(',')
            .Append(parcelRow.Cell(43).Value.ToCsv())
            .Append(',');

        // 45 (AS) No returns
        string value45;

        if (format == "f44") {
            value45 = string.Empty;
        } else {
            value45 = parcelRow.Cell(45).Value.ToCsv();
        }

        parcelsBuilder
            .Append(value45)
            .AppendLine();

        foreach (var row in sheet.Rows().Where(r => r.Cell(3).Value.ToString() == barcode)) {
            row.Cell(47).SetValue(manifestId);
            row.Cell(48).SetValue(parcelId);
        }
    }
}
