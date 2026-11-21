using System.Net;
using BelediyeBasvuru.Core.Yetki;
using BelediyeBasvuru.Infrastructure.Data;
using BelediyeBasvuru.Infrastructure.Kimlik;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BelediyeBasvuru.Tests;

public class HesapYonetimiTestleri
{
    [Fact]
    public async Task RolDegisikligiVePasifeAlma_EskiOturumuKapatir_DenetimiYazar()
    {
        using var uygulama = new TestUygulamasi();
        using var yonetici = uygulama.Istemci();
        using var hedef = uygulama.Istemci();
        await FormYardimcisi.GirisYapAsync(yonetici, "yonetici@test.local");
        await FormYardimcisi.GirisYapAsync(hedef, "vatandas@test.local");
        const string yol = "/KullaniciYonetimi?eposta=vatandas%40test.local";
        Assert.Equal(HttpStatusCode.Redirect, (await hedef.GetAsync(yol)).StatusCode);
        var ilk = await HesabiOku(uygulama);
        var alanlar = new Dictionary<string, string> {
            ["hedefId"] = ilk.Id, ["surum"] = ilk.Surum, ["Rol"] = Roller.Yonetici, ["PasifMi"] = "false"
        };
        var yetkisiz = await FormYardimcisi.GonderAsync(hedef, "/", "/KullaniciYonetimi/Kaydet", alanlar);
        Assert.Contains("ErisimEngellendi", yetkisiz.Headers.Location?.ToString());
        await FormYardimcisi.GonderAsync(yonetici, yol, "/KullaniciYonetimi/Kaydet", alanlar);
        var guncel = await HesabiOku(uygulama);
        Assert.Equal(Roller.Yonetici, guncel.Yetki.Rol);
        Assert.Contains("Hesap/Giris", (await hedef.GetAsync(yol)).Headers.Location?.ToString());
        await FormYardimcisi.GirisYapAsync(hedef, "vatandas@test.local");
        Assert.Equal(HttpStatusCode.OK, (await hedef.GetAsync(yol)).StatusCode);

        alanlar["PasifMi"] = "true";
        await FormYardimcisi.GonderAsync(yonetici, yol, "/KullaniciYonetimi/Kaydet", alanlar);
        Assert.False((await HesabiOku(uygulama)).Yetki.PasifMi); // Eski form sürümü
        alanlar["surum"] = guncel.Surum;
        alanlar["Rol"] = Roller.BirimPersoneli;
        alanlar["BirimId"] = "9999";
        await FormYardimcisi.GonderAsync(yonetici, yol, "/KullaniciYonetimi/Kaydet", alanlar);
        Assert.Equal(guncel, await HesabiOku(uygulama));
        alanlar["BirimId"] = "1";
        await FormYardimcisi.GonderAsync(yonetici, yol, "/KullaniciYonetimi/Kaydet", alanlar);
        Assert.Equal(new HesapYetkisi(Roller.BirimPersoneli, 1, true), (await HesabiOku(uygulama)).Yetki);
        Assert.Contains("Hesap/Giris", (await hedef.GetAsync(yol)).Headers.Location?.ToString());
        var giris = await FormYardimcisi.GirisYapAsync(hedef, "vatandas@test.local");
        Assert.Equal(HttpStatusCode.OK, giris.StatusCode);
        Assert.Contains("Hesap/Giris", (await hedef.GetAsync(yol)).Headers.Location?.ToString());

        using var kapsam = uygulama.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<AppDbContext>();
        var kayitlar = await db.Set<DenetimKaydi>().OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(2, kayitlar.Count);
        Assert.All(kayitlar, x => Assert.Equal(ilk.Id, x.HedefId));
        Assert.Contains("Vatandas", kayitlar[0].EskiDeger);
        Assert.Contains("BirimPersoneli", kayitlar[1].YeniDeger);
    }

    private static async Task<HesapGorunumu> HesabiOku(TestUygulamasi uygulama)
    {
        using var kapsam = uygulama.Services.CreateScope();
        var servis = kapsam.ServiceProvider.GetRequiredService<HesapYonetimi>();
        return Assert.IsType<HesapGorunumu>(await servis.BulAsync("vatandas@test.local"));
    }
}
