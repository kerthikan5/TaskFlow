using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("project_members");

        builder.HasKey(pm => new { pm.ProjectId, pm.UserId });

        builder.Property(pm => pm.ProjectId)
            .HasColumnName("project_id");

        builder.Property(pm => pm.UserId)
            .HasColumnName("user_id");

        builder.Property(pm => pm.JoinedAt)
            .HasColumnName("joined_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId)
            .HasConstraintName("fk_project_members_project")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pm => pm.User)
            .WithMany(u => u.ProjectMemberships)
            .HasForeignKey(pm => pm.UserId)
            .HasConstraintName("fk_project_members_user")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
