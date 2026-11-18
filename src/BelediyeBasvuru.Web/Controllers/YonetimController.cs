// IIstatistikServisi: panel verisini getiren servis sözleşmesi (Core).
using BelediyeBasvuru.Core.Interfaces;
// Politikalar.Yonetici sabiti.
using BelediyeBasvuru.Core.Yetki;
// [Authorize] özniteliği.
using Microsoft.AspNetCore.Authorization;
// Controller, IActionResult gibi MVC sınıfları.
using Microsoft.AspNetCore.Mvc;

namespace BelediyeBasvuru.Web.Controllers;

// Adres: "/Yonetim", "/Yonetim/Index" → Views/Yonetim/Index.cshtml
//
// H2-K1: Yalnız Yönetici rolü. Girişsiz → /Hesap/Giris; başka rol → /Hesap/ErisimEngellendi (403).
// Menüde bağlantıyı gizlemek yetki SAYILMAZ; kontrol sunucuda bu satırla yapılır (GG-08, GG-09).
[Authorize(Policy = Politikalar.Yonetici)]
public class YonetimController : Controller
{
    // "IIstatistikServisi istatistikServisi" → ASP.NET Core, DependencyInjection.cs'te kayıtlı
    // SahteIstatistikServisi örneğini otomatik olarak buraya verir (constructor injection).
    private readonly IIstatistikServisi _istatistikServisi;

    public YonetimController(IIstatistikServisi istatistikServisi)
    {
        _istatistikServisi = istatistikServisi;
    }

    // Adres: "/Yonetim" veya "/Yonetim/Index" → yönetim paneli (özet kartlar + grafikler).
    public async Task<IActionResult> Index()
    {
        // Servisten özet veriyi iste (şu an sahte/örnek veri; O-07 sonrası gerçek veri olacak).
        var ozet = await _istatistikServisi.OzetGetirAsync();

        // View(ozet) → Views/Yonetim/Index.cshtml'e @model olarak gönderilir.
        return View(ozet);
    }
}
