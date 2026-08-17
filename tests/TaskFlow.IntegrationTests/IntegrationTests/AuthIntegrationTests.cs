using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskFlow.Application.Common.DTOs.Auth;

namespace TaskFlow.IntegrationTests.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterAndLogin_ShouldSucceed_AndReturnValidToken()
    {
        // 1. Register User
        var registerRequest = new RegisterRequest
        {
            Email = $"integration_{Guid.NewGuid():N}@taskflow.test",
            Password = "SecurePassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var authResult = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResult);
        Assert.NotEmpty(authResult.Token);
        Assert.Equal(registerRequest.Email, authResult.User.Email);

        // 2. Login User
        var loginRequest = new LoginRequest
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginAuthResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loginAuthResult);
        Assert.NotEmpty(loginAuthResult.Token);

        // 3. Access Protected Endpoint /api/auth/me using Bearer token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginAuthResult.Token);
        var meResponse = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var meUser = await meResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(meUser);
        Assert.Equal(registerRequest.Email, meUser.Email);
    }
}
