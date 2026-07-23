using Meridian.Application.Common.Interfaces;
using Meridian.Infrastructure.Identity;
using Meridian.Infrastructure.Persistence;
using Meridian.Infrastructure.Persistence.Repositories;
using Meridian.Infrastructure.Security;
using Meridian.Ai.Matching;
using Meridian.Ai.Strategies;
using Meridian.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Infrastructure;

/// <summary>
/// Composition root for the infrastructure layer. The API project calls this one
/// method rather than knowing which concrete types exist, which is what keeps the
/// dependency arrow pointing inward.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");

        services.AddDbContext<MeridianDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        // JwtSettings is bound and registered by the API project, which owns the
        // configuration sources. Binding it here would drag a configuration-binder
        // dependency into a layer that has no business reading configuration files.

        // Scoped: one unit of work per HTTP request, so everything a request does
        // shares a single DbContext and can commit as one transaction.
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IApplicationService, ApplicationService>();

        // The matching engine holds no mutable state between calls, so one
        // instance serves every request. The strategies it resolves are likewise
        // stateless value objects.
        // Each strategy is registered individually so the factory receives them
        // through IEnumerable<IRankingStrategy>. Registering only the factory
        // would hand it an empty collection, which the container satisfies
        // silently and which then fails deep inside scoring.
        services.AddSingleton<IRankingStrategy, SkillWeightedStrategy>();
        services.AddSingleton<IRankingStrategy, TfIdfSimilarityStrategy>();
        services.AddSingleton<IRankingStrategy, HybridStrategy>();
        services.AddSingleton<IRankingStrategy, ExperienceFirstStrategy>();
        services.AddSingleton<IRankingStrategyFactory, RankingStrategyFactory>();
        services.AddSingleton<IMatchingEngine, MatchingEngine>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<DemoDataSeeder>();

        // Stateless and thread-safe, so a single instance serves every request.
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
