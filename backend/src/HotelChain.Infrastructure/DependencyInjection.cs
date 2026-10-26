using HotelChain.Application.Auth;
using HotelChain.Domain.Repositories;
using HotelChain.Infrastructure.Auth;
using HotelChain.Infrastructure.Persistence;
using HotelChain.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HotelChain.Infrastructure;

public static class DependencyInjection
{
    // Migrations are provider specific, so each provider keeps them in its own assembly.
    private const string SqliteMigrationsAssembly = "HotelChain.Migrations.Sqlite";
    private const string SqlServerMigrationsAssembly = "HotelChain.Migrations.SqlServer";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var dbSection = configuration.GetSection(DatabaseOptions.SectionName);
        services.Configure<DatabaseOptions>(dbSection);
        var dbOptions = dbSection.Get<DatabaseOptions>() ?? new DatabaseOptions();
        var connectionString = configuration.GetConnectionString("HotelChain");

        services.AddDbContext<HotelChainDbContext>(options =>
        {
            switch (dbOptions.Provider.ToLowerInvariant())
            {
                case "sqlserver":
                    options.UseSqlServer(connectionString, sql => sql
                        .MigrationsAssembly(SqlServerMigrationsAssembly)
                        .EnableRetryOnFailure());
                    break;
                case "inmemory":
                    options.UseInMemoryDatabase(connectionString ?? "HotelChain");
                    break;
                case "sqlite":
                    options.UseSqlite(connectionString ?? "Data Source=hotelchain.db", sqlite => sqlite
                        .MigrationsAssembly(SqliteMigrationsAssembly));
                    break;
                default:
                    throw new InvalidOperationException($"Proveedor de base de datos no soportado: {dbOptions.Provider}");
            }
        });

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IHotelRepository, HotelRepository>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<AdminUserOptions>(configuration.GetSection(AdminUserOptions.SectionName));
        services.AddScoped<IAuthService, JwtAuthService>();

        return services;
    }
}
