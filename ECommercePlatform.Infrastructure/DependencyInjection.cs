using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Infrastructure.Persistence;
using ECommercePlatform.Infrastructure.Persistence.Repositories;
using ECommercePlatform.Infrastructure.Security;
using ECommercePlatform.Infrastructure.Services;
using ECommercePlatform.Infrastructure.Services.Email;
using ECommercePlatform.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ECommercePlatform.Infrastructure;

/// <summary>
/// Infrastructure's composition root. Everything EF Core, cryptographic, or
/// otherwise environment-specific is bound to an Application interface here, so
/// this is the only assembly the rest of the system has to trust for those.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<FileStorageOptions>()
            .Bind(configuration.GetSection(FileStorageOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

                // RootPath in appsettings is kept relative ("Uploads") so the project
        // stays portable across machines/environments. Resolve it to an
        // absolute path here, rooted at the API's content root, so everything
        // downstream (including PhysicalFileResult, which requires a rooted
        // path) gets a full path — while the files still live inside the
        // project folder rather than some fixed drive/location.
        services.PostConfigure<FileStorageOptions>(options =>
        {
            if (!Path.IsPathRooted(options.RootPath))
            {
                options.RootPath = Path.Combine(environment.ContentRootPath, options.RootPath);
            }
        });

        // Optional: only used to grant the Admin role to a configured email on
        // start-up. See DatabaseInitializer.SeedPermissionsAsync.
        services.AddOptions<SeedOptions>()
            .Bind(configuration.GetSection(SeedOptions.SectionName));

        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName));


        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'ConnectionStrings:Default' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                // Transient SQL faults (failovers, throttling) are retried rather
                // than surfaced as 500s to the caller.
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);

                sql.CommandTimeout(30);
            });

            if (environment.IsDevelopment())
            {
                // Parameter values and richer errors are useful locally but would
                // leak credentials into logs anywhere else.
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // The context is the unit of work; resolving it through the interface keeps
        // the Application layer unaware of EF Core.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped(typeof(IAdminRepository<>), typeof(AdminRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
        services.AddScoped<ICategoryImageRepository, CategoryImageRepository>();
        services.AddScoped<ISubCategoryImageRepository, SubCategoryImageRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IBrandImageRepository, BrandImageRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();

services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddSingleton<IPassphraseHasher, PassphraseHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IOtpStore, InMemoryOtpStore>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();

        services.AddScoped<DatabaseInitializer>();
        services.AddHostedService<SessionCleanupService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ITaxRuleRepository, TaxRuleRepository>();
        services.AddScoped<IDeliveryRuleRepository, DeliveryRuleRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(name: "database", tags: ["ready"]);

        return services;
    }
}
