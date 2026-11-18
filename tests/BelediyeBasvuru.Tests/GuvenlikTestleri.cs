using System.Net;
using System.Security.Claims;
using BelediyeBasvuru.Core.Yetki;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BelediyeBasvuru.Tests;

/// <summary>Testlerde kullanılan sahte kayıt (gerçekte Basvuru bu arayüzü uygular).</summary>
public sealed record OrnekKayit(int No, string? SahipKullaniciId, int? BirimId) : IYetkiliKayit;

/// <summary>Testler için kimlik üretir (giriş yapmış kullanıcı gibi).</summary>
internal static class Kimlikler
{
    public static ClaimsPrincipal Olustur(string id, string rol, int? birimId = null)
    {
        var claimler = new List<Claim> { new(ClaimTypes.NameIdentifier, id), new(ClaimTypes.Role, rol) };
        if (birimId is not null)
        {
            claimler.Add(new Claim(OzelClaimler.BirimId, birimId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claimler, "Test"));
    }

    public static readonly ClaimsPrincipal Girissiz = new(new ClaimsIdentity());
    public static readonly ClaimsPrincipal Yonetici = Olustur("y1", Roller.Yonetici);
    public static readonly ClaimsPrincipal Personel1 = Olustur("p1", Roller.BirimPersoneli, 1);
    public static readonly ClaimsPrincipal BirimsizPersonel = Olustur("p0", Roller.BirimPersoneli);
    public static readonly ClaimsPrincipal VatandasA = Olustur("vA", Roller.Vatandas);
}

/// <summary>
/// H3-K1: Kayıt düzeyinde erişim kuralları (sahiplik / aynı birim / yönetici).
/// "Başka kullanıcının/birimin kaydına doğrudan istek reddediliyor" koşulunun kural tarafı.
/// </summary>
public class ErisimKurallariTestleri
{
    private static readonly OrnekKayit AKaydiBirim1 = new(1, "vA", 1);
    private static readonly OrnekKayit BKaydiBirim2 = new(2, "vB", 2);
    private static readonly OrnekKayit AtanmamisBKaydi = new(3, "vB", null);
    private static readonly OrnekKayit[] Tumu = [AKaydiBirim1, BKaydiBirim2, AtanmamisBKaydi];

    public static TheoryData<string, int, bool> Matris => new()
    {
        // kullanıcı,          kayıt no, erişebilir mi?
        { "yonetici",           1, true },  { "yonetici", 2, true },  { "yonetici", 3, true },
        { "personel1",          1, true },  { "personel1", 2, false }, { "personel1", 3, false },
        { "birimsizPersonel",   1, false }, { "birimsizPersonel", 2, false },
        { "vatandasA",          1, true },  { "vatandasA", 2, false }, { "vatandasA", 3, false },
        { "girissiz",           1, false },
    };

    private static ClaimsPrincipal Kullanici(string ad) => ad switch
    {
        "yonetici" => Kimlikler.Yonetici,
        "personel1" => Kimlikler.Personel1,
        "birimsizPersonel" => Kimlikler.BirimsizPersonel,
        "vatandasA" => Kimlikler.VatandasA,
        _ => Kimlikler.Girissiz
    };

    [Theory]
    [MemberData(nameof(Matris))]
    public void ErisebilirMi_YetkiMatrisineUyar(string kullanici, int kayitNo, bool beklenen)
    {
        var kayit = Tumu.Single(k => k.No == kayitNo);
        Assert.Equal(beklenen, ErisimKurallari.ErisebilirMi(Kullanici(kullanici), kayit));
    }

