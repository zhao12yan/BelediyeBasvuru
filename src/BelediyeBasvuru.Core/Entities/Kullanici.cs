using Microsoft.AspNetCore.Identity;

namespace BelediyeBasvuru.Core.Entities;

/// <summary>
/// Sistemdeki kullanıcı (Vatandaş, Birim Personeli, Yönetici).
/// ASP.NET Core Identity'nin IdentityUser sınıfından türer; parola özeti, e-posta,
/// telefon, kilitleme alanları oradan gelir.
/// </summary>
/// <remarks>
/// Kişi 4 (O-07): Ek alanlar (ör. BirimId, AktifMi) şema v1'de buraya eklenecek.
/// Kişi 1 (K1-08): Identity kurulumu bu sınıfı kullanır.
/// </remarks>
public class Kullanici : IdentityUser
{
    public string AdSoyad { get; set; } = string.Empty;
}
