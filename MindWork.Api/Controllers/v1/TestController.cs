using Microsoft.AspNetCore.Mvc;

namespace MindWork.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/test")]
    [ApiVersion("1.0")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() =>
            Ok(new { message = "API v1 funcionando!" });
    }
}
