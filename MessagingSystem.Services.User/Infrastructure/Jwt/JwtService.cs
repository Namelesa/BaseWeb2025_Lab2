using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MessagingSystem.Services.User.Application.Login.Dto;
using MessagingSystem.Services.User.Core.User;
using MessagingSystem.Services.User.Infrastructure.HasherInfo;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace MessagingSystem.Services.User.Infrastructure.Jwt;

public class JwtService(
    IConfiguration config,
    ILogger<JwtService> logger,
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository,
    IHasher hasher) : IJwtService
{
    private readonly IConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
    private readonly ILogger<JwtService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly PasswordHasher<LoginDto> _passwordHasher = new();
    
    public async Task<bool> AuthenticateAndSetCookieAsync(LoginDto? loginDto, string passwordRequest)
    {
        if (loginDto is null) return false;
    
        var user = await _userRepository.FindUserByHashLoginAsync(hasher.Hash(loginDto.Login));
        if (user is null || !ValidateUserCredentials(loginDto, passwordRequest))
        {
            _logger.LogWarning("Authentication failed for user {Login}", loginDto.Login);
            return false;
        }

        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        
        var refreshExpiry = DateTime.UtcNow.AddDays(_config.GetValue<int>("JWTConfig:RefreshTokenValidityDays"));
        user.SetRefreshToken(refreshToken, refreshExpiry);
        await _userRepository.UpdateUserAsync(user);

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_config.GetValue<int>("JWTConfig:TokenValidityMinutes"))
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = refreshExpiry
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append("access_token", accessToken, accessCookieOptions);
        _httpContextAccessor.HttpContext?.Response.Cookies.Append("refresh_token", refreshToken, refreshCookieOptions);

        return true;
    }
    
    private bool ValidateUserCredentials(LoginDto? user, string passwordRequest)
    {
        if (user?.Password == null) return false;
        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, passwordRequest, user.Password);
        return passwordVerificationResult == PasswordVerificationResult.Success;
    }

    public async Task<string?> RefreshTokenAsync(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken)) return null;

        var user = await _userRepository.FindByRefreshTokenAsync(refreshToken);
        if(user == null)
        {
            _logger.LogWarning("Refresh token not associated with any user");
            return null;
        }
        
        if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid or expired refresh token");
            return null;
        }
        
        var newAccessToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(_config.GetValue<int>("JWTConfig:RefreshTokenValidityDays")));
        await _userRepository.UpdateUserAsync(user);

        var accessCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_config.GetValue<int>("JWTConfig:TokenValidityMinutes"))
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = user.RefreshTokenExpiryTime
        };

        _httpContextAccessor.HttpContext?.Response.Cookies.Append("access_token", newAccessToken, accessCookieOptions);
        _httpContextAccessor.HttpContext?.Response.Cookies.Append("refresh_token", newRefreshToken, refreshCookieOptions);

        return newAccessToken;
    }
    
    private string GenerateJwtToken(Core.User.User user)
    {
        var issuer = _config["JWTConfig:Issuer"];
        var audience = _config["JWTConfig:Audience"];
        var key = _config["JWTConfig:Key"];
        var tokenMin = _config.GetValue<int>("JWTConfig:TokenValidityMinutes");

        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddMinutes(tokenMin);

        if (key == null)
            throw new InvalidOperationException("JWT Key is not configured.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(issuedAt).ToString(), ClaimValueTypes.Integer64),
            new(ClaimTypes.Role, user.Role),
            new(ClaimTypes.Name, user.Login)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            IssuedAt = issuedAt,
            NotBefore = issuedAt,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}