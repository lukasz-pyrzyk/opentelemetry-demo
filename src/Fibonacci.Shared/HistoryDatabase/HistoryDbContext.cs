using Microsoft.EntityFrameworkCore;

namespace Fibonacci.Shared.HistoryDatabase;

public class HistoryDbContext : DbContext
{
    public HistoryDbContext(DbContextOptions<HistoryDbContext> options) : base(options)
    {
            
    }

    public DbSet<HistoryEntry> Activities { get; set; }
}