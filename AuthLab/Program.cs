using AuthLab.Data;
using AuthLab.Middleware;
using AuthLab.Models;
using AuthLab.Services.Implementations;
using AuthLab.Services.Interfaces;
using AuthLab.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
var serverVersion = new MySqlServerVersion(new Version(8, 0, 31));

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, serverVersion);
});

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// Configure JWT settings
var jwtConfig = builder.Configuration.GetSection("JwtSettings").Get<JwtConfig>(); // Grab the JwtSettings section from config and deserialize it into a JwtConfig object.
// GetSection().Get<JwtConfig>() — creates a one-time snapshot of the config values right now. Just a plain object, no DI involvement.

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtSettings")); // Bind JwtConfig to the "JwtSettings" section in appsettings.json
// Services.Configure<JwtConfig>() — registers JwtConfig with the DI container so you can inject IOptions<JwtConfig> anywhere in your app.

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Set the default authentication scheme to JWT Bearer
    // "Use JWT to authenticate every request"

    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Set the default challenge scheme to JWT Bearer (used when authentication fails and a challenge is issued)
    // "When auth fails, return 401"
})
    .AddJwtBearer(options => // Add JWT Bearer authentication
    {
        options.TokenValidationParameters = new TokenValidationParameters // Configure the parameters for validating JWT tokens
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey))
            // jwtConfig.SecretKey — your secret key string from user secrets.
            // Encoding.UTF8.GetBytes(...) — converts that string to a byte array. Why? Because cryptographic algorithms work on bytes, not strings.
            // new SymmetricSecurityKey(...) — wraps those bytes into a key object that the signing algorithm can use.
        };
    });

builder.Services.AddAuthorization(options => // Add authorization policies
{
    options.AddPolicy("AdminOrUser", policy =>  // Define a policy named "AdminOrUser" that requires the user to have either the "Admin" or "User" role
        policy.RequireRole("Admin", "User"));   // This policy can be applied to endpoints to restrict access to users with either role.
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin")); // Define a policy named "AdminOnly" that requires the user to have the "Admin" role.
});

// Register the Services with the DI container 
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer(); 

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme // Define a security scheme for JWT Bearer authentication in Swagger
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement // Add a security requirement to indicate that the API endpoints require JWT Bearer authentication
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await DatabaseSeeder.SeedAsync(app.Services);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/minimal/hello", () => "Hello from minimal API!")
    .WithTags("Minimal")
    .WithSummary("Hello from minimal API");

app.MapGet("/api/minimal/protected", () => "Hello from protected minimal API!")
    .WithTags("Minimal")
    .RequireAuthorization();

app.MapGet("/api/test/exception", () =>
{
    throw new Exception("Test exception from middleware");
})
   .WithOpenApi()
    .WithTags("Test");

app.Run();
