using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence;

/// <summary>
/// The main database session for the TaskFlow application.
/// Gives access to all entities via DbSet properties.
/// Entity configurations are loaded from the Configurations/ folder.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ─────────────────────────────────────────────────────────
    // DbSets — one per entity (= one per database table)
    // ─────────────────────────────────────────────────────────

    /// <summary>The users table.</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>The projects table.</summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>The project_members join table.</summary>
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    /// <summary>The tasks table.</summary>
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    // ─────────────────────────────────────────────────────────
    // Model configuration
    // ─────────────────────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically discovers and applies all IEntityTypeConfiguration<T>
        // classes found in this assembly (Infrastructure project).
        // This keeps OnModelCreating clean — no inline configuration here.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
