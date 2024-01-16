using HSProject.Models;
using HSProject.Services;

using Microsoft.AspNetCore.Mvc;

namespace HSProject.Controllers;

[ApiController, Route("api/check")]
public class BlacklistController(BlacklistService blacklistService) : ControllerBase {

    [HttpPost]
    public IActionResult Check([FromBody] InputDto blacklistDto) {

        OutputDto outputDto = blacklistService.Check(blacklistDto);

        return Ok(outputDto);
    }

    [HttpPost("v2")]
    public IActionResult CheckV2([FromBody] InputDto blacklistDto) {

        var output = blacklistService.CheckV2(blacklistDto);

        return Ok(output);
    }

    [HttpGet]
    public IActionResult Compare([FromQuery] string text1, string text2) {
        var result = Comparer.CalculateLevenshteinDistance(text1, text2);
        return Ok(new {
            Difference = result
        });
    }
}
