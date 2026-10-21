using BelediyeBasvuru.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BelediyeBasvuru.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Altyapı servislerini (veritabanı, dosya, bildirim ...) kaydeder.
    /// Bağlantı dizesi: "ConnectionStrings:Default"
    ///  - Geliştirme: appsettings.Development.json (LocalDB) veya user-secrets
    ///  - CI: ConnectionStrings__Default ortam değişkeni (SQL Server 2022 konteyneri)
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("'ConnectionStrings:Default' bağlantı dizesi bulunamadı.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        // O-09: Sahte servisler hazır olunca buraya eklenir, ör.:
        // services.AddScoped<IBildirimServisi, SahteBildirimServisi>();
        // services.AddScoped<IDosyaDepolamaServisi, SahteDosyaServisi>();

        return services;
    }
}
