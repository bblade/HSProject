using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace HSProject.Controllers;

[ApiController, Route("/")]
public class AboutController(IWebHostEnvironment env) : ControllerBase {
    [HttpGet]
    public IActionResult About() {

        Version? version = Assembly.GetExecutingAssembly()?.GetName()?.Version;
        string versionString = version != null
            ? $"{version.Major}.{version.Minor}.{version.Build}"
            : "unknown";

        return Content($"ParcelReg is up and running.\n" +
               $"Version: {versionString}\n" +
               $"Environment: {env.EnvironmentName}", "text/plain; charset=utf-8");

    }

}
