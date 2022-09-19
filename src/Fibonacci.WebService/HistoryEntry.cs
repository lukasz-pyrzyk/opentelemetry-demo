using System;

namespace Fibonacci.WebService;

public class HistoryEntry
{
    public int Id { get; set; }

    public string IpAddress { get; set; }

    public DateTimeOffset Date { get; set; }
}