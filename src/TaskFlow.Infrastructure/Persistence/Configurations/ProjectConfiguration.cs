using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        // ── Table ────────────────────────────────────────────
        builder.ToTable("projects");

        // ── Primary key ──────────────────────────────────────
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // ── Name & description ────────────────────────────────
        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);
        // No .IsRequired() — Description is nullable

        // ── Status & deadline ─────────────────────────────────
        builder.Property(p => p.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(p => p.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("timestamp with time zone");
        // Nullable — no .IsRequired()

        // ── Timestamps ────────────────────────────────────────
        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // ── Relationships ─────────────────────────────────────

        // One-to-many: User (1) → Project (many) via OwnerId
        // Delete behavior: Restrict — you cannot delete a user who owns projects.
        // The owner must first delete their projects or transfer ownership.
        builder.HasOne(p => p.Owner)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(p => p.OwnerId)
            .HasConstraintName("fk_projects_owner")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        // Index on owner_id — makes "get all my projects" queries fast
        builder.HasIndex(p => p.OwnerId)
            .HasDatabaseName("ix_projects_owner_id");
    }
}
