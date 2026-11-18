using BelediyeBasvuru.Core.Entities;
using BelediyeBasvuru.Core.Yetki;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BelediyeBasvuru.Infrastructure.Data;

/// <summary>
/// H1-K1 / H2-K1: Kurulum sırası her ortamda AYNI:
///   1) veritabanı erişimi → 2) migration (tablolar + 3 rol) → 3) örnek hesaplar (yalnız geliştirme).
/// Docker ve yerel geliştirmede uygulama açılışında çalışır (Veritabani:BaslangictaHazirla=true);
/// CI'da ve elle kurulumda aynı adımlar "dotnet ef database update" ile yapılır.
/// </summary>
public static class VeritabaniHazirlayici
{
    /// <summary>Örnek hesap ayarı (appsettings "OrnekHesaplar" listesi). Parola depoya YAZILMAZ.</summary>
    public sealed class OrnekHesap
    {
        public string Eposta { get; set; } = string.Empty;
        public string AdSoyad { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int? BirimId { get; set; }
        /// <summary>user-secrets, ortam değişkeni veya .env ile verilir. Boşsa hesap oluşturulmaz.</summary>
        public string? Parola { get; set; }
    }

    public static async Task HazirlaAsync(IServiceProvider servisler, IConfiguration ayarlar, ILogger logger,
        CancellationToken iptal = default)
    {
        using var kapsam = servisler.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1–2) Bağlantı + migration. Hata olursa uygulama açık bir mesajla durur (sessizce "çalışıyor" görünmez).
        logger.LogInformation("Veritabanı migration'ları uygulanıyor...");
        await db.Database.MigrateAsync(iptal);

        // 3) Örnek hesaplar (yalnız ayarda listelenmiş ve parolası verilmiş olanlar).
        var hesaplar = ayarlar.GetSection("OrnekHesaplar").Get<List<OrnekHesap>>() ?? [];
        if (hesaplar.Count == 0)
        {
            return;
        }

        var userManager = kapsam.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
        foreach (var hesap in hesaplar)
        {
            await OrnekHesapOlusturAsync(userManager, hesap, logger);
        }
    }

    private static async Task OrnekHesapOlusturAsync(UserManager<Kullanici> userManager, OrnekHesap hesap, ILogger logger)
    {
        if (!Roller.Tumu.Contains(hesap.Rol))
        {
            logger.LogWarning("Örnek hesap {Eposta}: bilinmeyen rol '{Rol}', atlandı.", hesap.Eposta, hesap.Rol);
            return;
        }

        // Var olan hesaba DOKUNMA: parola sıfırlanmaz, rol değiştirilmez.
        if (await userManager.FindByEmailAsync(hesap.Eposta) is not null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(hesap.Parola))
        {
            logger.LogWarning("Örnek hesap {Eposta} için parola verilmedi (OrnekHesaplar:N:Parola); hesap oluşturulmadı.", hesap.Eposta);
            return;
        }

        var kullanici = new Kullanici
        {
            UserName = hesap.Eposta,
            Email = hesap.Eposta,
            EmailConfirmed = true,
            AdSoyad = hesap.AdSoyad,
            BirimId = hesap.Rol == Roller.BirimPersoneli ? hesap.BirimId : null
        };

        var sonuc = await userManager.CreateAsync(kullanici, hesap.Parola);
        if (sonuc.Succeeded)
        {
            sonuc = await userManager.AddToRoleAsync(kullanici, hesap.Rol);
        }

        if (!sonuc.Succeeded)
        {
            logger.LogWarning("Örnek hesap {Eposta} oluşturulamadı: {Hatalar}", hesap.Eposta,
                string.Join("; ", sonuc.Errors.Select(h => h.Description)));
            return;
        }

        logger.LogInformation("Örnek hesap oluşturuldu: {Eposta} ({Rol})", hesap.Eposta, hesap.Rol);
    }
}
