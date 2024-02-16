using ClosedXML.Excel;

using DocumentFormat.OpenXml.Spreadsheet;

using HSProject.ErrorHandling;
using HSProject.Models;

using Microsoft.Extensions.Primitives;

using System.Globalization;
using System.Text;

namespace HSProject.Services;
public class ManifestImporterService(ILogger<ManifestExporterService> logger) {

    const string goodsFilename = @"goods.csv";
    const string parcelsFilename = @"parcels.csv";
    const string manifestFilename = @"manifest.csv";

    string format = "error";
    string? manifestId;

    StringBuilder? parcelsBuilder;
    StringBuilder? goodsBuilder;
    StringBuilder? manifestBuilder;

    public async Task<ManifestImportOutputDto> Import(string path, string manifestId) {

        parcelsBuilder = new();
        goodsBuilder = new();
        manifestBuilder = new();
        this.manifestId = manifestId;

        using XLWorkbook book = new(path);
        IXLWorksheet sheet = book.Worksheet(1);


        format = CheckFormat(sheet);

        if (format == "error") {
            throw new ManifestInvalidException();
        }

        if (format != "f17") {
            ImportF46(sheet);
        } else {
            ImportF17(sheet);
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

    private void ImportF46(IXLWorksheet sheet) {

        if (manifestBuilder == null) {
            throw new Exception();
        }

        if (manifestId == null) {
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
            ProcessParcelsF46(barcode, sheet);
        }

        foreach (IXLRow? row in sheet.RowsUsed()) {
            ProcessGoodsF46(row);
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
        int columns = sheet.ColumnsUsed().Last().ColumnNumber();
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

    private void ProcessGoodsF46(IXLRow? row) {

        if (row == null) {
            return;
        }

        if (manifestId == null) {
            throw new Exception();
        }

        if (row.RowNumber() == 1) {
            return;
        }

        if (string.IsNullOrWhiteSpace(row.Cell(3).Value.ToString())) {
            return;
        }

        goodsBuilder ??= new();

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

        goodsBuilder
            .Append(hsCode)
            .Append(',')
            .Append(manifestId)
            .Append(',')
            .Append(row.Cell(47).Value.ToCsv()) // parcel_id
            .AppendLine();

    }

    private void ProcessParcelsF46(string barcode, IXLWorksheet sheet) {

        parcelsBuilder ??= new();

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

        foreach (var row in sheet.Rows()
            .Where(r => r.Cell(3).Value.ToString() == barcode)) {

            row.Cell(47).SetValue(parcelId);
        }
    }

    private void ImportF17(IXLWorksheet sheet) {

        manifestBuilder ??= new();

        if (manifestId == null) {
            throw new Exception("Manifest Id is not set");
        }

        const int firstRow = 16;
        int lastRow = sheet.RowsUsed().Last().RowNumber();

        List<string> parcelBarcodes = sheet
            .Range($"B{firstRow}:B{lastRow}")
            .CellsUsed()
            .Select(c => c.Value.ToString())
            .Distinct()
            .ToList();

        foreach (string barcode in parcelBarcodes
            .Where(b => !string.IsNullOrWhiteSpace(b))) {

            ProcessParcelsF17(barcode, sheet);
        }

        sheet.Cell(1, 18).SetValue("parcel_id");

        for (int i = firstRow; i <= lastRow; i++) {
            ProcessGoodsF17(sheet.Row(i));
        }

        manifestBuilder.Append(manifestId);
        manifestBuilder.Append(',');

        /*
         * 1 (A) MAWB Number
         * 5 (E) Shipper Name
         * 13 (M) Unused?
         */
        manifestBuilder
            .Append(sheet.Cell(firstRow, 1).Value.ToCsv())
            .Append(',')
            .Append(sheet.Cell(firstRow, 5).Value.ToCsv())
            .Append(',')
            .Append(sheet.Cell(firstRow, 13).Value.ToCsv());

    }

    private void ProcessParcelsF17(string barcode, IXLWorksheet sheet) {

        parcelsBuilder ??= new();

        IXLRow? parcelRow = sheet.Rows()
                .Where(r =>
                    r.Cell(2).Value.ToString() == barcode)
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

        // 3 (C) Expected Arrival Date
        try {
            DateTime arrivalDate = parcelRow.Cell(3).Value.GetDateTime();
            parcelsBuilder.Append(arrivalDate.ToShortDateString());
        } catch {
            parcelsBuilder.Append(parcelRow.Cell(3).Value.ToCsv());
        }

        /* 
         * 4 (D) Currency
         * 6 (F) Consignee Family Name
         * 7 (G) Consignee Name
         * 8 (H) Consignee Middle Name
         * 9 (I) Full Address
         * 10 (J) City
         * 11 (K) State
         */
        parcelsBuilder
            .Append(',')
            .Append(parcelRow.Cell(4).Value.ToCsv());

        for (int i = 6; i <= 11; i++) {
            parcelsBuilder
                .Append(',')
                .Append(parcelRow.Cell(i).Value.ToCsv());
        }

        parcelsBuilder.AppendLine();

        foreach (var row in sheet.Rows().Where(r => r.Cell(2).Value.ToString() == barcode)) {
            row.Cell(18).SetValue(parcelId);
        }
    }

    private void ProcessGoodsF17(IXLRow? row) {

        if (row == null) {
            return;
        }

        if (manifestId == null) {
            throw new Exception();
        }

        if (row.RowNumber() == 1) {
            return;
        }

        if (string.IsNullOrWhiteSpace(row.Cell(3).Value.ToString())) {
            return;
        }

        goodsBuilder ??= new();

        /*
         * 12 (L) Item Description (EN)
         * 14 (N) Item Description
         * 15 (O) Quantity
         * 16 (P) Weight
         * 17 (Q) Value
         */

        goodsBuilder
            .Append(row.Cell(12).Value.ToCsv())
            .Append(',')
            .Append(row.Cell(14).Value.ToCsv())
            .Append(',')
            .Append(row.Cell(15).Value.ToCsv())
            .Append(',')
            .Append(row.Cell(16).Value.ToString().ToCsv())
            .Append(',')
            .Append(row.Cell(17).Value.ToString().ToCsv())
            .Append(',')
            .Append(manifestId)
            .Append(',')
            .Append(row.Cell(18).Value.ToCsv()) // parcel_id
            .AppendLine();

    }
}
