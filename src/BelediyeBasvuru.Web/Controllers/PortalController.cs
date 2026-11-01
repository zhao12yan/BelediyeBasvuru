using System.Security.Cryptography;
using System.Text.Json;
using BelediyeBasvuru.Core.Enums;
using BelediyeBasvuru.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BelediyeBasvuru.Web.Controllers;

// shortcut: O-07/K1-08 servisleri birleşene kadar yalnız Development'ta, oturuma özel önizleme verisi.
// Üretimde kayıt, personel işlemi veya sahte kimlik doğrulama sunulmaz.
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class PortalController(IWebHostEnvironment ortam) : Controller
{
    private const string OturumAnahtari = "Portal.Onizleme";

    [HttpGet]
    public IActionResult Olustur() => View(new PortalBasvuruFormu());

    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(16 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 16 * 1024 * 1024)]
    public async Task<IActionResult> Olustur(PortalBasvuruFormu model)
    {
        if (!ortam.IsDevelopment())
        {
            ModelState.AddModelError("", "Başvuru hizmeti henüz kullanıma açılmadı. Başvurunuz gönderilmedi.");
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View(model);
        }

        if (!PortalSecenekleri.Kategoriler.Contains(model.Kategori))
            ModelState.AddModelError(nameof(model.Kategori), "Listeden bir kategori seçiniz.");
        if (!PortalSecenekleri.Mahalleler.Contains(model.Mahalle))
            ModelState.AddModelError(nameof(model.Mahalle), "Listeden bir mahalle / bölge seçiniz.");
        if (model.Fotograflar.Count > 3)
            ModelState.AddModelError(nameof(model.Fotograflar), "En fazla 3 fotoğraf seçebilirsiniz.");

        foreach (var fotograf in model.Fotograflar.Take(3))
        {
            if (fotograf.Length is <= 0 or > 5 * 1024 * 1024 || !await FotografMi(fotograf))
                ModelState.AddModelError(nameof(model.Fotograflar), "Fotoğraflar JPG, PNG veya WebP olmalı ve dosya başına 5 MB'ı aşmamalıdır.");
        }

        if (!ModelState.IsValid) return View(model);
        var kayitlar = KayitlariGetir();
        if (kayitlar.Count >= 25)
        {
            ModelState.AddModelError("", "Bu önizleme oturumunda en fazla 25 başvuru tutulabilir.");
            return View(model);
        }

        string takipNo;
        do
        {
            var sayi = $"{DateTime.UtcNow.Year}{RandomNumberGenerator.GetInt32(100000, 1000000)}";
            var toplam = 0;
            for (var i = sayi.Length - 1; i >= 0; i--)
            {
                var rakam = sayi[i] - '0';
                if ((sayi.Length - i) % 2 == 1) rakam *= 2;
                toplam += rakam > 9 ? rakam - 9 : rakam;
            }
            takipNo = $"BLD-{sayi[..4]}-{sayi[4..]}-{(10 - toplam % 10) % 10}";
        } while (kayitlar.Any(k => k.TakipNo == takipNo));

        var tarih = DateTimeOffset.UtcNow;
        kayitlar.Add(new PortalBasvuruKaydi
        {
            TakipNo = takipNo, Baslik = model.Baslik.Trim(), Aciklama = model.Aciklama.Trim(),
            Kategori = model.Kategori, Mahalle = model.Mahalle, Adres = model.Adres?.Trim(),
            Tarih = tarih, Durum = BasvuruDurumu.Atandi,
            Notlar = [new("Sistem", "Önizleme kaydı oluşturuldu ve ilgili birime atandı.", tarih)]
        });
        Kaydet(kayitlar);
        TempData["PortalMesaj"] = "Önizleme kaydı oluşturuldu. Belediyeye gönderilmedi; fotoğraflar kalıcı olarak saklanmadı.";
        return RedirectToAction(nameof(Sorgula), new { takipNo });
    }

    [HttpGet]
    public IActionResult Sorgula(string? takipNo)
    {
        var no = takipNo?.Trim().ToUpperInvariant();
        var kayit = ortam.IsDevelopment() ? KayitlariGetir().FirstOrDefault(k => k.TakipNo == no) : null;
        var mesaj = string.IsNullOrEmpty(no) ? null : ortam.IsDevelopment()
            ? kayit is null ? "Bu takip numarasına ait başvuru bulunamadı." : null
            : "Başvuru sorgulama hizmeti henüz kullanıma açılmadı.";
        return View(new PortalSorguModeli(no, kayit, mesaj));
    }

    [HttpGet]
    public IActionResult Personel()
    {
        if (!ortam.IsDevelopment()) return NotFound();
        return View(KayitlariGetir().Where(k => k.Kategori == PortalSecenekleri.Kategoriler[0]).OrderByDescending(k => k.Tarih).ToList());
    }

    [HttpGet]
    public IActionResult Detay(string takipNo)
    {
        if (!ortam.IsDevelopment()) return NotFound();
        var kayit = KayitlariGetir().FirstOrDefault(k => k.TakipNo == takipNo && k.Kategori == PortalSecenekleri.Kategoriler[0]);
        return kayit is null ? NotFound() : PartialView("_Detay", kayit);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Guncelle(PortalDurumFormu model)
    {
        if (!ortam.IsDevelopment()) return NotFound();
        if (!ModelState.IsValid) return BadRequest("Geçerli bir durum ve en az 5 karakterlik işlem notu giriniz.");
        var kayitlar = KayitlariGetir();
        var kayit = kayitlar.FirstOrDefault(k => k.TakipNo == model.TakipNo && k.Kategori == PortalSecenekleri.Kategoriler[0]);
        if (kayit is null) return NotFound();
        if (!BasvuruGecisKurallari.GecisGecerliMi(kayit.Durum, model.Durum))
            return BadRequest("Bu durum geçişine izin verilmiyor.");
        kayit.Durum = model.Durum;
        kayit.Notlar.Add(new("Mehmet Demir (Saha Sorumlusu)", model.Not.Trim(), DateTimeOffset.UtcNow));
        Kaydet(kayitlar);
        TempData["PortalMesaj"] = "Önizleme kaydının durumu ve işlem notu güncellendi.";
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            return Json(new { url = Url.Action(nameof(Personel)) });
        return RedirectToAction(nameof(Personel));
    }

    [HttpGet]
    public IActionResult Giris() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult GirisKontrol()
    {
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        ViewData["GirisHatasi"] = "Kimlik doğrulama hizmeti henüz bağlı değil. Giriş yapılmadı. Gerçek hesap bilgilerinizi kullanmayınız.";
        return View("Giris");
    }

    private List<PortalBasvuruKaydi> KayitlariGetir()
    {
        var json = HttpContext.Session.GetString(OturumAnahtari);
        if (json is not null) return JsonSerializer.Deserialize<List<PortalBasvuruKaydi>>(json)!;
        var kayitlar = new List<PortalBasvuruKaydi>
        {
            Ornek("BLD-2026-000123-7", "Gültepe Mah. 4022 Sokak Asfalt Çökmesi", "Batman Merkez - Gültepe Mahallesi", BasvuruDurumu.Islemde, 24),
            Ornek("BLD-2026-000145-2", "Bahçelievler Kaldırım Taşı Kırıkları", "Batman Merkez - Bahçelievler Mahallesi", BasvuruDurumu.Atandi, 25),
            Ornek("BLD-2026-000189-9", "Belde Mah. Sokak Lambası / Aydınlatma", "Batman Merkez - Belde Mahallesi", BasvuruDurumu.Cozuldu, 23)
        };
        Kaydet(kayitlar);
        return kayitlar;
    }

    private void Kaydet(List<PortalBasvuruKaydi> kayitlar) =>
        HttpContext.Session.SetString(OturumAnahtari, JsonSerializer.Serialize(kayitlar));

    private static PortalBasvuruKaydi Ornek(string no, string baslik, string mahalle, BasvuruDurumu durum, int gun) => new()
    {
        TakipNo = no, Baslik = baslik, Mahalle = mahalle, Kategori = PortalSecenekleri.Kategoriler[0],
        Aciklama = "Sokak üzerindeki su giderinin yanında ciddi çökme var. Araç geçişlerinde tehlike arz ediyor.",
        AdSoyad = "Ahmet Yılmaz", Tarih = new(2026, 10, gun, 14, 32, 0, TimeSpan.FromHours(3)), Durum = durum,
        Notlar = [new("Sistem", "Otomatik atama yapıldı → Fen İşleri Müd.", new(2026, 10, gun, 14, 35, 0, TimeSpan.FromHours(3)))]
    };

    private static async Task<bool> FotografMi(IFormFile dosya)
    {
        var baslik = new byte[12];
        await using var akis = dosya.OpenReadStream();
        var uzunluk = await akis.ReadAtLeastAsync(baslik, 12, throwOnEndOfStream: false);
        var uzanti = Path.GetExtension(dosya.FileName).ToLowerInvariant();
        return uzunluk >= 12 && (uzanti switch
        {
            ".jpg" or ".jpeg" => baslik[0] == 0xff && baslik[1] == 0xd8 && baslik[2] == 0xff,
            ".png" => baslik.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".webp" => baslik.AsSpan(0, 4).SequenceEqual("RIFF"u8) && baslik.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        });
    }
}
