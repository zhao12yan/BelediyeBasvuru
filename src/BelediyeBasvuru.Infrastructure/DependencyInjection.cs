
using BelediyeBasvuru.Infrastructure.Data;
// IIstatistikServisi sözleşmesi için (Core, bu projeye bağımlı değil; biz Core'a bağımlıyız).
using BelediyeBasvuru.Core.Interfaces;
// AppDbContext sınıfını kullanmak için.
using BelediyeBasvuru.Infrastructure.Data;
// SahteIstatistikServisi implementasyonu için.
using BelediyeBasvuru.Infrastructure.Services;
// UseSqlServer(...) metodu için.
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

        // K5-02: Yönetim paneli istatistikleri — şema (O-07) gelene kadar sahte servis.
        services.AddScoped<IIstatistikServisi, SahteIstatistikServisi>();

        // Zincirleme çağrı yapılabilsin diye aynı listeyi geri döndür.
        return services;
    }
}

