using MessagingSystem.Services.User.Application.Register.Dto;

namespace MessagingSystem.Services.User.Application.Register;

public interface IRegisterOrchestrator
{
    Task<OperationResult<string>> RegisterUserAsync(RegisterDto registerDto);
}