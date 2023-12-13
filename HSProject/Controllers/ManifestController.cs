using HSProject.Models;
using HSProject.Services;

using Microsoft.AspNetCore.Mvc;

namespace HSProject.Controllers;

[ApiController]
[Route("api/import")]
public class ManifestController(ManifestImporterService manifestImporterService) : ControllerBase {

    [HttpPost]
    public IActionResult Import(ManifestImportDto manifestImportDto) {
        var text = manifestImporterService.Import(manifestImportDto.Path);
        return new ContentResult() {
            Content = text,
            ContentType = "text/plain;charset=utf-8",
            StatusCode = 200
        };
    }
}
