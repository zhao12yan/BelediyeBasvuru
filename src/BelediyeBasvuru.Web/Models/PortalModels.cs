using System.ComponentModel.DataAnnotations;
using BelediyeBasvuru.Core.Enums;

namespace BelediyeBasvuru.Web.Models;

public static class PortalSecenekleri
{
    public static readonly string[] Kategoriler =
    [
        "Fen İşleri Müdürlüğü (Yol, Asfalt, Kaldırım)",
        "Park ve Bahçeler Müdürlüğü (Yeşil Alan, Ağaç, Park Bakımı)",
        "Temizlik İşleri Müdürlüğü (Çöp, Temizlik)",
        "Su ve Kanalizasyon Müdürlüğü (Su, Kanalizasyon)",
        "Zabıta Müdürlüğü (Denetim, Gürültü, İşgal)",
        "Ulaşım Hizmetleri Müdürlüğü (Ulaşım, Trafik)",
        "Diğer Talep ve Öneriler"
    ];

    public static readonly string[] Mahalleler =
    [
        "Batman Merkez - Bahçelievler Mahallesi", "Batman Merkez - Kültür Mahallesi",
        "Batman Merkez - Gültepe Mahallesi", "Batman Merkez - Belde Mahallesi",
        "Kozluk İlçe Merkezi", "Sason İlçe Merkezi", "Beşiri İlçe Merkezi",
        "Hasankeyf İlçe Merkezi", "Gercüş İlçe Merkezi"
    ];

    public static string DurumAdi(BasvuruDurumu durum) => durum switch
    {
        BasvuruDurumu.Yeni => "Yeni", BasvuruDurumu.Atandi => "Yeni Atandı",
        BasvuruDurumu.Islemde => "İşlemde", BasvuruDurumu.Cozuldu => "Çözüldü",
        BasvuruDurumu.Kapatildi => "Kapatıldı", BasvuruDurumu.Reddedildi => "Reddedildi",
        _ => "Bilinmiyor"
    };

    public static string DurumRengi(BasvuruDurumu durum) => durum switch
    {
        BasvuruDurumu.Yeni or BasvuruDurumu.Atandi => "warning",
        BasvuruDurumu.Islemde => "primary",
        BasvuruDurumu.Cozuldu or BasvuruDurumu.Kapatildi => "success",
        _ => "danger"
    };
}

public class PortalBasvuruFormu
{
    [Required(ErrorMessage = "Lütfen bir kategori seçiniz.")]
    public string Kategori { get; set; } = "";

    [Required(ErrorMessage = "Başvuru başlığını giriniz.")]
    [StringLength(120, MinimumLength = 5, ErrorMessage = "Başlık 5–120 karakter olmalıdır.")]
    public string Baslik { get; set; } = "";

    [Required(ErrorMessage = "Başvurunuzu açıklayınız.")]
    [StringLength(3000, MinimumLength = 20, ErrorMessage = "Açıklama 20–3000 karakter olmalıdır.")]
    public string Aciklama { get; set; } = "";

    [Required(ErrorMessage = "Lütfen mahalle / bölge seçiniz.")]
    public string Mahalle { get; set; } = "";

    [StringLength(300, ErrorMessage = "Adres en fazla 300 karakter olabilir.")]
    public string? Adres { get; set; }

    public List<IFormFile> Fotograflar { get; set; } = [];

    [Range(typeof(bool), "true", "true", ErrorMessage = "Devam etmek için onay vermelisiniz.")]
    public bool Onay { get; set; }
}

public class PortalBasvuruKaydi
{
    public string TakipNo { get; set; } = "";
    public string Baslik { get; set; } = "";
    public string Aciklama { get; set; } = "";
    public string Kategori { get; set; } = "";
    public string Mahalle { get; set; } = "";
    public string? Adres { get; set; }
    public string AdSoyad { get; set; } = "Önizleme Kullanıcısı";
    public DateTimeOffset Tarih { get; set; }
    public BasvuruDurumu Durum { get; set; }
    public List<PortalIslemNotu> Notlar { get; set; } = [];
    public string Birim => Kategori.Split(" (")[0];
}

public record PortalIslemNotu(string Personel, string Aciklama, DateTimeOffset Tarih);
public record PortalSorguModeli(string? TakipNo, PortalBasvuruKaydi? Basvuru, string? Mesaj);

public class PortalDurumFormu
{
    [Required]
    public string TakipNo { get; set; } = "";
    [EnumDataType(typeof(BasvuruDurumu))]
    public BasvuruDurumu Durum { get; set; }
    [Required(ErrorMessage = "İşlem notu giriniz.")]
    [StringLength(1000, MinimumLength = 5)]
    public string Not { get; set; } = "";
}
