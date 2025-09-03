using MessagingSystem.Services.User.Application.Login.Dto;

namespace MessagingSystem.Services.User.Infrastructure.Jwt;

public interface IJwtService
{
    Task<bool> AuthenticateAndSetCookieAsync(LoginDto? user, string passwordRequest);
    Task<string?> RefreshTokenAsync(string? refreshToken);
}