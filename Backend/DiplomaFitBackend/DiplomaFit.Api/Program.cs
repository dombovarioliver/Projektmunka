using DiplomaFit.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DiplomaFit.Api.Application.Interfaces;
using DiplomaFit.Api.Application.Services;


namespace DiplomaFit.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // EF Core
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Application service DI
            builder.Services.AddScoped<IDietPlanService, DietPlanService>();

            builder.Services.AddScoped<IDietPlanService, DietPlanService>();
            builder.Services.AddScoped<IImportService, ImportService>();
            builder.Services.AddHttpClient<IDietMlClient, DietMlClient>();
            builder.Services.AddScoped<IMlDietPlanService, MlDietPlanService>();
            builder.Services.AddScoped<IDietPlanService, DietPlanService>();
            builder.Services.AddScoped<IWorkoutPlanService, WorkoutPlanService>();

            builder.Services.AddHttpClient<IDietMlClient, DietMlClient>((sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var baseUrl = config["Ml:BaseUrl"] ?? "http://localhost:8000";

                client.BaseAddress = new Uri(baseUrl);
            });

            builder.Services.AddHttpClient<IMlWorkoutSplitClient, MlWorkoutSplitClient>(client =>
            {
                // Dockerben: "http://ml:8000" (ahogy a diet ML-nél is)
                // Lokálon teszteléshez lehet "http://localhost:8000"
                var baseUrl = builder.Configuration["MlService:BaseUrl"] ?? "http://ml:8000";
                client.BaseAddress = new Uri(baseUrl);
            });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            // app.UseHttpsRedirection();

            app.UseCors();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
