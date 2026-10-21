// HttpStatusCode (200 OK, 404 NotFound ...) için.
using System.Net;
// WebApplicationFactory: uygulamayı test içinde bellekte çalıştırır.
using Microsoft.AspNetCore.Mvc.Testing;

namespace BelediyeBasvuru.Tests;

/// <summary>
/// İskeletin ayağa kalktığını doğrulayan duman (smoke) testi.
/// Ana sayfa veritabanına dokunmaz; bu yüzden her ortamda çalışır.
/// </summary>
// "(WebApplicationFactory<Program> factory)" → xUnit bu nesneyi kurucuya kendisi verir.
// IClassFixture → bu sınıftaki tüm testler aynı factory'yi paylaşır (uygulama bir kez açılır).
public class IskeletTestleri(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    // [Fact] → parametresiz tek bir test. Adı: Ne_Olunca_NeOlmalı.
    [Fact]
    public async Task AnaSayfa_200_Doner()
    {
        // HAZIRLA (Arrange): uygulamaya istek atacak bir HTTP istemcisi oluştur.
        var client = factory.CreateClient();

        // ÇALIŞTIR (Act): ana sayfaya GET isteği gönder ve cevabı bekle.
        var yanit = await client.GetAsync("/");

        // DOĞRULA (Assert): cevap kodu 200 OK olmalı; değilse test kırmızı olur.
        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
    }
}
