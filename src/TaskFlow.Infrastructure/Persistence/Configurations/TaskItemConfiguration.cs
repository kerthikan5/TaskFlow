using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("timestamp with time zone");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .HasConstraintName("fk_tasks_project")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.HasOne(t => t.CreatedBy)
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(t => t.CreatedById)
            .HasConstraintName("fk_tasks_created_by")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired();

        builder.HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssigneeId)
            .HasConstraintName("fk_tasks_assignee")
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(t => t.AssigneeId)
            .HasColumnName("assignee_id");

        builder.HasIndex(t => t.ProjectId)
            .HasDatabaseName("ix_tasks_project_id");

        builder.HasIndex(t => t.AssigneeId)
            .HasDatabaseName("ix_tasks_assignee_id");

        builder.HasIndex(t => t.CreatedById)
            .HasDatabaseName("ix_tasks_created_by_id");

        builder.HasIndex(t => new { t.ProjectId, t.Status })
            .HasDatabaseName("ix_tasks_project_id_status");
    }
}
