using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Authentication;

namespace TaskFlow.UnitTests.Authentication;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_ShouldReturnValidJwtString_WithUserClaims()
    {
        // Arrange
        var jwtOptions = Options.Create(new JwtOptions
        {
            Secret = "SUPER_SECRET_KEY_FOR_UNIT_TESTING_PURPOSES_12345",
            Issuer = "TaskFlow.TestIssuer",
            Audience = "TaskFlow.TestAudience",
            ExpiryMinutes = 60
        });

        var generator = new JwtTokenGenerator(jwtOptions);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@taskflow.local",
            FirstName = "Jane",
            LastName = "Doe",
            Role = UserRole.User
        };

        // Act
        var tokenString = generator.GenerateToken(user);

        // Assert
        Assert.NotNull(tokenString);
        Assert.NotEmpty(tokenString);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenString);

        Assert.Equal("TaskFlow.TestIssuer", jwtToken.Issuer);
        Assert.Equal("TaskFlow.TestAudience", jwtToken.Audiences.First());

        var nameIdClaim = jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        Assert.Equal(user.Id.ToString(), nameIdClaim);
    }
}
