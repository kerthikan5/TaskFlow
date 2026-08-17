using TaskFlow.Application.Common.DTOs.ProjectMember;

namespace TaskFlow.Application.Common.Interfaces;

public interface IProjectMemberService
{
    Task<ProjectMemberDto> AddMemberAsync(Guid projectId, AddProjectMemberRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectMemberDto>> GetProjectMembersAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(Guid projectId, Guid targetUserId, CancellationToken cancellationToken = default);
}
