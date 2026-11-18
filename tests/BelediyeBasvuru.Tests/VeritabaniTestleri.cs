// Test edeceğimiz AppDbContext sınıfı.
using BelediyeBasvuru.Infrastructure.Data;
// DbContextOptionsBuilder ve UseSqlServer için.
using Microsoft.EntityFrameworkCore;

namespace BelediyeBasvuru.Tests;

/// <summary>
/// Gerçek SQL Server'a karşı modelin oluşturulabildiğini doğrular.
/// CI'da ConnectionStrings__Default (SQL Server 2022 konteyneri) ile çalışır.
/// </summary>
/// <remarks>
/// H1-K1: Bağlantı yoksa test eskiden sessizce "Başarılı" sayılıyordu; bu, veritabanı hiç
/// denenmediği hâlde yanlış güven veriyordu. Artık:
///  - Bağlantı yok  → test "Atlandı" (Skipped) olarak görünür, nedeni yazılır.
///  - VERITABANI_TESTI_ZORUNLU=1 (CI) ve bağlantı yok → test KIRMIZI olur.
/// Yerelde denemek için: $env:ConnectionStrings__Default="Server=(localdb)\mssqllocaldb;Trusted_Connection=True;TrustServerCertificate=True"
/// </remarks>
public class VeritabaniTestleri
{
    [SkippableFact]
    public async Task Model_SqlServerda_Olusturulabilir()
    {
        var baglanti = VeritabaniOrtami.BaglantiyiAlVeyaAtla();

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
}

/// <summary>
/// Gerçek SQL Server isteyen testlerin ortak ön koşul kontrolü.
/// </summary>
internal static class VeritabaniOrtami
{
    /// <summary>
    /// ConnectionStrings__Default'u döndürür. Yoksa testi atlar; CI'da (VERITABANI_TESTI_ZORUNLU=1) testi düşürür.
    /// </summary>
    public static string BaglantiyiAlVeyaAtla()
    {
        var baglanti = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        if (!string.IsNullOrWhiteSpace(baglanti))
        {
            return baglanti;
        }

        // CI veritabanını kurmayı unuttuysa bunu "geçti" diye saklamayalım.
        if (Environment.GetEnvironmentVariable("VERITABANI_TESTI_ZORUNLU") == "1")
        {
            Assert.Fail("VERITABANI_TESTI_ZORUNLU=1 ama ConnectionStrings__Default tanımlı değil; gerçek SQL testi çalıştırılamadı.");
        }

        Skip.If(true, "ConnectionStrings__Default tanımlı değil; gerçek SQL Server testi bu makinede ÇALIŞTIRILMADI.");
        return string.Empty; // Buraya gelinmez (Skip istisna fırlatır).
    }
}
