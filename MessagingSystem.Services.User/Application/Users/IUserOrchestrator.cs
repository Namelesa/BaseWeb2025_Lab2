namespace MessagingSystem.Services.User.Application.Users;

public interface IUserOrchestrator
{
    Task<OperationResult<Core.User.User>> GetUserByHashLoginAsync(string login);
    Task<OperationResult<string>> DeleteUserByHashLoginAsync(string login);
}