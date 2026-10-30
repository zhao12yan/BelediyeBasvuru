// AppDbContext sınıfını kullanmak için.
using BelediyeBasvuru.Infrastructure.Data;
// UseSqlServer(...) metodu için.
using Microsoft.EntityFrameworkCore;
// IConfiguration: appsettings.json ve ortam değişkenlerindeki ayarları okur.
using Microsoft.Extensions.Configuration;
// IServiceCollection: uygulamanın servis kayıt listesi (Dependency Injection kabı).
using Microsoft.Extensions.DependencyInjection;

namespace BelediyeBasvuru.Infrastructure;

// "static class" → nesnesi oluşturulmaz; sadece yardımcı metot barındırır.
public static class DependencyInjection
{
    /// <summary>
    /// Altyapı servislerini (veritabanı, dosya, bildirim ...) kaydeder.
    /// Bağlantı dizesi: "ConnectionStrings:Default"
    ///  - Geliştirme: appsettings.Development.json (LocalDB) veya user-secrets
    ///  - Docker: docker-compose.yml içindeki ConnectionStrings__Default ortam değişkeni
    ///  - CI: ConnectionStrings__Default ortam değişkeni (SQL Server 2022 konteyneri)
    /// </summary>
    // "this IServiceCollection services" → uzantı metodu: Program.cs'te builder.Services.AddInfrastructure(...) diye çağrılır.
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Ayarlardan "Default" adlı bağlantı dizesini oku.
        // "?? throw ..." → bulunamazsa (null ise) uygulamayı açık bir hata mesajıyla durdur.
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("'ConnectionStrings:Default' bağlantı dizesi bulunamadı.");

        // AppDbContext'i servis olarak kaydet ve SQL Server kullanacağını söyle.
        // Artık bir controller'ın kurucusuna "AppDbContext db" yazmak yeterli; nesneyi sistem verir.
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        // O-09: Sahte servisler hazır olunca buraya eklenir, ör.:
        // services.AddScoped<IBildirimServisi, SahteBildirimServisi>();
        // services.AddScoped<IDosyaDepolamaServisi, SahteDosyaServisi>();
        // Gerçek servis hazır olunca sadece sağdaki sınıf adı değiştirilir.

        // Zincirleme çağrı yapılabilsin diye aynı listeyi geri döndür.
        return services;
    }
}
