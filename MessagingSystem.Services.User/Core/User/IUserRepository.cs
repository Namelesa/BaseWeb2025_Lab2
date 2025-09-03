namespace MessagingSystem.Services.User.Core.User;

public interface IUserRepository
{
    Task<User?> FindUserByHashLoginAsync(string login);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(User user);
    Task<User?> FindByRefreshTokenAsync(string refreshToken);
}