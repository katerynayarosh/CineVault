using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.API.Controllers;
[Route("api/v{v:apiVersion}")]
[ApiVersion(1)]
[ApiVersion(2)]
[ApiController]
public class AppInfoController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public AppInfoController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("environment")]
    public ActionResult<string> GetEnvironment()
    {
        var environment = new { EnvironmentName = _environment.EnvironmentName };

        return Ok(environment);
    }

    [HttpGet("throw-exception")]
    public ActionResult ThrowException()
    {
        throw new NotImplementedException("This method should never be called");
    }

    [MapToApiVersion(1)]
    [HttpGet("old")]
    public string OldTestMethod()
    {
        return "This is old api method for version 1";
    }

    [MapToApiVersion(2)]
    [HttpGet("new")]
    public string NewTestMethod()
    {
        return "This is new api method for version 2";
    }
}
