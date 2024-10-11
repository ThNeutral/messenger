using Microsoft.AspNetCore.Mvc;

namespace server.Controllers
{
    public abstract class AbstractController : ControllerBase
    {
        protected IActionResult InternalServerError(object? value)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, value);
        }

        protected IActionResult Forbidden(object? value)
        {
            return StatusCode(StatusCodes.Status403Forbidden, value);
        }
    }
}
