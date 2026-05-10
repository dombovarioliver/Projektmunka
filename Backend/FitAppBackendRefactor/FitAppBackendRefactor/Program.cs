using DiplomaFit.Data;
using DiplomaFit.Service.DietService;
using DiplomaFit.Service.Dto;
using DiplomaFit.Service.ExerciseService;
using DiplomaFit.Service.FoodService;
using DiplomaFit.Service.GymService;
using DiplomaFit.Service.UserService;
using DiplomaFit.Service.WorkoutService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:8081"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "MovieClub API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
{
{
new OpenApiSecurityScheme
{
Reference = new OpenApiReference
{
Type=ReferenceType.SecurityScheme,
Id="Bearer"
}
},
new string[]{}
}
});
});

//JWT

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        ),


        ClockSkew = TimeSpan.Zero
    };
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

builder.Services.AddHttpClient<GooglePlacesGymService>();

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

app.UseStaticFiles();

//CORS
app.UseCors("frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();