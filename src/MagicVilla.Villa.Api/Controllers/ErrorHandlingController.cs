using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
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
        public IActionResult ProcessError([FromServices] IHostEnvironment hostEnvironment) 
        {
            if (hostEnvironment.IsDevelopment())
            {
                var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();

                return Problem (
                    detail: feature.Error.StackTrace,
                    title: feature.Error.Message,
                    instance: hostEnvironment.EnvironmentName
                );
            }
            else
            {
                return Problem();
            }   
        } 
    }
}