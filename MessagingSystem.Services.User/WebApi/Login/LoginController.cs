using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MessagingSystem.Services.User.Application.Login;
using MessagingSystem.Services.User.Application.Login.Dto;
using MessagingSystem.Services.User.WebApi.Login.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MessagingSystem.Services.User.WebApi.Login;

[ApiController]
[Route("users")]
public class LoginController(IMapper mapper, ILoginOrchestrator loginOrchestrator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([Required, FromBody] LoginContract loginContract)
    {
        var loginDto = mapper.Map<LoginDto>(loginContract);
        var result = await loginOrchestrator.LoginUserAsync(loginDto);

        return result.Success
            ? Ok($"{result.Data}")
            : BadRequest($"{result.Message}");
    }
}