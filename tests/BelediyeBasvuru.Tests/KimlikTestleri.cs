using System.Net;
using BelediyeBasvuru.Core.Entities;
using BelediyeBasvuru.Core.Yetki;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BelediyeBasvuru.Tests;

/// <summary>
/// H2-K1 kabul koşulları: gerçek kayıt/giriş/çıkış, hatalı giriş, girişsiz erişim, yanlış rol (403),
/// rol yükseltme denemesi, hesap kilitleme ve açık yönlendirme.
/// </summary>
public class KimlikTestleri(TestUygulamasi uygulama) : IClassFixture<TestUygulamasi>
{
    private static Dictionary<string, string> KayitFormu(string eposta) => new()
    {
        ["AdSoyad"] = "Deneme Vatandaş",
        ["Eposta"] = eposta,
        ["Parola"] = TestUygulamasi.TestParolasi,
        ["ParolaTekrar"] = TestUygulamasi.TestParolasi
    };

    [Fact]
    public async Task Kayit_YeniKullaniciVatandasOlur_FormdanGelenRolYokSayilir()
    {
        var istemci = uygulama.Istemci();
        var form = KayitFormu("yeni-vatandas@test.local");
        form["Rol"] = Roller.Yonetici;          // Saldırgan rol yükseltmeyi dener.
        form["BirimId"] = "1";

        var yanit = await FormYardimcisi.GonderAsync(istemci, "/Hesap/Kayit", "/Hesap/Kayit", form);

        Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
        using var kapsam = uygulama.Services.CreateScope();
        var userManager = kapsam.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
        var kullanici = await userManager.FindByEmailAsync("yeni-vatandas@test.local");
        Assert.NotNull(kullanici);
        Assert.Null(kullanici.BirimId);
        Assert.Equal([Roller.Vatandas], await userManager.GetRolesAsync(kullanici));

        // Kayıt sonrası otomatik giriş yapıldı ama yönetim paneline giremez.
        var panel = await istemci.GetAsync("/Yonetim");
        Assert.Contains("/Hesap/ErisimEngellendi", panel.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Kayit_ZayifParola_Reddedilir()
    {
        var istemci = uygulama.Istemci();
        var form = KayitFormu("zayif@test.local");
        form["Parola"] = form["ParolaTekrar"] = "kucukharf1";

        var yanit = await FormYardimcisi.GonderAsync(istemci, "/Hesap/Kayit", "/Hesap/Kayit", form);

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode); // Form hatayla tekrar gösterilir.
        using var kapsam = uygulama.Services.CreateScope();
        var userManager = kapsam.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
        Assert.Null(await userManager.FindByEmailAsync("zayif@test.local"));
    }

    [Fact]
    public async Task Giris_DogruParola_OturumAcar()
    {
        var istemci = uygulama.Istemci();

        var yanit = await FormYardimcisi.GirisYapAsync(istemci, "vatandas@test.local");

        Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
        Assert.Contains(yanit.Headers.GetValues("Set-Cookie"), c => c.StartsWith(".BelediyeBasvuru.Kimlik="));
        Assert.Contains("vatandas@test.local", await istemci.GetStringAsync("/"));
    }

    [Fact]
    public async Task Giris_YanlisParola_BasariGostermez()
    {
        var istemci = uygulama.Istemci();

        var yanit = await FormYardimcisi.GirisYapAsync(istemci, "personel1@test.local", "YanlisParola9");

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        // Razor Türkçe harfleri HTML olarak kodlar (ı → &#x131;); karşılaştırmadan önce çözüyoruz.
        Assert.Contains("E-posta veya parola hatalı.", WebUtility.HtmlDecode(await yanit.Content.ReadAsStringAsync()));
        Assert.False(yanit.Headers.TryGetValues("Set-Cookie", out var cerezler)
                     && cerezler.Any(c => c.StartsWith(".BelediyeBasvuru.Kimlik=")));
    }

    [Fact]
    public async Task Girissiz_YonetimPaneli_GirisSayfasinaYonlenir()
    {
        var yanit = await uygulama.Istemci().GetAsync("/Yonetim");

        Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
        Assert.Contains("/Hesap/Giris", yanit.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("vatandas@test.local")]
    [InlineData("personel1@test.local")]
    public async Task YanlisRol_YonetimPaneli_403Alir(string eposta)
    {
        var istemci = uygulama.Istemci();
        await FormYardimcisi.GirisYapAsync(istemci, eposta);

        var yanit = await istemci.GetAsync("/Yonetim");
        Assert.Contains("/Hesap/ErisimEngellendi", yanit.Headers.Location?.OriginalString);

        var engel = await istemci.GetAsync(yanit.Headers.Location);
        Assert.Equal(HttpStatusCode.Forbidden, engel.StatusCode);
    }

    [Fact]
    public async Task BesHataliDeneme_HesabiKilitler()
    {
        using (var kapsam = uygulama.Services.CreateScope())
        {
            var userManager = kapsam.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
            var k = new Kullanici { UserName = "kilit@test.local", Email = "kilit@test.local", AdSoyad = "Kilit" };
            Assert.True((await userManager.CreateAsync(k, TestUygulamasi.TestParolasi)).Succeeded);
        }

        var istemci = uygulama.Istemci();
        for (var i = 0; i < 5; i++)
        {
            await FormYardimcisi.GirisYapAsync(istemci, "kilit@test.local", "YanlisParola9");
        }

        // Doğru parola bile artık kabul edilmez.
        var yanit = await FormYardimcisi.GirisYapAsync(istemci, "kilit@test.local");

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        Assert.Contains("kilitlendi", await yanit.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Giris_DisAdreseYonlendirmez()
    {
        var istemci = uygulama.Istemci();

        var yanit = await FormYardimcisi.GonderAsync(istemci, "/Hesap/Giris", "/Hesap/Giris", new Dictionary<string, string>
        {
            ["Eposta"] = "vatandas@test.local",
            ["Parola"] = TestUygulamasi.TestParolasi,
            ["DonusAdresi"] = "https://kotu-site.example/tuzak"
        });

        Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
        Assert.Equal("/", yanit.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Cikis_GetIleYapilamaz()
    {
        var yanit = await uygulama.Istemci().GetAsync("/Hesap/Cikis");

        Assert.True(yanit.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed,
            $"GET /Hesap/Cikis çıkış yapmamalı; dönen kod: {(int)yanit.StatusCode}");
    }

    [Fact]
    public async Task Cikis_JetonluPost_OturumuKapatir()
    {
        var istemci = uygulama.Istemci();
        await FormYardimcisi.GirisYapAsync(istemci, "yonetici@test.local");
        Assert.Equal(HttpStatusCode.OK, (await istemci.GetAsync("/Yonetim")).StatusCode);

        // Ana sayfadaki "Çıkış Yap" formunun jetonuyla çıkış.
        var cikis = await FormYardimcisi.GonderAsync(istemci, "/", "/Hesap/Cikis", new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.Redirect, cikis.StatusCode);

        var sonra = await istemci.GetAsync("/Yonetim");
        Assert.Contains("/Hesap/Giris", sonra.Headers.Location?.OriginalString);
    }
}
