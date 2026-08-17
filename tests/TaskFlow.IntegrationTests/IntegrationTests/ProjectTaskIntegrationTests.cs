using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskFlow.Application.Common.DTOs.Auth;
using TaskFlow.Application.Common.DTOs.Project;
using TaskFlow.Application.Common.DTOs.ProjectMember;
using TaskFlow.Application.Common.DTOs.Task;
using TaskFlow.Domain.Enums;

namespace TaskFlow.IntegrationTests.IntegrationTests;

public class ProjectTaskIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProjectTaskIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CompleteProjectAndTaskWorkflow_ShouldSucceed()
    {
        var ownerClient = _factory.CreateClient();
        var memberClient = _factory.CreateClient();

        // 1. Register Owner
        var ownerEmail = $"owner_{Guid.NewGuid():N}@taskflow.test";
        var ownerAuth = await RegisterUserAsync(ownerClient, ownerEmail, "Owner", "User");
        ownerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerAuth.Token);

        // 2. Register Member
        var memberEmail = $"member_{Guid.NewGuid():N}@taskflow.test";
        var memberAuth = await RegisterUserAsync(memberClient, memberEmail, "Member", "User");
        memberClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", memberAuth.Token);

        // 3. Owner Creates Project
        var createProjectReq = new CreateProjectRequest
        {
            Name = "Sprint 1 Project",
            Description = "Full Stack TaskFlow Integration Project",
            Status = ProjectStatus.Active
        };

        var projectRes = await ownerClient.PostAsJsonAsync("/api/projects", createProjectReq);
        Assert.Equal(HttpStatusCode.Created, projectRes.StatusCode);

        var project = await projectRes.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);

        // 4. Owner Invites Member
        var addMemberReq = new AddProjectMemberRequest { Email = memberEmail };
        var addMemberRes = await ownerClient.PostAsJsonAsync($"/api/projects/{project.Id}/members", addMemberReq);
        Assert.Equal(HttpStatusCode.Created, addMemberRes.StatusCode);

        // 5. Owner Creates Task in Project
        var createTaskReq = new CreateTaskRequest
        {
            Title = "Implement API Integration Tests",
            Description = "Write end-to-end tests for Projects and Tasks",
            Priority = TaskPriority.High,
            AssigneeId = memberAuth.User.Id
        };

        var taskRes = await ownerClient.PostAsJsonAsync($"/api/projects/{project.Id}/tasks", createTaskReq);
        Assert.Equal(HttpStatusCode.Created, taskRes.StatusCode);

        var task = await taskRes.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(task);
        Assert.Equal(TaskItemStatus.ToDo, task.Status);
        Assert.Equal(memberAuth.User.Id, task.AssigneeId);

        // 6. Member Updates Task Status to Completed
        var updateStatusReq = new UpdateTaskStatusRequest { Status = TaskItemStatus.Completed };
        var updateStatusRes = await memberClient.PatchAsJsonAsync($"/api/tasks/{task.Id}/status", updateStatusReq);
        Assert.Equal(HttpStatusCode.OK, updateStatusRes.StatusCode);

        var updatedTask = await updateStatusRes.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(updatedTask);
        Assert.Equal(TaskItemStatus.Completed, updatedTask.Status);
    }

    private static async Task<AuthResponse> RegisterUserAsync(HttpClient client, string email, string firstName, string lastName)
    {
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = "SecurePassword123!",
            FirstName = firstName,
            LastName = lastName
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        return result;
    }
}
