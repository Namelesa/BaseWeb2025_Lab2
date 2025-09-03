using MessagingSystem.Services.User.Application;
using MessagingSystem.Services.User.Infrastructure;
using MessagingSystem.Services.User.Persistence;
using MessagingSystem.Services.User.Persistence.DbInitializer;

namespace MessagingSystem.Services.User.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddPersistenceLayer(builder.Configuration);
        builder.Services.AddApplicationLayer(builder.Configuration);
        builder.Services.AddInfrastructureLayer(builder.Configuration);
        builder.Services.AddWebApiLayer(builder.Configuration);

        builder.Services.AddControllers();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        using (var scope = app.Services.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            await dbInitializer.Initialize();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        
        //app.MapGet("/api/test/protected", [Authorize]() => Results.Ok("Protected endpoint reached successfully!"));
        
        await app.RunAsync();
    }
}