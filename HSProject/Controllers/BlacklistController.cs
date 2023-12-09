using HSProject.Models;
using HSProject.Services;

using Microsoft.AspNetCore.Mvc;

namespace HSProject.Controllers;

[ApiController]
[Route("api/check")]
public class BlacklistController(BlacklistService blacklistService) : ControllerBase {

    [HttpPost]
    public IActionResult Check([FromBody] BlacklistDto blacklistDto) {

        var outputDto = blacklistService.Check(blacklistDto);

        return Ok(outputDto);
    }

    [HttpGet]
    public IActionResult Compare([FromQuery] string text1, string text2) {
        var result = Comparer.CalculateLevenshteinDistance(text1, text2);
        return Ok(new {
            Difference = result
        });
    }
}
