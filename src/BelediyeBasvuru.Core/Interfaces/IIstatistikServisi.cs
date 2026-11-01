// Bu dosyadaki tipler "BelediyeBasvuru.Core.Interfaces" ad alanına aittir.
namespace BelediyeBasvuru.Core.Interfaces;

/// <summary>
/// Yönetim paneli için özet istatistikleri sağlayan servisin sözleşmesi (K5-01/K5-02).
/// Core, Infrastructure'a bağımlı OLAMAZ; bu yüzden burada sadece arayüz ve DTO'lar var,
/// gerçek veritabanı sorgusu Infrastructure katmanında yapılacak implementasyonda yer alır.
/// </summary>
/// <remarks>
/// O-07 (şema) ve O-08 (durum geçiş tablosu) birleşene kadar bu arayüzün tek implementasyonu
/// <c>SahteIstatistikServisi</c>'dir (bkz. Infrastructure/Services) ve sabit/örnek veri döner.
/// Şema gelince gerçek veritabanı sorgusu yapan yeni bir sınıf yazılır; değişen sadece
/// DependencyInjection.cs'teki kayıt satırı olur, panel (controller/view) kodu değişmez.
/// Ayrıntılı tanım ve formüller: docs/analiz/gostergeler.md
/// </remarks>
public interface IIstatistikServisi
{
    /// <summary>Yönetim panelinde gösterilecek özet göstergeleri (G1-G5) getirir.</summary>
    Task<IstatistikOzeti> OzetGetirAsync();
}

/// <summary>
/// Yönetim panelinin üst kısmındaki sayı kartları ve grafikler için veri taşıyıcısı.
/// "record" seçildi: bu bir veritabanı varlığı (Entity) değil, salt-okunur bir anlık görüntü (snapshot).
/// </summary>
/// <param name="ToplamAcikBasvuru">G1: Çözüldü/Kapatıldı/Reddedildi DIŞINDAKİ başvuru sayısı.</param>
/// <param name="BugunkuYeniBasvuru">G2: Bugün oluşturulan başvuru sayısı.</param>
/// <param name="DurumDagilimi">G3: Her durumun adet/oranı.</param>
/// <param name="KategoriDagilimi">G4: Her kategorinin başvuru adedi.</param>
/// <param name="HaftalikTrend">G5: Son 7 gün, gün başına yeni başvuru sayısı (grafikte soldan sağa, eskiden yeniye).</param>
public sealed record IstatistikOzeti(
    int ToplamAcikBasvuru,
    int BugunkuYeniBasvuru,
    IReadOnlyList<DurumAdedi> DurumDagilimi,
    IReadOnlyList<KategoriAdedi> KategoriDagilimi,
    IReadOnlyList<GunlukAdet> HaftalikTrend);

/// <summary>Bir başvuru durumu ve o duruma sahip başvuru sayısı (G3, pasta grafik için).</summary>
public sealed record DurumAdedi(string Durum, int Adet);

/// <summary>Bir başvuru kategorisi ve o kategorideki başvuru sayısı (G4, bar grafik için).</summary>
public sealed record KategoriAdedi(string Kategori, int Adet);

/// <summary>Bir takvim günü ve o gün açılan başvuru sayısı (G5, çizgi grafik için).</summary>
public sealed record GunlukAdet(DateOnly Gun, int Adet);
