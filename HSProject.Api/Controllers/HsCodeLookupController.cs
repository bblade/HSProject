using HSProject.Api.Models;

using Microsoft.AspNetCore.Mvc;

namespace HSProject.Api.Controllers;

[ApiController, Route("api/lookup")]
public class HsCodeLookupController : ControllerBase {

    [HttpPost]
    public async Task<IActionResult> LookupCodes(IEnumerable<LookupDto> lookupList) {

    }
}
