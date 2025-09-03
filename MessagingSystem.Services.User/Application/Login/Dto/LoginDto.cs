namespace MessagingSystem.Services.User.Application.Login.Dto;

public class LoginDto(string login, string password)
{
    public string Login { get; init; } = login;
    public string Password { get; init; } = password;
}