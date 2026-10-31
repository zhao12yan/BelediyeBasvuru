
namespace BelediyeBasvuru.Core.Entities;

public class Basvuru
{
    public int Id { get; set; }

    public string TakipNo { get; set; } = string.Empty;

    public string AdSoyad { get; set; } = string.Empty;

    public string Telefon { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Baslik { get; set; } = string.Empty;

    public string Aciklama { get; set; } = string.Empty;

    public string Kategori { get; set; } = string.Empty;

    public string? Konum { get; set; }

    public string? FotografYolu { get; set; }

    public double? Enlem { get; set; }

    public double? Boylam { get; set; }

    public string Durum { get; set; } = "Beklemede";

    public DateTime OlusturulmaTarihi { get; set; }
        = DateTime.UtcNow;

    public string? KullaniciId { get; set; }
}
