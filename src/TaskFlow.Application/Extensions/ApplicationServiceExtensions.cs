using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Services;

namespace TaskFlow.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectMemberService, ProjectMemberService>();
        return services;
    }
}
