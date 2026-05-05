using Microsoft.AspNetCore.Mvc;
using ThomasPool.Application.Dtos;

namespace ThomasPool.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleFailure(Error error)
    {
        return Problem(
            statusCode: error.Type.ToStatusCode(),
            type: error.Code,
            detail: error.Message
        );
    }
}