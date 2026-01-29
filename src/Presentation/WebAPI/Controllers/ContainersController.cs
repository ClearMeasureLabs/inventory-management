using Application;
using Application.Features.Containers;
using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.DeleteContainer;
using Application.Features.Containers.UpdateContainer;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Contracts;
using ValidationException = Application.Exceptions.ValidationException;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Containers")]
public class ContainersController : ControllerBase
{
    private readonly IApplication _application;

    public ContainersController(IApplication application)
    {
        _application = application;
    }

    /// <summary>
    /// Gets all containers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContainerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllContainers(CancellationToken cancellationToken)
    {
        var containers = await _application.Containers.GetAllAsync(cancellationToken);
        var response = containers.Select(ContainerResponse.FromDto);
        return Ok(response);
    }

    /// <summary>
    /// Gets a container by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContainerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContainerById(int id, CancellationToken cancellationToken)
    {
        var container = await _application.Containers.GetByIdAsync(id, cancellationToken);

        if (container is null)
        {
            return NotFound();
        }

        var response = ContainerResponse.FromDto(container);
        return Ok(response);
    }

    /// <summary>
    /// Creates a new container.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContainerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContainer([FromBody] CreateContainerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateContainerCommand
            {
                Name = request.Name,
                Description = request.Description
            };

            var result = await _application.Containers.CreateAsync(command, cancellationToken);
            var response = ContainerResponse.FromDto(result);
            
            return CreatedAtAction(nameof(GetAllContainers), new { id = response.ContainerId }, response);
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

    /// <summary>
    /// Deletes a container by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteContainer(int id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteContainerCommand
            {
                ContainerId = id
            };

            await _application.Containers.DeleteAsync(command, cancellationToken);
            
            return NoContent();
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

    /// <summary>
    /// Updates an existing container.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ContainerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateContainer(int id, [FromBody] UpdateContainerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateContainerCommand
            {
                ContainerId = id,
                Name = request.Name,
                Description = request.Description
            };

            var result = await _application.Containers.UpdateAsync(command, cancellationToken);
            var response = ContainerResponse.FromDto(result);
            
            return Ok(response);
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
