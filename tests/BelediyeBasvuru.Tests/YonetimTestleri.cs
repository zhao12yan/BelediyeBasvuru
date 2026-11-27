// HttpStatusCode (200 OK ...) için.
using System.Net;
// WebApplicationFactory: uygulamayı test içinde bellekte çalıştırır (gerçek sunucu açmaz).
using Microsoft.AspNetCore.Mvc.Testing;
// Test edilen servis, enum ve DTO'lar.
using BelediyeBasvuru.Core.Enums;
using BelediyeBasvuru.Core.Interfaces;
using BelediyeBasvuru.Infrastructure.Services;

namespace BelediyeBasvuru.Tests;

/// <summary>
/// Yönetim paneli sayfasının ayağa kalktığını doğrulayan duman (smoke) testi.
/// IskeletTestleri'ndeki ana sayfa testiyle aynı desen: panel SahteIstatistikServisi'nden
/// veri aldığı için veritabanına dokunmaz, bu yüzden her ortamda (CI dahil) çalışır.
/// </summary>
public class YonetimPaneliTestleri(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task YonetimPaneli_200_Doner()
    {
        // HAZIRLA
        var client = factory.CreateClient();

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
    public async Task OzetGetirAsync_AcikBasvuruSayisi_CozulduKapatildiReddedildiDisindakilerleEslesir()
    {
        // HAZIRLA
        var servis = new SahteIstatistikServisi();

        // ÇALIŞTIR
        var ozet = await servis.OzetGetirAsync();

        // DOĞRULA: docs/analiz/gostergeler.md G1 tanımı: "Cozuldu/Kapatildi/Reddedildi
        // DIŞINDAKİ" başvurular açık sayılır (Yeni + Atandi + Islemde).
        var beklenenAcik = ozet.DurumDagilimi
            .Where(d => d.Durum is nameof(BasvuruDurumu.Yeni) or nameof(BasvuruDurumu.Atandi) or nameof(BasvuruDurumu.Islemde))
            .Sum(d => d.Adet);

        Assert.Equal(beklenenAcik, ozet.ToplamAcikBasvuru);
    }

    [Fact]
    public async Task OzetGetirAsync_DurumAdlariOrtakEnumIleEslesir()
    {
        var servis = new SahteIstatistikServisi();
        var ozet = await servis.OzetGetirAsync();
        var gecerliAdlar = Enum.GetNames<BasvuruDurumu>().ToHashSet();

        Assert.NotEmpty(ozet.DurumDagilimi);
        Assert.All(ozet.DurumDagilimi, d => Assert.Contains(d.Durum, gecerliAdlar));
    }

    [Fact]
    public async Task OzetGetirAsync_BugunkuYeniBasvuru_SonGunIleEslesir()
    {
        var servis = new SahteIstatistikServisi();
        var ozet = await servis.OzetGetirAsync();

        Assert.Equal(ozet.HaftalikTrend[^1].Adet, ozet.BugunkuYeniBasvuru);
    }
}
