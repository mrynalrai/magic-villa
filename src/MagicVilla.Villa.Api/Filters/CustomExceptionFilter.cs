using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MagicVilla.Villa.Api.Filters
{
    public class CustomExceptionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is FileNotFoundException fileNotFoundException)
            {
                context.Result = new ObjectResult("File Not found but handled in filter")
                {
                    StatusCode = 503
                };

                // This flags implies that the exception has already been handled, so no need to handle it again at "Default Exception Handler"
                // i.e, app.UseExceptionHandler("/ErrorHandling/ProcessError");
                context.ExceptionHandled = true;    
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {

        }
    }
}