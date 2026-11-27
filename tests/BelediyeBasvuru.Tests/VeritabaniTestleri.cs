// Test edeceğimiz AppDbContext ve Kullanici sınıfları.
using BelediyeBasvuru.Core.Entities;
using BelediyeBasvuru.Infrastructure.Data;
// DbContextOptionsBuilder ve UseSqlServer için.
using Microsoft.EntityFrameworkCore;

namespace BelediyeBasvuru.Tests;

/// <summary>
/// Gerçek SQL Server'a karşı modelin oluşturulabildiğini doğrular.
/// CI'da ConnectionStrings__Default (SQL Server 2022 konteyneri) ile çalışır.
/// Bu değişken yoksa (ör. yerel makine) test hiçbir şey yapmadan geçer.
/// </summary>
public class VeritabaniTestleri
{
    [Fact]
    public async Task Model_SqlServerda_Olusturulabilir()
    {
        // Bağlantı dizesini ortam değişkeninden oku (CI bunu ayarlar).
        var baglanti = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        // Değişken yoksa veya boşsa:
        if (string.IsNullOrWhiteSpace(baglanti))
        {
            return; // Veritabanı yok: atla.
        }

        // Her çalıştırmada ayrı, geçici bir veritabanı adı kullan.
        // Böylece testler birbirinin verisini bozmaz. Guid → benzersiz rastgele kimlik.
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(baglanti)
        {
            InitialCatalog = $"BelediyeBasvuru_Test_{Guid.NewGuid():N}"
        };

        // AppDbContext'e "bu SQL Server'ı kullan" ayarını hazırla.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(builder.ConnectionString)
            .Options;

        // "await using" → iş bitince bağlantıyı otomatik kapat.
        await using var context = new AppDbContext(options);
        try
        {
            // Modeldeki tüm tabloları oluştur; true dönmeli (yeni veritabanı açıldı).
            Assert.True(await context.Database.EnsureCreatedAsync());
            // Veritabanına bağlanılabildiğini doğrula.
            Assert.True(await context.Database.CanConnectAsync());
        }
        finally
        {
            // Test geçse de kalsa da geçici veritabanını sil; geride çöp kalmasın.
            await context.Database.EnsureDeletedAsync();
        }
    }

    [Fact]
    public async Task Kullanici_SqlServera_KaydedilipOkunabilir()
    {
        var baglanti = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        if (string.IsNullOrWhiteSpace(baglanti))
        {
            return;
        }

        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(baglanti)
        {
            InitialCatalog = $"BelediyeBasvuru_Test_{Guid.NewGuid():N}"
        };

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(builder.ConnectionString)
            .Options;

        await using var context = new AppDbContext(options);
        try
        {
            await context.Database.EnsureCreatedAsync();

            var kullanici = new Kullanici
            {
                UserName = "vatandas.test@ornek.test",
                NormalizedUserName = "VATANDAS.TEST@ORNEK.TEST",
                Email = "vatandas.test@ornek.test",
                NormalizedEmail = "VATANDAS.TEST@ORNEK.TEST",
                AdSoyad = "Test Vatandaş"
            };

            context.Users.Add(kullanici);
            await context.SaveChangesAsync();

            var kayit = await context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == "vatandas.test@ornek.test");

            Assert.NotNull(kayit);
            Assert.Equal("Test Vatandaş", kayit.AdSoyad);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
}
