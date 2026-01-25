using Application.Features.Containers;
using Application.Features.Containers.CreateContainer;
using Microsoft.AspNetCore.Mvc;
using ValidationException = Application.Exceptions.ValidationException;

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
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContainer([FromBody] CreateContainerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _containers.CreateAsync(command, cancellationToken);
            return CreatedAtAction(nameof(CreateContainer), new { id = result.ContainerId }, result);
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                foreach (var message in error.Value)
                {
                    ModelState.AddModelError(error.Key, message);
                }
            }
            return ValidationProblem(ModelState);
        }
    }
}
