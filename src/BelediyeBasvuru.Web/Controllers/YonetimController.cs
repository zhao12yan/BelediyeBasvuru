// IIstatistikServisi: panel verisini getiren servis sözleşmesi (Core).
using BelediyeBasvuru.Core.Interfaces;
// Controller, IActionResult gibi MVC sınıfları.
using Microsoft.AspNetCore.Mvc;

namespace BelediyeBasvuru.Web.Controllers;

// Adres: "/Yonetim", "/Yonetim/Index" → Views/Yonetim/Index.cshtml
//
// K1-09 (roller/policy'ler) eklenince bu satır açılacak, sayfa sadece Yönetici rolüne kapanacak:
//     [Authorize(Roles = "Yonetici")]
// Şu an Identity henüz kurulmadığı (K1-08 beklemede) için [Authorize] eklenmedi;
// eklenirse Identity olmayan bir projede sayfa hiç açılmaz.
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
