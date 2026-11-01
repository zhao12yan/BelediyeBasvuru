using System.Net;
using System.Text.RegularExpressions;
using BelediyeBasvuru.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BelediyeBasvuru.Tests;

public class PortalTestleri : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PortalTestleri(WebApplicationFactory<Program> factory) =>
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development"));

    private HttpClient Client() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false
    });

    [Theory]
    [InlineData("/Portal/Olustur", "Yeni Şikayet / Talep Oluştur")]
    [InlineData("/Portal/Sorgula", "Başvuru Takip ve Sorgulama")]
    [InlineData("/Portal/Giris", "Sisteme Giriş Yap")]
    [InlineData("/Portal/Personel", "Birim Personel İşlem Paneli")]
    public async Task Ekranlar_Acilir(string yol, string baslik)
    {
        using var client = Client();
        var response = await client.GetAsync(yol);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(baslik, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task Basvuru_Sorgulanir_PersoneldeGuncellenir_VeBaskaOturumaSizmaz()
    {
        using var client = Client();
        var fields = GecerliForm(await Token(client));
        var response = await client.PostAsync("/Portal/Olustur", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location!.ToString();
        var no = Regex.Match(location, @"BLD-\d{4}-\d{6}-\d").Value;
        Assert.NotEmpty(no);
        var rakamlar = no.Replace("BLD-", "").Replace("-", "");
        var toplam = rakamlar.Reverse().Select((c, i) =>
        {
            var n = (c - '0') * (i % 2 == 1 ? 2 : 1);
            return n > 9 ? n - 9 : n;
        }).Sum();
        Assert.Equal(0, toplam % 10);
        Assert.Contains("Takip No: " + no, WebUtility.HtmlDecode(await client.GetStringAsync(location)));
        Assert.Contains("Test başvurusu", WebUtility.HtmlDecode(await client.GetStringAsync("/Portal/Personel")));

        using var diger = Client();
        Assert.Contains("Bu takip numarasına ait başvuru bulunamadı.", WebUtility.HtmlDecode(await diger.GetStringAsync(location)));
        var update = await client.PostAsync("/Portal/Guncelle", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = fields["__RequestVerificationToken"],
            ["TakipNo"] = no, ["Durum"] = "3", ["Not"] = "Saha incelemesi başlatıldı."
        }));
        Assert.Equal(HttpStatusCode.Redirect, update.StatusCode);
        var detail = WebUtility.HtmlDecode(await client.GetStringAsync("/Portal/Detay?takipNo=" + no));
        Assert.Contains("Saha incelemesi başlatıldı.", detail);
        Assert.Contains("İşlemde", detail);

        var invalid = await client.PostAsync("/Portal/Guncelle", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = fields["__RequestVerificationToken"],
            ["TakipNo"] = no, ["Durum"] = "5", ["Not"] = "Geçersiz geçiş denemesi."
        }));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        using var ajax = new HttpRequestMessage(HttpMethod.Post, "/Portal/Guncelle");
        ajax.Headers.Add("X-Requested-With", "XMLHttpRequest");
        ajax.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = fields["__RequestVerificationToken"],
            ["TakipNo"] = no, ["Durum"] = "4", ["Not"] = "Saha çalışması tamamlandı."
        });
        var ajaxResponse = await client.SendAsync(ajax);
        Assert.Equal(HttpStatusCode.OK, ajaxResponse.StatusCode);
        Assert.Contains("/Portal/Personel", await ajaxResponse.Content.ReadAsStringAsync());
        Assert.Contains("Önizleme kaydının durumu ve işlem notu güncellendi.", WebUtility.HtmlDecode(await client.GetStringAsync("/Portal/Personel")));
    }

    [Fact]
    public async Task Onaysiz_VeyaListedeOlmayanKategori_Kaydedilmez()
    {
        using var client = Client();
        var fields = GecerliForm(await Token(client));
        fields["Onay"] = "false";
        fields["Kategori"] = "Uydurma müdürlük";
        var response = await client.PostAsync("/Portal/Olustur", new FormUrlEncodedContent(fields));
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Listeden bir kategori seçiniz.", html);
        Assert.Contains("Devam etmek için onay vermelisiniz.", html);
        Assert.DoesNotContain("Test başvurusu", WebUtility.HtmlDecode(await client.GetStringAsync("/Portal/Personel")));
    }

    [Fact]
    public async Task FotografUzantisi_IcerigiMaskeleyemez()
    {
        using var client = Client();
        using var body = new MultipartFormDataContent();
        foreach (var field in GecerliForm(await Token(client))) body.Add(new StringContent(field.Value), field.Key);
        using var file = new ByteArrayContent("<script>kotuicerik</script>"u8.ToArray());
        file.Headers.ContentType = new("image/jpeg");
        body.Add(file, "Fotograflar", "sahte.jpg");
        var response = await client.PostAsync("/Portal/Olustur", body);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Fotoğraflar JPG, PNG veya WebP olmalı", WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task AntiforgeryOlmadan_IslemYapilamaz()
    {
        using var client = Client();
        var response = await client.PostAsync("/Portal/Olustur", new FormUrlEncodedContent(GecerliForm("")));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Uretimde_OnizlemeIslemleriAcikDegildir()
    {
        using var factory = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Portal/Personel")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Portal/Detay?takipNo=BLD-2026-000123-7")).StatusCode);
        var response = await client.PostAsync("/Portal/Olustur", new FormUrlEncodedContent(GecerliForm(await Token(client))));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var login = await client.PostAsync("/Portal/GirisKontrol", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = await Token(client), ["Kullanici"] = "personelfen1", ["Parola"] = "onizleme"
        }));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, login.StatusCode);
    }

    private static async Task<string> Token(HttpClient client)
    {
        var html = await client.GetStringAsync("/Portal/Olustur");
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(token);
        return WebUtility.HtmlDecode(token);
    }

    private static Dictionary<string, string> GecerliForm(string token) => new()
    {
        ["__RequestVerificationToken"] = token, ["Kategori"] = PortalSecenekleri.Kategoriler[0],
        ["Baslik"] = "Test başvurusu", ["Aciklama"] = "Yol üzerinde giderilmesi gereken bir asfalt çökmesi var.",
        ["Mahalle"] = PortalSecenekleri.Mahalleler[0], ["Adres"] = "Test Sokak 12", ["Onay"] = "true"
    };
}
