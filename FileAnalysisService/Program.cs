using FileAnalisysService;
using Microsoft.EntityFrameworkCore;

class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpClient();
        builder.Services.AddDbContext<AnalysisDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

        builder.Services.AddScoped<IAnalysisService, SimpleAnalysisService>();

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AnalysisDbContext>();
            db.Database.Migrate(); // Применяет все миграции
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();
        app.Run();

    }
}