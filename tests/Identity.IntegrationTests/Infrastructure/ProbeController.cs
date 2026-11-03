using Microsoft.AspNetCore.Mvc;

namespace Identity.IntegrationTests.Infrastructure
{
    /// <summary> Deliberately without [Authorize] or [AllowAnonymous]: only the fallback policy protects it. </summary>
    [ApiController]
    [Route("test/probe")]
    public sealed class ProbeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok();
    }
}
