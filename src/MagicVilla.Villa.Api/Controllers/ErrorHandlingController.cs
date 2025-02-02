using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla.Villa.Api.Controllers
{
    [Route("ErrorHandling")]
    [ApiController]
    [AllowAnonymous]
    // [ApiVersionNeutral]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorHandlingController: ControllerBase
    {
        [Route("ProcessError")]
        public IActionResult ProcessError() => Problem();
    }
}