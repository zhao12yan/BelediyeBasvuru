// IdentityUser sınıfını kullanabilmek için Identity kütüphanesini içe alıyoruz.
using Microsoft.AspNetCore.Identity;

// Bu dosyadaki sınıflar "BelediyeBasvuru.Core.Entities" ad alanına (namespace) aittir.
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
// ": IdentityUser" → Kullanici, IdentityUser'ın tüm alanlarını miras alır (Id, Email, PasswordHash ...).
public class Kullanici : IdentityUser
{
    // Kullanıcının adı ve soyadı. Veritabanında AspNetUsers tablosunda "AdSoyad" sütunu olur.
    // "= string.Empty" → başlangıç değeri boş metin; böylece null olmaz.
    public string AdSoyad { get; set; } = string.Empty;
}
