using System.Globalization;
using System.Security.Claims;

namespace BelediyeBasvuru.Core.Yetki;

/// <summary>
/// H3-K1: Sahibi ve birimi olan her kayıt (ör. Basvuru) bu arayüzü uygular;
/// böylece "kim neye erişebilir?" kuralı TEK yerde yazılır ve her serviste aynı çalışır.
/// </summary>
/// <example>
/// public class Basvuru : IYetkiliKayit
/// {
///     public string? KullaniciId { get; set; }
///     public int? BirimId { get; set; }
///     string? IYetkiliKayit.SahipKullaniciId => KullaniciId;
/// }
/// </example>
public interface IYetkiliKayit
{
    /// <summary>Kaydı oluşturan vatandaşın kullanıcı Id'si (AspNetUsers.Id).</summary>
    string? SahipKullaniciId { get; }

    /// <summary>Kaydın atandığı birim. Henüz atanmadıysa null (yalnız yönetici görür).</summary>
    int? BirimId { get; }
}

/// <summary>
/// H3-K1: Kayıt düzeyindeki erişim kuralları (yetki matrisi: docs/tasarim/kimlik-yetki.md §8).
///  - Yönetici: tüm kayıtlar
///  - Birim personeli: yalnız KENDİ BİRİMİNE atanmış kayıtlar (birimi yoksa hiçbiri)
///  - Vatandaş: yalnız KENDİ oluşturduğu kayıtlar
///  - Diğer herkes (girişsiz dahil): hiçbiri
/// Tek kayıt için <see cref="ErisebilirMi"/>, liste sorguları için <see cref="YetkiliOlanlar{T}"/> kullanın.
/// </summary>
public static class ErisimKurallari
{
    public static bool ErisebilirMi(ClaimsPrincipal kullanici, IYetkiliKayit kayit)
    {
        if (kullanici.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (kullanici.IsInRole(Roller.Yonetici))
        {
            return true;
        }

        if (kullanici.IsInRole(Roller.BirimPersoneli) && BirimiAl(kullanici) is int birim && kayit.BirimId == birim)
        {
            return true;
        }

        return kullanici.IsInRole(Roller.Vatandas)
               && KullaniciIdAl(kullanici) is { } id
               && kayit.SahipKullaniciId == id;
    }

    /// <summary>
    /// Liste sorgusunu kullanıcının görebileceği kayıtlarla sınırlar (EF Core bunu SQL WHERE'e çevirir).
    /// Örn. <c>db.Basvurular.YetkiliOlanlar(User).ToListAsync()</c>
    /// </summary>
    public static IQueryable<T> YetkiliOlanlar<T>(this IQueryable<T> sorgu, ClaimsPrincipal kullanici)
        where T : class, IYetkiliKayit
    {
        if (kullanici.Identity?.IsAuthenticated != true)
        {
            return sorgu.Where(_ => false);
        }

        if (kullanici.IsInRole(Roller.Yonetici))
        {
            return sorgu;
        }

        var birim = kullanici.IsInRole(Roller.BirimPersoneli) ? BirimiAl(kullanici) : null;
        var sahipId = kullanici.IsInRole(Roller.Vatandas) ? KullaniciIdAl(kullanici) : null;

        if (birim is null && sahipId is null)
        {
            return sorgu.Where(_ => false);
        }

        return sorgu.Where(k => (birim != null && k.BirimId == birim) || (sahipId != null && k.SahipKullaniciId == sahipId));
    }

    /// <summary>Çerezdeki birim_id claim'i (yalnız birim personelinde).</summary>
    public static int? BirimiAl(ClaimsPrincipal kullanici) =>
        int.TryParse(kullanici.FindFirstValue(OzelClaimler.BirimId), NumberStyles.None, CultureInfo.InvariantCulture, out var birim)
            ? birim
            : null;

    private static string? KullaniciIdAl(ClaimsPrincipal kullanici) =>
        kullanici.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } id ? id : null;
}
