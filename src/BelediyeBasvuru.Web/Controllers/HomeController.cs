// Activity: isteğin izleme (trace) numarasını almak için.
using System.Diagnostics;
// Controller, IActionResult, ResponseCache gibi MVC sınıfları.
using Microsoft.AspNetCore.Mvc;
// ErrorViewModel sınıfı için.
using BelediyeBasvuru.Web.Models;

namespace BelediyeBasvuru.Web.Controllers;

// Adı "...Controller" ile biten ve Controller'dan türeyen sınıflar MVC tarafından otomatik bulunur.
// HomeController → adreste "Home" olarak geçer.
public class HomeController : Controller
{
    // Adres: "/" veya "/Home/Index" → Views/Home/Index.cshtml sayfasını gösterir.
    public IActionResult Index()
    {
        // View() → metot adıyla aynı isimdeki görünümü (Index.cshtml) döndürür.
        return View();
    }

    // Adres: "/Home/Privacy" → Views/Home/Privacy.cshtml (KVKK / gizlilik sayfası).
    public IActionResult Privacy()
    {
        return View();
    }

    // Hata sayfası. Program.cs'teki UseExceptionHandler("/Home/Error") buraya yönlendirir.
    // ResponseCache(... NoStore = true) → hata sayfası tarayıcıda önbelleğe alınmasın.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Hatayı loglarda bulabilmek için istek numarasını modele koyup Views/Shared/Error.cshtml'e gönder.
        // "?." → Activity.Current null ise hata verme; "??" → null ise TraceIdentifier'ı kullan.
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
