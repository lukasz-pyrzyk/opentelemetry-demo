using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.ServiceBus;
using Fibonacci.Shared.HistoryDatabase;
using Fibonacci.Shared.ServiceBus;
using Fibonacci.Shared.TableStorage;
using Microsoft.Extensions.Logging;

namespace Fibonacci.WebService.Controllers;

[ApiController]
public class FibonacciController : ControllerBase
{
    private readonly FibonacciQueueSender _sender;
    private readonly FibonacciTableStorage _tableStorage;

    private readonly ILogger<FibonacciController> _logger;
    private readonly HistoryDbContext _trackingDatabase;

    public FibonacciController(FibonacciQueueSender sender, FibonacciTableStorage tableStorage, HistoryDbContext trackingDatabase, ILogger<FibonacciController> logger)
    {
        _sender = sender;
        _tableStorage = tableStorage;
        _trackingDatabase = trackingDatabase;
        _logger = logger;
    }

    [HttpGet("/")]
    public IActionResult Get()
    {
        return Ok("App is started");
    }

    [HttpPost("/{n?}")]
    public async Task<IActionResult> Calculate(int? n)
    {
        if (!n.HasValue) return BadRequest();

        _logger.LogInformation("Saving new activity info to the database");
        var entry = new HistoryEntry { Date = DateTimeOffset.UtcNow, IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString() };
        _trackingDatabase.Activities.Add(entry);
        await _trackingDatabase.SaveChangesAsync();
        _logger.LogInformation("Information saved");

        _logger.LogInformation("Creating a calculation request for n {n}", n);

        var msg = new ServiceBusMessage(n.Value.ToString());

        _logger.LogInformation("Sending calculation request for n {n}", n);
        await _sender.Send(msg);
        _logger.LogInformation("Message for n {n} sent", n);

        return Accepted($"/{n.Value}");
    }

    [HttpGet("/{n}")]
    public async Task<IActionResult> Get(int n, CancellationToken ct)
    {
        _logger.LogInformation("Getting result for {n}", n);
        var fib = await _tableStorage.GetEntity(n, ct);

        if (fib.HasValue)
        {
            _logger.LogInformation("Result for {n} found", n);
            return Ok(fib);
        }

        _logger.LogInformation("Result for {n} not found", n);
        return NotFound($"No result for n: {n}");
    }

    [HttpDelete("/{n?}")]
    public async Task<IActionResult> Delete(int? n, CancellationToken ct)
    {
        if (!n.HasValue) return BadRequest();

        _logger.LogInformation("Deleting entity {n}", n);
        await _tableStorage.Delete(n.Value, ct);

        return NoContent();
    }
}