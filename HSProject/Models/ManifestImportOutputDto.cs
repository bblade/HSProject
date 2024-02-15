namespace HSProject.Models;
public record struct ManifestImportOutputDto(
    string Format,
    string GoodsPath,
    string ParcelsPath,
    string ManifestPath);