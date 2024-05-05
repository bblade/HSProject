using HSProject.Api.Models;
using HSProject.Api.Services;

using Microsoft.AspNetCore.Mvc;

namespace HSProject.Api.Controllers;

[ApiController, Route("api/lookup")]
public class HsCodeLookupController(FileMakerService fileMakerService) : ControllerBase {

    [HttpPost]
    public async Task<IActionResult> LookupCodes(IEnumerable<LookupDto> lookupList) {
        var result = await fileMakerService.SubmitAsync(lookupList);
        return new ContentResult {
            Content = result,
            ContentType = "application/json"
        };
    }
}
