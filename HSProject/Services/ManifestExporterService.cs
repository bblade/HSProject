using ClosedXML.Excel;

namespace HSProject.Services;
public class ManifestExporterService {

    public void ExportF46(string path, string format) {

        using XLWorkbook book = new(path);
        IXLWorksheet sheet = book.Worksheet(1);

        if (format == "f44" || format == "f46") {
            AddHeadersF46(sheet, format);
        } else {
            throw new NotImplementedException();
        }

        sheet.Columns().AdjustToContents();

        book.Save();
    }

    private static void AddHeadersF46(IXLWorksheet sheet, string format) {
        sheet.Row(1).InsertRowsAbove(1);
        sheet.Cell(1, 1).Value = "client_name";
        sheet.Cell(1, 2).Value = "agreement_no";
        sheet.Cell(1, 3).Value = "barcode";

        sheet.Cell(1, 4).Value = "sender_name";

        sheet.Cell(1, 5).Value = "receiver_name";

        sheet.Cell(1, 6).Value = "receiver_zip_code";
        sheet.Cell(1, 7).Value = "receiver_region";
        sheet.Cell(1, 8).Value = "receiver_district";
        sheet.Cell(1, 9).Value = "receiver_city";
        sheet.Cell(1, 10).Value = "receiver_street";
        sheet.Cell(1, 11).Value = "receiver_house";
        sheet.Cell(1, 12).Value = "receiver_building";
        sheet.Cell(1, 13).Value = "receiver_apartment";
        sheet.Cell(1, 14).Value = "receiver_mobile_phone_number";

        sheet.Cell(1, 15).Value = "tarif_euro";
        sheet.Cell(1, 16).Value = "tarif_euro_cents";

        sheet.Cell(1, 17).Value = "weight_of_the_parcel_kg";
        sheet.Cell(1, 18).Value = "weight_of_the_parcel_g";

        sheet.Cell(1, 19).Value = "type_of_service_code";
        sheet.Cell(1, 20).Value = "COD_amount_rur";
        sheet.Cell(1, 21).Value = "COD_amount_rur_kopeks";

        sheet.Cell(1, 22).Value = "no_of_the_product";
        sheet.Cell(1, 23).Value = "name_of_the_product";
        sheet.Cell(1, 24).Value = "quantity of identical items_of_product";
        sheet.Cell(1, 25).Value = "weight_of_the_product_kg";
        sheet.Cell(1, 26).Value = "weight_of_the_product_g";
        sheet.Cell(1, 27).Value = "product_value_eur";
        sheet.Cell(1, 28).Value = "product_value_eur_cents";

        sheet.Cell(1, 29).Value = "category";
        sheet.Cell(1, 30).Value = "comments";
        sheet.Cell(1, 31).Value = "invoice_id";

        sheet.Cell(1, 32).Value = "sender_country";
        sheet.Cell(1, 33).Value = "sender_zip_code";
        sheet.Cell(1, 34).Value = "sender_region";
        sheet.Cell(1, 35).Value = "sender_district";
        sheet.Cell(1, 36).Value = "sender_city";
        sheet.Cell(1, 37).Value = "sender_street";
        sheet.Cell(1, 38).Value = "sender_house";
        sheet.Cell(1, 39).Value = "sender_building";
        sheet.Cell(1, 40).Value = "sender_apartment";

        sheet.Cell(1, 41).Value = "client_id";
        sheet.Cell(1, 42).Value = "no_of_delivery_lot";
        sheet.Cell(1, 43).Value = "type_delivery_code";


        if (format == "f44") {
            sheet.Cell(1, 44).Value = "hs_code";
        } else if (format == "f46") {
            sheet.Cell(1, 44).Value = "*";
            sheet.Cell(1, 45).Value = "no_returns";
            sheet.Cell(1, 46).Value = "hs_code";
        }
    }
}
