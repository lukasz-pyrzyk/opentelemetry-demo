using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Fibonacci.Shared;

public static class ServiceCollectionExtensions
{
    public static void AddOpenTelemetry(this IServiceCollection services)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
    }
}