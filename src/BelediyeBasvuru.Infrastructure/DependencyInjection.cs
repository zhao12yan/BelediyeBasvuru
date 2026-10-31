
using BelediyeBasvuru.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BelediyeBasvuru.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Infrastructure katmanındaki servisleri uygulamaya kaydeder.
    /// Veritabanı bağlantısını yapılandırır.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // appsettings.json içerisinden bağlantı adresini al.
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "'ConnectionStrings:Default' bağlantı dizesi bulunamadı."
            );

        // Entity Framework Core ile SQL Server bağlantısını kaydet.
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString)
        );

        // İleride servis kayıtları buraya eklenecek.

        return services;
    }
}

