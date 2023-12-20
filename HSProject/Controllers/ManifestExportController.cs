using HSProject.Models;
using HSProject.Services;

using Microsoft.AspNetCore.Mvc;

namespace HSProject.Controllers;

[ApiController, Route("api/export")]
public class ManifestExportController(ManifestExporterService manifestExporterService) : ControllerBase {

    [HttpPost]
    public IActionResult Index(ExportDto exportDto) {

        string path = exportDto.Path;
        manifestExporterService.Export(path);

        FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);

        return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}
