using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        // ── Table ────────────────────────────────────────────
        builder.ToTable("project_members");

        // ── Composite primary key ─────────────────────────────
        // This is the key concept for join tables.
        // (ProjectId + UserId) together form a unique identifier.
        // EF Core will enforce that the same user cannot be added twice.
        builder.HasKey(pm => new { pm.ProjectId, pm.UserId });

        builder.Property(pm => pm.ProjectId)
            .HasColumnName("project_id");

        builder.Property(pm => pm.UserId)
            .HasColumnName("user_id");

        // ── Membership metadata ───────────────────────────────
        builder.Property(pm => pm.JoinedAt)
            .HasColumnName("joined_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // ── Relationships ─────────────────────────────────────

        // Many-to-many: ProjectMember is the join entity between User and Project

        // ProjectMember → Project
        // Delete behavior: Cascade — when a project is deleted, all its membership records are also deleted.
        builder.HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId)
            .HasConstraintName("fk_project_members_project")
            .OnDelete(DeleteBehavior.Cascade);

        // ProjectMember → User
        // Delete behavior: Cascade — when a user is deleted, their memberships are removed across all projects.
        builder.HasOne(pm => pm.User)
            .WithMany(u => u.ProjectMemberships)
            .HasForeignKey(pm => pm.UserId)
            .HasConstraintName("fk_project_members_user")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
