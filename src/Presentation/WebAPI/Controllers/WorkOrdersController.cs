using Application.Features.WorkOrders;
using Application.Features.WorkOrders.CreateWorkOrder;
using Application.Features.WorkOrders.DeleteWorkOrder;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Contracts;
using ValidationException = Application.Exceptions.ValidationException;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("WorkOrders")]
public class WorkOrdersController : ControllerBase
{
    private readonly IWorkOrders _workOrders;

    public WorkOrdersController(IWorkOrders workOrders)
    {
        _workOrders = workOrders;
    }

    /// <summary>
    /// Gets all work orders.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkOrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllWorkOrders(CancellationToken cancellationToken)
    {
        var workOrders = await _workOrders.GetAllAsync(cancellationToken);
        var response = workOrders.Select(WorkOrderResponse.FromDto);
        return Ok(response);
    }

    /// <summary>
    /// Creates a new work order.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WorkOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWorkOrder([FromBody] CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateWorkOrderCommand
            {
                Title = request.Title
            };

            var result = await _workOrders.CreateAsync(command, cancellationToken);
            var response = WorkOrderResponse.FromDto(result);
            
            return CreatedAtAction(nameof(GetAllWorkOrders), new { id = response.WorkOrderId }, response);
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
    /// Deletes a work order by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteWorkOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteWorkOrderCommand
            {
                WorkOrderId = id
            };

            await _workOrders.DeleteAsync(command, cancellationToken);
            
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
}
