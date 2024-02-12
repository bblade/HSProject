﻿using ClosedXML.Excel;

using HSProject.ErrorHandling;
using HSProject.Models;

using System.Text;

namespace HSProject.Services;
public class ManifestImporterService(ILogger<ManifestExporterService> logger) {

    const string goodsFilename = @"goods.csv";
    const string parcelsFilename = @"parcels.csv";
    const string manifestFilename = @"manifest.csv";

    public async Task<ManifestImportOutputDto> Import(string path, string manifestId) {

        using XLWorkbook book = new(path);
        IXLWorksheet sheet = book.Worksheet(1);


        if (sheet.Columns().Count() < 43) {
            logger.LogDebug(sheet.Columns().Count().ToString());
            throw new ManifestInvalidException();
        }


        int lastRow = sheet.Rows().Last().RowNumber();

        var parcelBarcodes = sheet
            .Range($"C2:C{lastRow}")
            .Cells()
            .Select(c => c.Value.ToString())
            .Distinct()
            .ToList();

        StringBuilder parcelsBuilder = new();

        foreach (string barcode in parcelBarcodes.Where(b => !string.IsNullOrWhiteSpace(b))) {
            IXLRow? parcelRow = sheet.Rows()
                .Where(r => r.Cell(3).Value.ToString() == barcode)
                .FirstOrDefault();

            if (parcelRow == null) {
                break;
            }

            string parcelId = "DD041-" + Guid.NewGuid().ToString().ToUpperInvariant();

            parcelsBuilder.Append(parcelId);
            parcelsBuilder.Append(',');
            parcelsBuilder.Append(manifestId);
            parcelsBuilder.Append(',');
            parcelsBuilder.Append(barcode);
            parcelsBuilder.Append(',');

            for (int i = 4; i <= 21; i++) {
                parcelsBuilder.Append(parcelRow.Cell(i).Value.ToCsv());
                parcelsBuilder.Append(',');
            }

            for (int i = 29; i <= 40; i++) {
                parcelsBuilder.Append(parcelRow.Cell(i).Value.ToCsv());
                parcelsBuilder.Append(',');
            }

            parcelsBuilder.Append(parcelRow.Cell(42).Value.ToCsv());
            parcelsBuilder.Append(',');
            parcelsBuilder.Append(parcelRow.Cell(43).Value.ToCsv());
            parcelsBuilder.AppendLine();

            foreach (var row in sheet.Rows().Where(r => r.Cell(3).Value.ToString() == barcode)) {
                row.Cell(47).SetValue(manifestId);
                row.Cell(48).SetValue(parcelId);
            }
        }

        sheet.Cell(1, 47).SetValue("manifest_id");
        sheet.Cell(1, 48).SetValue("parcel_id");

        StringBuilder goodsBuilder = new();

        foreach (var row in sheet.Rows()) {
            if (row.RowNumber() == 1) {
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.Cell(3).Value.ToString())) {
                break;
            }

            for (int i = 22; i <= 28; i++) {
                goodsBuilder.Append(row.Cell(i).Value.ToCsv());
                goodsBuilder.Append(',');
            }
            for (int i = 46; i <= 48; i++) {
                goodsBuilder.Append(row.Cell(i).Value.ToCsv());
                goodsBuilder.Append(',');
            }
            goodsBuilder.AppendLine();
        }

        StringBuilder manifestBuilder = new();

        manifestBuilder.Append(manifestId);
        manifestBuilder.Append(',');
        manifestBuilder.Append(sheet.Cell(2, 1).Value.ToCsv());
        manifestBuilder.Append(',');
        manifestBuilder.Append(sheet.Cell(2, 2).Value.ToCsv());
        manifestBuilder.Append(',');
        manifestBuilder.Append(sheet.Cell(2, 41).Value.ToCsv());

        string? folderPath = Path.GetDirectoryName(path);

        if (string.IsNullOrWhiteSpace(folderPath)) {
            folderPath = Path.GetTempPath();
        }

        string goodsPath = Path.Combine(folderPath, goodsFilename);
        string parcelsPath = Path.Combine(folderPath, parcelsFilename);
        string manifestPath = Path.Combine(folderPath, manifestFilename);

        await File.WriteAllTextAsync(goodsPath, goodsBuilder.ToString());
        await File.WriteAllTextAsync(parcelsPath, parcelsBuilder.ToString());
        await File.WriteAllTextAsync(manifestPath, manifestBuilder.ToString());

        return new ManifestImportOutputDto(goodsPath, parcelsPath, manifestPath);
    }
}
