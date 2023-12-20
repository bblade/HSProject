using HSProject.Models;
using HSProject.Services;

using Microsoft.AspNetCore.Mvc;

namespace HSProject.Controllers;

[ApiController, Route("api/import")]
public class ManifestImportController(ManifestImporterService manifestImporterService) : ControllerBase {

    [HttpPost]
    public async Task<IActionResult> Import(ManifestImportDto manifestImportDto) {
        ManifestImportOutputDto output = await manifestImporterService
            .Import(manifestImportDto.Path, manifestImportDto.ManifestId);
        return Ok(output);
    }
}
