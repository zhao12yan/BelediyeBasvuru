using System.Security.Claims;
using BelediyeBasvuru.Core.Yetki;
using BelediyeBasvuru.Infrastructure.Kimlik;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BelediyeBasvuru.Web.Controllers;

[Authorize(Policy = Politikalar.Yonetici)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class KullaniciYonetimiController(HesapYonetimi yonetim) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? eposta)
    {
        HesapGorunumu? hesap = null;
        if (!string.IsNullOrWhiteSpace(eposta))
        {
            hesap = await yonetim.BulAsync(eposta);
            if (hesap is null) ModelState.AddModelError("", "Hesap bulunamadı.");
        }
        ViewBag.Birimler = yonetim.Birimler;
        return View(hesap);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Kaydet(string hedefId, string surum, HesapYetkisi yetki)
    {
        if (!ModelState.IsValid) return BadRequest("Hesap ve yetki bilgilerini kontrol edin.");
        var yapanId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (yapanId is null) return Forbid();
        var sonuc = await yonetim.DegistirAsync(yapanId, hedefId, surum, yetki,
            HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["Sonuc"] = sonuc.Succeeded ? "Hesap kaydedildi." : string.Join(" ", sonuc.Errors.Select(e => e.Description));
        return RedirectToAction(nameof(Index));
    }
}
