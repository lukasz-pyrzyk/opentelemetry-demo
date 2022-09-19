using System;
using System.Threading;
using System.Threading.Tasks;
using Fibonacci.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Fibonacci.WebService.Controllers;

[ApiController]
public class FibonacciController : ControllerBase
{
    private readonly ILogger<FibonacciController> _logger;
    private readonly HistoryDbContext _db;

    public FibonacciController(ILogger<FibonacciController> logger, HistoryDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpPost("/{n?}")]
    public async Task<IActionResult> Calculate(int? n, [FromServices] QueueClient queueClient)
    {
        if (!n.HasValue) return BadRequest();

        _logger.LogInformation("Saving new activity info to the database");
        var entry = new HistoryEntry {Date = DateTimeOffset.UtcNow, IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString()};
        _db.Activities.Add(entry);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Information saved");

        _logger.LogInformation("Creating a calculation request for n {n}", n);
            
        var msg = new Message { Body = BitConverter.GetBytes(n.Value) };

        _logger.LogInformation("Sending calculation request for n {n}", n);
        await queueClient.SendAsync(msg);
        _logger.LogInformation("Message for n {n} sent", n);

        return Accepted($"/{n.Value}");
    }

    [HttpGet("/{n?}")]
    public async Task<IActionResult> Get(int? n, [FromServices] Repository repository, CancellationToken ct)
    {
        if (!n.HasValue) return BadRequest();

        _logger.LogInformation("Getting result for {n}", n);
        var fib = await repository.GetEntity(n.Value, ct);

        if (fib.HasValue)
        {
            _logger.LogInformation("Result for {n} found", n);
            return Ok(fib);
        }
            
        _logger.LogInformation("Result for {n} not found", n);
        return NotFound();
    }

    [HttpDelete("/{n?}")]
    public async Task<IActionResult> Delete(int? n, [FromServices] Repository repository, CancellationToken ct)
    {
        if (!n.HasValue) return BadRequest();

        _logger.LogInformation("Deleting entity {n}", n);
        await repository.Delete(n.Value, ct);

        return NoContent();
    }
}