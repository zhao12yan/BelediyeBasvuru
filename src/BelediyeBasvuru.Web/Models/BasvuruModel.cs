
using System.ComponentModel.DataAnnotations;

namespace BelediyeBasvuru.Web.Models;

public class BasvuruModel
{
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    public string AdSoyad { get; set; } = "";

    [Required(ErrorMessage = "Telefon zorunludur.")]
    public string Telefon { get; set; } = "";

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Kategori seçiniz.")]
    public string Kategori { get; set; } = "";

    [Required(ErrorMessage = "Başlık zorunludur.")]
    public string Baslik { get; set; } = "";

    [Required(ErrorMessage = "Açıklama zorunludur.")]
    public string Aciklama { get; set; } = "";

    public IFormFile? Fotograf { get; set; }

    public string? Konum { get; set; }
}
