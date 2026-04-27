using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace Coworking.WebApi.Controller;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiController : ControllerBase
{
    // Метод-помощник для обработки ошибок из ErrorOr
    protected IActionResult Problem(List<Error> errors)
    {
        var firstError = errors[0];

        var statusCode = firstError.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Problem(statusCode: statusCode, title: firstError.Description);
    }
}