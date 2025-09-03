using MessagingSystem.Services.User.WebApi.Login;
using MessagingSystem.Services.User.WebApi.Register;

namespace MessagingSystem.Services.User.WebApi;

public static class AddWebApi
{
    public static void AddWebApiLayer(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAutoMapper(config => config.AddProfile(new RegisterMap()));
        services.AddAutoMapper(config => config.AddProfile(new LoginMap()));
    }
}