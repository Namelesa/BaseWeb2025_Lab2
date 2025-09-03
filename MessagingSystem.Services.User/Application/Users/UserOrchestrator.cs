using MessagingSystem.Services.User.Core.User;
using MessagingSystem.Services.User.Infrastructure.HasherInfo;

namespace MessagingSystem.Services.User.Application.Users;

public class UserOrchestrator(IUserRepository userRepository, IHasher hasher) : IUserOrchestrator
{
    public async Task<OperationResult<Core.User.User>> GetUserByHashLoginAsync(string login)
    {
        var user = await userRepository.FindUserByHashLoginAsync(hasher.Hash(login));
        return user is null 
            ? OperationResult<Core.User.User>.Fail("User not found") 
            : OperationResult<Core.User.User>.Ok(user);
    }
    
    public async Task<OperationResult<string>> DeleteUserByHashLoginAsync(string login)
    {
        var user = await userRepository.FindUserByHashLoginAsync(hasher.Hash(login));
        if (user is null)
            return OperationResult<string>.Fail("User not found");
        
        await userRepository.DeleteUserAsync(user);
        return OperationResult<string>.Ok("User deleted successfully");
    }
}