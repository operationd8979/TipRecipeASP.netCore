using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace TipRecipe.Controllers
{
    public class ErrorController : ControllerBase
    {
        [Route("/error-development")]
        public IActionResult HandleErrorDevelopment(
            [FromServices] IHostEnvironment hostEnvironment)
        {
            if (!hostEnvironment.IsDevelopment())
            {
                return NotFound();
            }
            var exceptionHandlerFeature =
                HttpContext.Features.Get<IExceptionHandlerFeature>()!;

            if (exceptionHandlerFeature != null)
            {
                Log.Error(exceptionHandlerFeature.Error, "Unhandled exception occurred in development environment");
                return Problem(
                    detail: exceptionHandlerFeature.Error.StackTrace,
                    title: exceptionHandlerFeature.Error.Message);
            }

            return Problem();
        }


        [Route("/error")]
        public IActionResult HandleError()
        {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandlerFeature != null)
            {
                Log.Error(exceptionHandlerFeature.Error, "Unhandled exception occurred in production environment");
            }

            return Problem();
        }
    }
}
