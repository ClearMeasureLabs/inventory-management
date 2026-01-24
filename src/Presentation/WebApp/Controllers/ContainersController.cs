using Application.Exceptions;
using Application.Features.Containers;
using Application.Features.Containers.CreateContainer;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Containers")]
public class ContainersController : ControllerBase
{
    private readonly IContainers _containers;

    public ContainersController(IContainers containers)
    {
        _containers = containers;
    }

    /// <summary>
    /// Creates a new container
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContainerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContainer([FromBody] CreateContainerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _containers.CreateAsync(command, cancellationToken);
            return CreatedAtAction(nameof(CreateContainer), new { id = result.ContainerId }, result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }
}
