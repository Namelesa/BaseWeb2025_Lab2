using MessagingSystem.Services.User.Application.Users;
using MessagingSystem.Services.User.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MessagingSystem.Services.User.WebApi.User;

[ApiController]
[Route("users/v1")]
public class UserController(IUserOrchestrator userOrchestration, IJwtService jwtService) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpDelete("userDelete/{login}")]
    public async Task<IActionResult> GetAllUsersAsync(string login)
    {
        var user = await userOrchestration.DeleteUserByHashLoginAsync(login);
        return user.Success 
            ? Ok(user.Data) 
            : BadRequest(user.Message);
    }
    
    [Authorize(Roles = "User, Admin")]
    [HttpGet("userById/{login}")]
    public async Task<IActionResult> GetUserByIdAsync(string login)
    {
        var user = await userOrchestration.GetUserByHashLoginAsync(login);
        return user.Success 
            ? Ok(user.Data) 
            : BadRequest(user.Message);
    }
    
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized("Refresh token is missing.");

        var newAccessToken = await jwtService.RefreshTokenAsync(refreshToken);
        if (newAccessToken is null)
            return Unauthorized("Invalid or expired refresh token.");

        return Ok(new { access_token = newAccessToken });
    }
    
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        return Ok(new { message = "Logged out" });
    }
}