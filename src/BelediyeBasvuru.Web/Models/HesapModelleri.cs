using System.ComponentModel.DataAnnotations;

namespace BelediyeBasvuru.Web.Models;

/// <summary>Giriş formu (/Hesap/Giris). Alan adları Kişi 2'nin ekranlarıyla paylaşılır.</summary>
public class GirisModeli
{
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    [StringLength(254)]
    [Display(Name = "E-posta")]
    public string Eposta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola zorunludur.")]
    [StringLength(256)]
    [DataType(DataType.Password)]
    [Display(Name = "Parola")]
    public string Parola { get; set; } = string.Empty;

    [Display(Name = "Beni hatırla")]
    public bool BeniHatirla { get; set; }

    /// <summary>Girişten sonra dönülecek YEREL adres (dış adreslere yönlendirme yapılmaz).</summary>
    public string? DonusAdresi { get; set; }
}

/// <summary>
/// Vatandaş kayıt formu (/Hesap/Kayit).
/// BİLEREK rol alanı YOKTUR: formdan "Rol=Yonetici" gönderilse bile bağlanacak bir özellik bulunmaz.
/// </summary>
public class KayitModeli
{
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Ad soyad 3–100 karakter olmalıdır.")]
    [Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    [StringLength(254)]
    [Display(Name = "E-posta")]
    public string Eposta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola zorunludur.")]
    [StringLength(256, MinimumLength = 8, ErrorMessage = "Parola en az 8 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parola")]
    public string Parola { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola tekrarı zorunludur.")]
    [Compare(nameof(Parola), ErrorMessage = "Parolalar eşleşmiyor.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parola (tekrar)")]
    public string ParolaTekrar { get; set; } = string.Empty;
}
