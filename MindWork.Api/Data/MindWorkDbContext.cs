using Microsoft.EntityFrameworkCore;
using MindWork.Api.Entities;

namespace MindWork.Api.Data;

public class MindWorkDbContext : DbContext
{
    public MindWorkDbContext(DbContextOptions<MindWorkDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<MoodEntry> MoodEntries { get; set; }
}
