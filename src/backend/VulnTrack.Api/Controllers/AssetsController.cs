using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VulnTrack.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AssetsController(IMediator mediator) : ControllerBase
{
    // Asset CQRS handlers will be wired here in subsequent iterations.
    [HttpGet]
    public IActionResult GetAll() => Ok(Array.Empty<object>());
}
