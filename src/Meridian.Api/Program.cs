using System.Text;
using Meridian.Api.Services;
using Meridian.Application.Common.Interfaces;
using Meridian.Domain.Entities;
using Meridian.Infrastructure;
using Meridian.Infrastructure.Persistence;
using Meridian.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Infrastructure: DbContext, repositories, unit of work, hashing, tokens.
// ---------------------------------------------------------------------------
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// ---------------------------------------------------------------------------
// Authentication. The signing key is validated at startup rather than on the
// first login, so a misconfigured deployment fails immediately and visibly.
// ---------------------------------------------------------------------------
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("The Jwt configuration section is missing.");
jwtSettings.Validate(builder.Environment.IsDevelopment());
builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(jwtSettings));

builder.Services
    .AddAuthentication(options =>
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
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),

            // Tokens expire exactly when they say they do. The five minute default
            // skew is convenient in distributed systems and misleading in a demo.
            ClockSkew = TimeSpan.Zero
        };
    });

// ---------------------------------------------------------------------------
// Role-based access control. Policies are named once here so that controllers
// reference an intent ("RecruiterOnly") rather than repeating role strings.
// ---------------------------------------------------------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CandidateOnly", policy => policy.RequireRole(Role.Candidate));
    options.AddPolicy("RecruiterOnly", policy => policy.RequireRole(Role.Recruiter));
    options.AddPolicy("HiringManagerOnly", policy => policy.RequireRole(Role.HiringManager));
    options.AddPolicy("AdministratorOnly", policy => policy.RequireRole(Role.Administrator));
    options.AddPolicy("RecruitingStaff", policy =>
        policy.RequireRole(Role.Recruiter, Role.HiringManager, Role.Administrator));
});

// ---------------------------------------------------------------------------
// CORS. The React client runs on a different origin during development, so the
// allowed origins are configuration rather than a wildcard.
// ---------------------------------------------------------------------------
const string CorsPolicy = "MeridianWebClient";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };
var allowedOriginSuffixes = builder.Configuration.GetSection("Cors:AllowedOriginSuffixes").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy => policy
        .SetIsOriginAllowed(origin =>
        {
            if (allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            // Cloudflare Pages gives every deployment its own preview subdomain,
            // so the deployed client's exact origin is not known ahead of time.
            // Suffix matching covers those without opening the API to any origin.
            return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                   && uri.Scheme == Uri.UriSchemeHttps
                   && allowedOriginSuffixes.Any(suffix =>
                       uri.Host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
        })
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Meridian Talent Platform API",
        Version = "v1",
        Description = "AI-powered recruitment and talent management platform. "
                      + "SE205.3 Software Architecture coursework, NSBM Green University."
    });

    // Lets Swagger UI send the bearer token, so the whole API can be exercised
    // from the browser without a separate REST client.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the access token returned by /api/auth/login."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var xmlPath = Path.Combine(AppContext.BaseDirectory, "Meridian.Api.xml");
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Apply migrations and seed on startup. Acceptable here because the evaluator
// runs the project from source with no deployment pipeline; a production system
// would run migrations as a separate, gated step.
// ---------------------------------------------------------------------------
// A database that is unreachable at boot must not prevent the process from
// starting. On a shared host that turns into an opaque 500.30 with no way to
// read the cause, so the failure is recorded and surfaced through /api/health
// instead, leaving Swagger and the rest of the pipeline available.
string? startupError = null;

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<MeridianDbContext>();
        await context.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();

        var demoSeeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
        await demoSeeder.SeedAsync();

        logger.LogInformation("Database migrated and seeded successfully.");
    }
    catch (Exception ex)
    {
        startupError = $"{ex.GetType().Name}: {ex.Message}";
        logger.LogError(ex, "Database migration or seeding failed during startup.");
    }
}

// Swagger is served in every environment, not only development. The coursework
// requires evidence of API testing and the deployed instance is a demonstration
// system, so evaluators need to be able to exercise the endpoints directly.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Meridian API v1");
    options.DocumentTitle = "Meridian Talent Platform API";
});

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// The API root. Without this the base URL is a bare 404, which reads as a broken
// deployment to anyone checking the link. It points at the documentation instead.
app.MapGet("/", () => Results.Json(new
{
    service = "Meridian Talent Platform API",
    description = "AI-powered recruitment and talent management. SE205.3 Software Architecture, Group 4, NSBM Green University.",
    documentation = "/swagger",
    health = "/api/health",
    client = "https://meridian-talent.pages.dev"
})).AllowAnonymous();

/// <summary>
/// Liveness and dependency check. Reports whether the database is reachable and
/// whether the schema was applied at startup, which is the difference between
/// "the app is down" and "the app is up but its database is not".
/// </summary>
app.MapGet("/api/health", async (IConfiguration configuration) =>
{
    var canConnect = false;
    string? databaseError = null;

    try
    {
        // Deliberately not EF's CanConnectAsync. The context is configured with
        // EnableRetryOnFailure, so a health probe against a failing database
        // would sit through the whole retry schedule and time the caller out.
        // A health endpoint has to answer quickly, including when the answer is
        // bad news, so this opens a raw connection with a short timeout instead.
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(
            configuration.GetConnectionString("Default"))
        {
            ConnectTimeout = 5
        };

        await using var connection = new Microsoft.Data.SqlClient.SqlConnection(builder.ConnectionString);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(8));

        await connection.OpenAsync(cancellation.Token);
        canConnect = connection.State == System.Data.ConnectionState.Open;
    }
    catch (Exception ex)
    {
        databaseError = $"{ex.GetType().Name}: {ex.Message}";
    }

    var healthy = canConnect && startupError is null;

    return Results.Json(new
    {
        status = healthy ? "healthy" : "degraded",
        environment = app.Environment.EnvironmentName,
        databaseReachable = canConnect,
        startupError,
        databaseError,
        timestampUtc = DateTime.UtcNow
    }, statusCode: healthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable);
}).AllowAnonymous();

app.Run();

/// <summary>
/// Exposed so that the integration test project can drive the application
/// through WebApplicationFactory.
/// </summary>
public partial class Program;
