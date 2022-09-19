using Microsoft.EntityFrameworkCore;

namespace Fibonacci.WebService;

public class HistoryDbContext : DbContext
{
    public HistoryDbContext(DbContextOptions<HistoryDbContext> options) : base(options)
    {
            
    }

    public DbSet<HistoryEntry> Activities { get; set; }
}