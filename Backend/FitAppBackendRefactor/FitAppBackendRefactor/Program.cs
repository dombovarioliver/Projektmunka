using DiplomaFit.Data;
using DiplomaFit.Service.Dto;
using DiplomaFit.Service.UserService;
using DiplomaFit.Service.FoodService;
using DiplomaFit.Service.ExerciseService;
using DiplomaFit.Service.DietService;
using DiplomaFit.Service.WorkoutService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//CORS (frontend miatt)
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

//Connection string (local + docker fallback)
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection") ??
    builder.Configuration["ConnectionStrings__DefaultConnection"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string nincs beállítva.");
}

//DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
    .UseSqlServer(connectionString)
    .UseLazyLoadingProxies();
});

//ML API URL (docker + local fallback)
var mlApiBaseUrl =
    builder.Configuration["ExternalServices:MlApiBaseUrl"] ??
    builder.Configuration["ExternalServices__MlApiBaseUrl"] ??
    "http://localhost:8000";

//DTO Provider
builder.Services.AddScoped<DtoProvider>();

//Repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<FoodRepository>();
builder.Services.AddScoped<ExerciseRepository>();

//Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<FoodService>();
builder.Services.AddScoped<ExerciseService>();

builder.Services.AddScoped<DietPlanService>();
builder.Services.AddScoped<WorkoutPlanService>();

// 🌐 HttpClient → ML API
builder.Services.AddHttpClient<DietMlClientService>(client =>
{
    client.BaseAddress = new Uri(mlApiBaseUrl);
});

builder.Services.AddHttpClient<WorkoutSplitMlClientService>(client =>
{
    client.BaseAddress = new Uri(mlApiBaseUrl);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var retryCount = 0;
    const int maxRetries = 10;

    while (true)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch
        {
            retryCount++;

            if (retryCount >= maxRetries)
                throw;

            Thread.Sleep(5000);
        }
    }
}

//Swagger
app.UseSwagger();
app.UseSwaggerUI();

//CORS
app.UseCors("frontend");

// Dockerben NE erőltesd a HTTPS redirectet
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();