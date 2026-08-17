using Microsoft.Extensions.DependencyInjection;

namespace TaskFlow.Application.Extensions;

/// <summary>
/// Registers Application layer services with ASP.NET Core Dependency Injection container.
/// </summary>
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application services (AuthService, ProjectService, TaskService) will be registered here in upcoming phases

        return services;
    }
}
