using FluentValidation;
using MessagingSystem.Services.User.Application.Login;
using MessagingSystem.Services.User.Application.Login.Dto;
using MessagingSystem.Services.User.Application.Register;
using MessagingSystem.Services.User.Application.Register.Dto;
using MessagingSystem.Services.User.Application.Users;

namespace MessagingSystem.Services.User.Application;

public static class AddApplication
{
    public static void AddApplicationLayer(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IRegisterOrchestrator, RegisterOrchestrator>();
        services.AddScoped<ILoginOrchestrator, LoginOrchestrator>();
        services.AddScoped<IUserOrchestrator, UserOrchestrator>();
        services.AddScoped<IValidator<RegisterDto>, RegisterValidator>();
        services.AddScoped<IValidator<LoginDto>, LoginValidator>();
    }
}