// IIstatistikServisi ve DTO'lar (IstatistikOzeti, DurumAdedi ...) için.
using BelediyeBasvuru.Core.Interfaces;

namespace BelediyeBasvuru.Infrastructure.Services;

/// <summary>
/// IIstatistikServisi'nin GEÇİCİ implementasyonu (O-09 "arayüz + sahte servis" kuralı).
/// Gerçek <c>Basvuru</c> tablosu henüz yok (O-07/O-08 beklemede); bu sınıf veritabanına
/// hiç dokunmadan, panelin tasarımını ve grafiklerini şimdiden test edebilmek için
/// sabit/örnek veri döner.
/// </summary>
/// <remarks>
/// Şema birleşince bu sınıfın yerine <c>AppDbContext</c> kullanan gerçek bir
/// <c>IstatistikServisi</c> yazılacak ve DependencyInjection.cs'teki tek satır değişecek:
/// <c>services.AddScoped&lt;IIstatistikServisi, IstatistikServisi&gt;();</c>
/// Panel kodunda (YonetimController, Index.cshtml) HİÇBİR değişiklik gerekmeyecek.
/// </remarks>
public sealed class SahteIstatistikServisi : IIstatistikServisi
{
    public Task<IstatistikOzeti> OzetGetirAsync()
    {
        // Gerçek sorgu olmadığı için veritabanına gidilmiyor; elle yazılmış örnek veri döndürülüyor.
        // Sayılar docs/analiz/gostergeler.md'deki tanımlarla tutarlı (durum/kategori adları orada da geçer).
        var durumDagilimi = new List<DurumAdedi>
        {
            new("Yeni", 18),
            new("İnceleniyor", 11),
            new("Atandı", 7),
            new("Çözüldü", 42),
            new("Reddedildi", 3),
        };

        var kategoriDagilimi = new List<KategoriAdedi>
        {
            new("Yol / Kaldırım", 24),
            new("Aydınlatma", 16),
            new("Çevre / Temizlik", 19),
            new("Su / Kanalizasyon", 12),
            new("Diğer", 10),
        };

        // Son 7 gün için örnek bir trend: bugünden başlayıp geriye gidiyoruz, sonra sırayı
        // (eskiden yeniye) çeviriyoruz ki grafikte soldan sağa doğru akış doğru görünsün.
        var bugun = DateOnly.FromDateTime(DateTime.Today);
        var ornekGunlukAdetler = new[] { 9, 7, 12, 8, 14, 10, 6 };
        var haftalikTrend = Enumerable.Range(0, 7)
            .Select(i => new GunlukAdet(bugun.AddDays(-i), ornekGunlukAdetler[i]))
            .Reverse()
            .ToList();

        var toplamAcik = durumDagilimi
            .Where(d => d.Durum is "Yeni" or "İnceleniyor" or "Atandı")
            .Sum(d => d.Adet);

        var ozet = new IstatistikOzeti(
            ToplamAcikBasvuru: toplamAcik,
            BugunkuYeniBasvuru: ornekGunlukAdetler[0],
            DurumDagilimi: durumDagilimi,
            KategoriDagilimi: kategoriDagilimi,
            HaftalikTrend: haftalikTrend);

        // Gerçek bir veritabanı sorgusu olmadığı için senkron hesaplanan sonucu
        // Task.FromResult ile "tamamlanmış görev" olarak döndürüyoruz; arayüz async kalsın diye
        // (ileride gerçek EF Core sorgusu await gerektirecek, imza o yüzden Task<T> döner).
        return Task.FromResult(ozet);
    }
}
