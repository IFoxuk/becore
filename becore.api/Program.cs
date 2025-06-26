using becore.api.Scheme;
using becore.api.Services;
using becore.api.Services.Interfaces;
using becore.api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Amazon.S3;
using becore.api.S3;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#region S3

builder.Services.Configure<S3Options>(options =>
{
    options.AccessKey = builder.Configuration["S3:AWS_ACCESS_KEY"]!;
    options.SecretKey = builder.Configuration["S3:AWS_SECRET_ACCESS_KEY"]!;
    options.BucketName = builder.Configuration["S3:AWS_BUCKET_NAME"]!;
    options.ServiceUrl = builder.Configuration["S3:AWS_SERVICE_URL"]!;
});
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var options = sp.GetRequiredService<IOptions<S3Options>>().Value;
    return new AmazonS3Client(options.AccessKey, options.SecretKey, new AmazonS3Config
    {
        ServiceURL = options.ServiceUrl,
        ForcePathStyle = true
    });
});

#endregion


builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = builder.Configuration.GetValue<bool>("IdentitySettings:RequireDigit");
    options.Password.RequireLowercase = builder.Configuration.GetValue<bool>("IdentitySettings:RequireLowercase");
    options.Password.RequireNonAlphanumeric = builder.Configuration.GetValue<bool>("IdentitySettings:RequireNonAlphanumeric");
    options.Password.RequireUppercase = builder.Configuration.GetValue<bool>("IdentitySettings:RequireUppercase");
    options.Password.RequiredLength = builder.Configuration.GetValue<int>("IdentitySettings:RequiredLength");
    options.Password.RequiredUniqueChars = builder.Configuration.GetValue<int>("IdentitySettings:RequiredUniqueChars");
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    
    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationContext>()
.AddDefaultTokenProviders();

// Register services
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();

// Configure JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "your-super-secret-key-that-should-be-at-least-32-characters-long";
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "becore-api",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "becore-client",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7291", "http://localhost:5000") // Adjust ports as needed
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "My API V1");
    });
}

app.UseHttpsRedirection();

app.UseCors();

// Добавляем middleware для отладки заголовков и принудительной аутентификации
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/auth"))
    {
        Console.WriteLine($"\n=== REQUEST TO {context.Request.Path} ===");
        Console.WriteLine($"Method: {context.Request.Method}");
        Console.WriteLine("Headers:");
        foreach (var header in context.Request.Headers)
        {
            Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value.ToArray())}");
        }
        Console.WriteLine("=== END REQUEST INFO ===\n");
        
        // Принудительно проверяем JWT для отладки
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            Console.WriteLine("🔍 Forcing JWT validation for debugging...");
            
            // Получаем JWT сервис и пытаемся валидировать токен вручную
            var jwtService = context.RequestServices.GetService<becore.api.Services.Interfaces.IJwtService>();
            if (jwtService != null)
            {
                var token = authHeader.Substring("Bearer ".Length);
                var principal = jwtService.ValidateToken(token);
                
                if (principal != null)
                {
                    Console.WriteLine($"✅ Manual JWT validation successful! User: {principal.Identity?.Name}");
                    Console.WriteLine($"✅ Claims count: {principal.Claims.Count()}");
                    
                    // Устанавливаем пользователя вручную для отладки
                    context.User = principal;
                }
                else
                {
                    Console.WriteLine("❌ Manual JWT validation failed!");
                }
            }
        }
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();