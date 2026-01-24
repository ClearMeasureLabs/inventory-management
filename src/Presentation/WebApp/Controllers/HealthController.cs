using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApp.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Returns the health status of the application and its dependencies
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthReportResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth()
    {
        var report = await _healthCheckService.CheckHealthAsync();

        var response = new HealthReportResponse
        {
            Status = report.Status.ToString(),
            TotalDuration = report.TotalDuration,
            Entries = report.Entries.Select(e => new HealthEntryResponse
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = e.Value.Duration
            }).ToList()
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}

public class HealthReportResponse
{
    public string Status { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public List<HealthEntryResponse> Entries { get; set; } = new();
}

public class HealthEntryResponse
{
    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public TimeSpan Duration { get; set; }
}
