using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        // ── Table ────────────────────────────────────────────
        // Named "tasks" in the database (not "task_items") — cleaner SQL
        builder.ToTable("tasks");

        // ── Primary key ──────────────────────────────────────
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        // ── Title & description ───────────────────────────────
        builder.Property(t => t.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        // ── Status & priority ─────────────────────────────────
        builder.Property(t => t.Status)
            .HasColumnName("status")
            .IsRequired();
        // Default = ToDo (1)

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .IsRequired();
        // Default = Medium (2)

        // ── Deadline & timestamps ─────────────────────────────
        builder.Property(t => t.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("timestamp with time zone");
        // Nullable

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // ── Relationships ─────────────────────────────────────

        // TaskItem → Project (many-to-one)
        // Delete behavior: Cascade — deleting a project deletes all its tasks.
        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .HasConstraintName("fk_tasks_project")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        // TaskItem → User (creator — many-to-one)
        // Delete behavior: Restrict — you cannot delete a user who has created tasks.
        // This protects task history integrity.
        builder.HasOne(t => t.CreatedBy)
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(t => t.CreatedById)
            .HasConstraintName("fk_tasks_created_by")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.CreatedById)
            .HasColumnName("created_by_id")
            .IsRequired();

        // TaskItem → User (assignee — many-to-one, optional)
        // Delete behavior: SetNull — if the assigned user is deleted, the task becomes unassigned.
        // This is better than Restrict (which would block deletion) or Cascade (which would delete the task).
        builder.HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssigneeId)
            .HasConstraintName("fk_tasks_assignee")
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(t => t.AssigneeId)
            .HasColumnName("assignee_id");
        // No .IsRequired() — nullable

        // ── Indexes ───────────────────────────────────────────
        // These make common query patterns fast

        // "List all tasks in this project"
        builder.HasIndex(t => t.ProjectId)
            .HasDatabaseName("ix_tasks_project_id");

        // "List all tasks assigned to me"
        builder.HasIndex(t => t.AssigneeId)
            .HasDatabaseName("ix_tasks_assignee_id");

        // "List all tasks I created"
        builder.HasIndex(t => t.CreatedById)
            .HasDatabaseName("ix_tasks_created_by_id");

        // Composite index — "get all InProgress tasks in project X" is a common pattern
        builder.HasIndex(t => new { t.ProjectId, t.Status })
            .HasDatabaseName("ix_tasks_project_id_status");
    }
}
