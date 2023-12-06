using HSProject.Models;
using HSProject.Services;

using Microsoft.AspNetCore.Mvc;

namespace HSProject.Controllers;
public class BlacklistController(BlacklistService blacklistService) : ControllerBase {

    [HttpPost("api/check")]
    public IActionResult Check([FromBody] BlacklistDto blacklistDto) {

        var outputDto = blacklistService.Check(blacklistDto);

        return Ok(outputDto);
    }

    [HttpPost("api/check2")]
    public IActionResult Check2([FromBody] BlacklistDto blacklistDto) {

        var outputDto = blacklistService.Check2(blacklistDto);

        return Ok(outputDto);
    }
}
