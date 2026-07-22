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
jwtSettings.Validate();
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

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
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
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MeridianDbContext>();
    await context.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Meridian API v1");
        options.DocumentTitle = "Meridian Talent Platform API";
    });
}

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Exposed so that the integration test project can drive the application
/// through WebApplicationFactory.
/// </summary>
public partial class Program;
