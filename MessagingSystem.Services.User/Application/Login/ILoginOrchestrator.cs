using MessagingSystem.Services.User.Application.Login.Dto;

namespace MessagingSystem.Services.User.Application.Login;

public interface ILoginOrchestrator
{
    Task<OperationResult<string>> LoginUserAsync(LoginDto loginDto);
}