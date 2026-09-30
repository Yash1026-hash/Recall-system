using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Services;

public class JwtTokenService(IConfiguration configuration)
{
    public LoginResponse Create(UserAccount user)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "RecallOperations.Api";
        var audience = configuration["Jwt:Audience"] ?? "VehicleRecallApp";
        var key = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(60);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            ],
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new LoginResponse(
            user.UserId,
            user.Username,
            user.Role,
            user.FullName,
            user.Email,
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
