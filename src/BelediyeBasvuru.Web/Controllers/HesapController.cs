using BelediyeBasvuru.Core.Entities;
using BelediyeBasvuru.Core.Yetki;
using BelediyeBasvuru.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BelediyeBasvuru.Web.Controllers;

/// <summary>
/// H2-K1: Kayıt, giriş, çıkış ve erişim reddi.
/// Yusuf'un PR #19'daki AccountController'ı temel alındı; tek hesap akışında birleştirildi:
///  - Kayıtta rol istemciden alınmaz, rol burada oluşturulmaz (roller migration ile gelir).
///  - Yanlış rol 403 sayfasına gider (giriş sayfasına değil).
///  - Dönüş adresi yalnız site içi olabilir (açık yönlendirme yok).
/// Adresler: GET/POST /Hesap/Kayit · GET/POST /Hesap/Giris · POST /Hesap/Cikis · /Hesap/ErisimEngellendi
/// </summary>
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class HesapController(
    UserManager<Kullanici> userManager,
    SignInManager<Kullanici> signInManager,
    ILogger<HesapController> logger) : Controller
{
    [HttpGet, AllowAnonymous]
    public IActionResult Kayit() => View(new KayitModeli());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Kayit(KayitModeli model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var eposta = model.Eposta.Trim();
        var kullanici = new Kullanici
        {
            UserName = eposta,
            Email = eposta,
            AdSoyad = model.AdSoyad.Trim()
        };

        var sonuc = await userManager.CreateAsync(kullanici, model.Parola);
        if (sonuc.Succeeded)
        {
            // Herkes Vatandaş olarak başlar. Personel/Yönetici rolü yalnız yönetici tarafından verilir (H6).
            sonuc = await userManager.AddToRoleAsync(kullanici, Roller.Vatandas);
            if (!sonuc.Succeeded)
            {
                await userManager.DeleteAsync(kullanici);
            }
        }

        if (!sonuc.Succeeded)
        {
            foreach (var hata in sonuc.Errors)
            {
                ModelState.AddModelError(string.Empty, hata.Description);
            }

            return View(model);
        }

        logger.LogInformation("Yeni vatandaş hesabı oluşturuldu: {KullaniciId}", kullanici.Id);
        await signInManager.SignInAsync(kullanici, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Giris(string? returnUrl = null) =>
        View(new GirisModeli { DonusAdresi = returnUrl });

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(GirisModeli model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // lockoutOnFailure: true → 5 hatalı denemede hesap 15 dk kilitlenir.
        var sonuc = await signInManager.PasswordSignInAsync(
            model.Eposta.Trim(), model.Parola, model.BeniHatirla, lockoutOnFailure: true);

        if (sonuc.Succeeded)
        {
            return Url.IsLocalUrl(model.DonusAdresi)
                ? LocalRedirect(model.DonusAdresi)
                : RedirectToAction("Index", "Home");
        }

        // Hesap var mı yok mu belli etmeyen genel mesaj. Kilitlenme ayrıca söylenir.
        ModelState.AddModelError(string.Empty, sonuc.IsLockedOut
            ? "Çok sayıda hatalı deneme nedeniyle hesabınız geçici olarak kilitlendi. 15 dakika sonra tekrar deneyin."
            : "E-posta veya parola hatalı.");
        model.Parola = string.Empty;
        return View(model);
    }

    // Çıkış yalnız POST + CSRF jetonu ile (bir bağlantıya tıklatarak kullanıcıyı çıkış yaptırmak mümkün olmasın).
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cikis()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet, AllowAnonymous]
    public IActionResult ErisimEngellendi()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}
