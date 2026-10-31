
using BelediyeBasvuru.Core.Entities;
using BelediyeBasvuru.Infrastructure.Data;
using BelediyeBasvuru.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BelediyeBasvuru.Web.Controllers;

public class BasvuruController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public BasvuruController(
        AppDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Olustur()
    {
        return View(new BasvuruModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Olustur(BasvuruModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        string? fotografYolu = null;

        if (model.Fotograf != null && model.Fotograf.Length > 0)
        {
            var izinVerilenUzantilar = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            var uzanti = Path.GetExtension(model.Fotograf.FileName)
                .ToLowerInvariant();

            if (!izinVerilenUzantilar.Contains(uzanti))
            {
                ModelState.AddModelError(
                    "Fotograf",
                    "Sadece JPG, JPEG, PNG ve WEBP dosyaları yüklenebilir.");

                return View(model);
            }

            if (model.Fotograf.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    "Fotograf",
                    "Fotoğraf boyutu en fazla 5 MB olabilir.");

                return View(model);
            }

            string klasor = Path.Combine(
                _environment.WebRootPath,
                "uploads");

            Directory.CreateDirectory(klasor);

            string dosyaAdi = Guid.NewGuid().ToString("N") + uzanti;

            string tamYol = Path.Combine(klasor, dosyaAdi);

            using (var stream = new FileStream(tamYol, FileMode.Create))
            {
                await model.Fotograf.CopyToAsync(stream);
            }

            fotografYolu = "/uploads/" + dosyaAdi;
        }

        var basvuru = new Basvuru
        {
            TakipNo = "BB-" +
                DateTime.UtcNow.ToString("yyyyMMdd") +
                "-" +
                Guid.NewGuid().ToString("N")[..6].ToUpper(),

            AdSoyad = model.AdSoyad,
            Telefon = model.Telefon,
            Email = model.Email,
            Kategori = model.Kategori,
            Baslik = model.Baslik,
            Aciklama = model.Aciklama,
            Konum = model.Konum,
            FotografYolu = fotografYolu,

            Durum = "Beklemede",
            OlusturulmaTarihi = DateTime.UtcNow,

            KullaniciId = User.FindFirstValue(
                ClaimTypes.NameIdentifier)
        };

        _context.Basvurular.Add(basvuru);

        await _context.SaveChangesAsync();

        TempData["Basarili"] =
            "Başvurunuz başarıyla kaydedildi. Takip numaranız: "
            + basvuru.TakipNo;

        return RedirectToAction(nameof(Olustur));
    }
}