    [Theory]
    [InlineData("yonetici", new[] { 1, 2, 3 })]
    [InlineData("personel1", new[] { 1 })]
    [InlineData("birimsizPersonel", new int[0])]
    [InlineData("vatandasA", new[] { 1 })]
    [InlineData("girissiz", new int[0])]
    public void YetkiliOlanlar_ListeyiSinirlar(string kullanici, int[] beklenenNolar)
    {
        var sonuc = Tumu.AsQueryable().YetkiliOlanlar(Kullanici(kullanici)).Select(k => k.No).ToArray();
        Assert.Equal(beklenenNolar, sonuc);
    }
}

/// <summary>H3-K1: Uygulamaya kayıtlı "KayitErisimi" policy'si gerçekten bu kuralı uyguluyor mu?</summary>
public class KayitErisimPolicyTestleri(TestUygulamasi uygulama) : IClassFixture<TestUygulamasi>
{
    [Fact]
    public async Task Policy_BaskaBiriminKaydini_Reddeder_KendiBiriminiKabulEder()
    {
        using var kapsam = uygulama.Services.CreateScope();
        var yetki = kapsam.ServiceProvider.GetRequiredService<IAuthorizationService>();

        var kendiBirimi = await yetki.AuthorizeAsync(Kimlikler.Personel1, new OrnekKayit(1, "vA", 1), Politikalar.KayitErisimi);
        var baskaBirim = await yetki.AuthorizeAsync(Kimlikler.Personel1, new OrnekKayit(2, "vB", 2), Politikalar.KayitErisimi);
        var baskaVatandas = await yetki.AuthorizeAsync(Kimlikler.VatandasA, new OrnekKayit(2, "vB", 2), Politikalar.KayitErisimi);

        Assert.True(kendiBirimi.Succeeded);
        Assert.False(baskaBirim.Succeeded);
        Assert.False(baskaVatandas.Succeeded);
    }
}

/// <summary>H3-K1: CSRF koruması ve güvenlik başlıkları.</summary>
public class CsrfVeBaslikTestleri(TestUygulamasi uygulama) : IClassFixture<TestUygulamasi>
{
    [Theory]
    [InlineData("/Hesap/Giris")]
    [InlineData("/Hesap/Kayit")]
    [InlineData("/Hesap/Cikis")]
    public async Task JetonsuzPost_400Alir(string adres)
    {
        var yanit = await uygulama.Istemci().PostAsync(adres, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Eposta"] = "vatandas@test.local",
            ["Parola"] = TestUygulamasi.TestParolasi
        }));

        Assert.Equal(HttpStatusCode.BadRequest, yanit.StatusCode);
    }

    [Fact]
    public async Task KorumalarAcikken_NormalGirisAkisi_Calisir()
    {
        var yanit = await FormYardimcisi.GirisYapAsync(uygulama.Istemci(), "vatandas@test.local");
        Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
    }

    [Fact]
    public async Task Yanit_GuvenlikBasliklariniIcerir()
    {
        var yanit = await uygulama.Istemci().GetAsync("/");

        Assert.Equal("nosniff", yanit.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", yanit.Headers.GetValues("X-Frame-Options").Single());
        Assert.Contains("frame-ancestors 'none'", yanit.Headers.GetValues("Content-Security-Policy").Single());
        Assert.True(yanit.Headers.Contains("Referrer-Policy"));
    }
}

/// <summary>Giriş sınırı 3 istek/dk olan ayrı uygulama (diğer testlerin sayacını etkilemez).</summary>
public sealed class DusukSinirliUygulama : TestUygulamasi
{
    protected override IDictionary<string, string?> EkAyarlar => new Dictionary<string, string?>
    {
        ["IstekSiniri:giris:Limit"] = "3",
        ["IstekSiniri:giris:PencereSaniye"] = "60"
    };
}

/// <summary>H3-K1: Yoğun istek belirlenen sınırda 429 alıyor.</summary>
public class IstekSiniriTestleri(DusukSinirliUygulama uygulama) : IClassFixture<DusukSinirliUygulama>
{
    [Fact]
    public async Task Giris_SiniriAsinca_429VeRetryAfterDoner()
    {
        var istemci = uygulama.Istemci();
        for (var i = 0; i < 3; i++)
        {
            var izinli = await FormYardimcisi.GirisYapAsync(istemci, "vatandas@test.local", "YanlisParola9");
            Assert.Equal(HttpStatusCode.OK, izinli.StatusCode);
        }

        var reddedilen = await FormYardimcisi.GirisYapAsync(istemci, "vatandas@test.local", "YanlisParola9");

        Assert.Equal(HttpStatusCode.TooManyRequests, reddedilen.StatusCode);
        Assert.True(reddedilen.Headers.Contains("Retry-After"));
        // Sayfayı görüntülemek (GET) sınırlanmaz; yalnız deneme (POST) sayılır.
        Assert.Equal(HttpStatusCode.OK, (await istemci.GetAsync("/Hesap/Giris")).StatusCode);
    }
}
