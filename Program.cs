using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SchoolManager.Data;
using SchoolManager.Models;
using SchoolManager.Services;
using SchoolManager.Services.Implementations;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework and Identity
builder.Services.AddDbContext<SchoolManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings from configuration
    var passwordPolicy = builder.Configuration.GetSection("SecuritySettings:PasswordPolicy");
    options.Password.RequiredLength = passwordPolicy.GetValue<int>("RequiredLength", 8);
    options.Password.RequireDigit = passwordPolicy.GetValue<bool>("RequireDigit", true);
    options.Password.RequireLowercase = passwordPolicy.GetValue<bool>("RequireLowercase", true);
    options.Password.RequireUppercase = passwordPolicy.GetValue<bool>("RequireUppercase", true);
    options.Password.RequireNonAlphanumeric = passwordPolicy.GetValue<bool>("RequireNonAlphanumeric", true);
    options.Password.RequiredUniqueChars = passwordPolicy.GetValue<int>("RequiredUniqueChars", 4);

    // Lockout settings
    var lockoutPolicy = builder.Configuration.GetSection("SecuritySettings:Lockout");
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
        lockoutPolicy.GetValue<int>("DefaultLockoutTimeSpanMinutes", 15));
    options.Lockout.MaxFailedAccessAttempts = lockoutPolicy.GetValue<int>("MaxFailedAccessAttempts", 5);
    options.Lockout.AllowedForNewUsers = lockoutPolicy.GetValue<bool>("AllowedForNewUsers", true);

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Set to true in production with email service
})
.AddEntityFrameworkStores<SchoolManagementDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Authentication:Jwt");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ??
                jwtSettings["Key"] ??
                throw new InvalidOperationException("JWT Secret Key is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TeacherOrAdmin", policy => policy.RequireRole("Teacher", "Admin"));
    options.AddPolicy("StudentOrHigher", policy => policy.RequireRole("Student", "Teacher", "Admin"));
});

// Add CORS
var corsSettings = builder.Configuration.GetSection("Cors");
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsSettings["PolicyName"] ?? "SchoolManagerCorsPolicy", policy =>
    {
        var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost:3000" };
        var allowedMethods = corsSettings.GetSection("AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE" };
        var allowedHeaders = corsSettings.GetSection("AllowedHeaders").Get<string[]>() ?? new[] { "Content-Type", "Authorization" };

        policy.WithOrigins(allowedOrigins)
              .WithMethods(allowedMethods)
              .WithHeaders(allowedHeaders);

        if (corsSettings.GetValue<bool>("AllowCredentials", true))
        {
            policy.AllowCredentials();
        }
    });
});

// Add services to the container - FIXED dependency injection
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordService, PasswordService>(); 
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>(); 

// Add caching
builder.Services.AddMemoryCache();
var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
    });
}

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    var rateLimitSettings = builder.Configuration.GetSection("SecuritySettings:RateLimiting");
    if (rateLimitSettings.GetValue<bool>("EnableRateLimiting", true))
    {
        options.AddFixedWindowLimiter("GlobalRateLimit", limiterOptions =>
        {
            limiterOptions.PermitLimit = rateLimitSettings.GetValue<int>("RequestsPerMinute", 100);
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = rateLimitSettings.GetValue<int>("BurstLimit", 20);
        });
    }
});

// Add Health Checks(corrected version)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SchoolManagementDbContext>()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Add controllers with options
builder.Services.AddControllers(options =>
{
    options.MaxModelValidationErrors = 50;
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor((_) => "The field is required.");
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});


// Configure OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "School Manager API",
        Version = "v1.0",
        Description = "API for School Management System",
        Contact = new OpenApiContact
        {
            Name = "School Manager Support",
            Email = "support@schoolmanager.com"
        }
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "School Manager API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseHttpsRedirection();

// Enable CORS
app.UseCors(corsSettings["PolicyName"] ?? "SchoolManagerCorsPolicy");

// Enable rate limiting
var rateLimitSettings = builder.Configuration.GetSection("SecuritySettings:RateLimiting");
if (rateLimitSettings.GetValue<bool>("EnableRateLimiting", true))
{
    app.UseRateLimiter();
}

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");

// Map controllers
app.MapControllers();

app.Run();

