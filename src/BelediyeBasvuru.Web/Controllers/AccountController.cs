
using BelediyeBasvuru.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BelediyeBasvuru.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<Kullanici> _userManager;
    private readonly SignInManager<Kullanici> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(
        UserManager<Kullanici> userManager,
        SignInManager<Kullanici> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    // Kayıt sayfası
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // Vatandaş kayıt işlemi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        string adSoyad,
        string email,
        string password)
    {
        if (string.IsNullOrWhiteSpace(adSoyad) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Tüm alanları doldurun.");
            return View();
        }

        email = email.Trim();

        // Vatandaş rolü yoksa oluştur.
        if (!await _roleManager.RoleExistsAsync("Vatandas"))
        {
            var rolSonuc = await _roleManager.CreateAsync(
                new IdentityRole("Vatandas"));

            if (!rolSonuc.Succeeded)
            {
                ModelState.AddModelError(
                    "", "Vatandaş rolü oluşturulamadı.");

                return View();
            }
        }

        var kullanici = new Kullanici
        {
            UserName = email,
            Email = email,
            AdSoyad = adSoyad.Trim()
        };

        // Kullanıcıyı oluştur.
        var sonuc = await _userManager.CreateAsync(
            kullanici, password);

        if (!sonuc.Succeeded)
        {
            foreach (var hata in sonuc.Errors)
            {
                ModelState.AddModelError("", hata.Description);
            }

            return View();
        }

        // Kullanıcıya vatandaş rolü ata.
        var rolAtamaSonucu = await _userManager.AddToRoleAsync(
            kullanici, "Vatandas");

        if (!rolAtamaSonucu.Succeeded)
        {
            await _userManager.DeleteAsync(kullanici);

            foreach (var hata in rolAtamaSonucu.Errors)
            {
                ModelState.AddModelError("", hata.Description);
            }

            return View();
        }

        // Kayıt başarılıysa otomatik giriş yap.
        await _signInManager.SignInAsync(kullanici, false);

        return RedirectToAction("Index", "Home");
    }

    // Giriş sayfası
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // Giriş işlemi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string email,
        string password)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
                "", "E-posta ve şifre zorunludur.");

            return View();
        }

        var sonuc = await _signInManager.PasswordSignInAsync(
            email.Trim(),
            password,
            false,
            true);

        if (sonuc.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(
            "", "E-posta veya şifre hatalı.");

        return View();
    }

    // Çıkış işlemi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }
}
