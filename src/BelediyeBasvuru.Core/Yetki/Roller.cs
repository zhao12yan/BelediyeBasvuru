namespace BelediyeBasvuru.Core.Yetki;

/// <summary>
/// H2-K1: Sistemdeki üç rolün TEK kaynağı. Rol adını hiçbir yerde elle yazmayın
/// ("Admin", "Personel" gibi farklı yazımlar yetki açığı doğurur); bu sabitleri kullanın.
/// </summary>
/// <remarks>
/// Roller veritabanına migration ile gelir (bkz. Infrastructure/Data/Configurations/RolYapilandirmasi.cs).
/// Yeni kayıt olan herkes <see cref="Vatandas"/> olur; rol formdan ASLA alınmaz.
/// </remarks>
public static class Roller
{
    /// <summary>Başvuru yapan, yalnızca KENDİ başvurularını gören kullanıcı.</summary>
    public const string Vatandas = "Vatandas";

    /// <summary>Bir birime bağlı personel; yalnızca KENDİ BİRİMİNİN başvurularını görür/işler.</summary>
    public const string BirimPersoneli = "BirimPersoneli";

    /// <summary>Tüm başvuruları, istatistikleri ve yönetim ekranlarını gören kullanıcı.</summary>
    public const string Yonetici = "Yonetici";

    /// <summary>Tüm roller (seed ve doğrulama için).</summary>
    public static readonly IReadOnlyList<string> Tumu = [Vatandas, BirimPersoneli, Yonetici];
}

/// <summary>
/// Policy adları. Controller'da <c>[Authorize(Policy = Politikalar.Yonetici)]</c> şeklinde kullanılır.
/// Tanımları Web/Guvenlik/KimlikKurulumu.cs içindedir.
/// </summary>
public static class Politikalar
{
    public const string Vatandas = "VatandasPolicy";
    public const string BirimPersoneli = "BirimPersoneliPolicy";
    public const string Yonetici = "YoneticiPolicy";
    /// <summary>Personel ekranları: Birim personeli VEYA yönetici.</summary>
    public const string PersonelEkrani = "PersonelEkraniPolicy";
    /// <summary>
    /// H3-K1: Tek bir kayda (ör. başvuru) erişim — sahiplik / aynı birim / yönetici.
    /// <c>await authorizationService.AuthorizeAsync(User, basvuru, Politikalar.KayitErisimi)</c>
    /// </summary>
    public const string KayitErisimi = "KayitErisimiPolicy";
}

/// <summary>Kimlik çerezine eklenen özel claim adları.</summary>
public static class OzelClaimler
{
    /// <summary>Personelin bağlı olduğu birim (Kullanici.BirimId). Vatandaşta yoktur.</summary>
    public const string BirimId = "birim_id";
}
