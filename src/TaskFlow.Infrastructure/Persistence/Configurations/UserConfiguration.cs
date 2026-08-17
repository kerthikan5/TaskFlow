using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // ── Table ────────────────────────────────────────────
        builder.ToTable("users");

        // ── Primary key ──────────────────────────────────────
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // We assign GUIDs in application code, not the database

        // ── Email ─────────────────────────────────────────────
        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(256);

        // Unique constraint — no two users can share an email address
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        // ── Password ──────────────────────────────────────────
        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        // ── Profile ───────────────────────────────────────────
        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasMaxLength(100);

        // ── Role & Status ─────────────────────────────────────
        builder.Property(u => u.Role)
            .HasColumnName("role")
            .IsRequired();
        // Stored as integer in PostgreSQL (1 = User, 2 = Admin)

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        // ── Timestamps ────────────────────────────────────────
        // "timestamp with time zone" = timestamptz in PostgreSQL — stores UTC
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // ── Relationships ─────────────────────────────────────
        // Navigation properties are configured on the other side (Project, Task configurations)
        // User itself has no FK columns — it's the principal end of all relationships
    }
}
