// HttpStatusCode (200 OK ...) için.
using System.Net;
// WebApplicationFactory: uygulamayı test içinde bellekte çalıştırır (gerçek sunucu açmaz).
using Microsoft.AspNetCore.Mvc.Testing;
// Test edilen servis ve DTO'lar.
using BelediyeBasvuru.Core.Interfaces;
using BelediyeBasvuru.Infrastructure.Services;

namespace BelediyeBasvuru.Tests;

/// <summary>
/// Yönetim paneli sayfasının ayağa kalktığını doğrulayan duman (smoke) testi.
/// H2-K1: Panel artık yalnız Yönetici rolüne açık; test önce yönetici olarak giriş yapar.
/// Yetkisiz erişim senaryoları KimlikTestleri'ndedir.
/// </summary>
public class YonetimPaneliTestleri(TestUygulamasi factory)
    : IClassFixture<TestUygulamasi>
{
    [Fact]
    public async Task YonetimPaneli_Yoneticiye_200_Doner()
    {
        // HAZIRLA
        var client = factory.Istemci();
        await FormYardimcisi.GirisYapAsync(client, "yonetici@test.local");

        // ÇALIŞTIR
        var yanit = await client.GetAsync("/Yonetim");

        // DOĞRULA
        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
    }
}

/// <summary>
/// SahteIstatistikServisi'nin döndürdüğü örnek verinin kendi içinde tutarlı olduğunu
/// doğrulayan birim testleri (web sunucusu açmadan, doğrudan sınıfı çağırır).
/// </summary>
public class SahteIstatistikServisiTestleri
{
    [Fact]
    public async Task OzetGetirAsync_YediGunlukTrendDoner()
    {
        // HAZIRLA
        var servis = new SahteIstatistikServisi();

        // ÇALIŞTIR
        var ozet = await servis.OzetGetirAsync();

        // DOĞRULA: G5 tanımı "son 7 gün" olduğu için tam 7 kayıt beklenir.
        Assert.Equal(7, ozet.HaftalikTrend.Count);
    }

    [Fact]
    public async Task OzetGetirAsync_AcikBasvuruSayisi_YeniInceleniyorAtandiToplamiyleEslesir()
    {
        // HAZIRLA
        var servis = new SahteIstatistikServisi();

        // ÇALIŞTIR
        var ozet = await servis.OzetGetirAsync();

        // DOĞRULA: docs/analiz/gostergeler.md G1 tanımı: "Çözüldü/Kapatıldı/Reddedildi
        // DIŞINDAKİ" başvurular açık sayılır; burada bu küme Yeni+İnceleniyor+Atandı'dır.
        var beklenenAcik = ozet.DurumDagilimi
            .Where(d => d.Durum is "Yeni" or "İnceleniyor" or "Atandı")
            .Sum(d => d.Adet);

        Assert.Equal(beklenenAcik, ozet.ToplamAcikBasvuru);
    }
}
