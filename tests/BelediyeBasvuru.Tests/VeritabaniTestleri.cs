using BelediyeBasvuru.Infrastructure.Data;
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
        var baglanti = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        if (string.IsNullOrWhiteSpace(baglanti))
        {
            return; // Veritabanı yok: atla.
        }

        // Her çalıştırmada ayrı, geçici bir veritabanı adı kullan.
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
            Assert.True(await context.Database.EnsureCreatedAsync());
            Assert.True(await context.Database.CanConnectAsync());
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }
}
