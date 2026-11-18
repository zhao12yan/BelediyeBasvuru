using System.Net;
using System.Text.RegularExpressions;
using BelediyeBasvuru.Core.Entities;
using BelediyeBasvuru.Core.Yetki;
using BelediyeBasvuru.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BelediyeBasvuru.Tests;

/// <summary>
/// H2-K1: Uygulamayı bellekte, gerçek Identity ile ama SQL Server YERİNE bellek içi SQLite ile çalıştırır.
/// Böylece giriş/rol/yetki testleri her bilgisayarda ve CI'da veritabanı kurmadan çalışır.
/// (Gerçek SQL Server uyumluluğu VeritabaniTestleri ve CI'daki migration adımıyla ayrıca doğrulanır.)
/// </summary>
public class TestUygulamasi : WebApplicationFactory<Program>
{
    /// <summary>Test hesaplarının ortak parolası (yalnız bu bellek içi test veritabanında vardır).</summary>
    public const string TestParolasi = "TestParola1";

    private readonly SqliteConnection _baglanti = new("DataSource=:memory:");

    /// <summary>Alt sınıflar test ayarlarını (ör. düşük istek sınırı) buradan ekler.</summary>
    protected virtual IDictionary<string, string?> EkAyarlar => new Dictionary<string, string?>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Açılışta migration/örnek hesap çalışmasın (SQL Server yok); test verisini aşağıda biz kuruyoruz.
        builder.UseSetting("Veritabani:BaslangictaHazirla", "false");
        // Testlerde bütün istekler aynı "IP"den gelir; sınır testi dışındaki testler 429'a takılmasın.
        foreach (var politika in new[] { "giris", "basvuru", "sorgu" })
        {
            builder.UseSetting($"IstekSiniri:{politika}:Limit", "10000");
        }

        foreach (var (anahtar, deger) in EkAyarlar)
        {
            builder.UseSetting(anahtar, deger);
        }

        builder.ConfigureServices(services =>
        {
            // SQL Server kaydını kaldır, aynı AppDbContext'i SQLite ile kaydet.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_baglanti));
        });
    }

    /// <summary>Bellek içi veritabanını açar, tabloları + 3 rolü + test hesaplarını oluşturur.</summary>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        _baglanti.Open();
        var host = base.CreateHost(builder);

        using var kapsam = host.Services.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated(); // Rolleri de (HasData) oluşturur.

        var userManager = kapsam.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();
        HesapOlustur(userManager, "yonetici@test.local", Roller.Yonetici, null);
        HesapOlustur(userManager, "personel1@test.local", Roller.BirimPersoneli, 1);
        HesapOlustur(userManager, "birimsiz-personel@test.local", Roller.BirimPersoneli, null);
        HesapOlustur(userManager, "vatandas@test.local", Roller.Vatandas, null);
        return host;
    }

    private static void HesapOlustur(UserManager<Kullanici> userManager, string eposta, string rol, int? birimId)
    {
        var kullanici = new Kullanici { UserName = eposta, Email = eposta, AdSoyad = eposta, BirimId = birimId };
        var sonuc = userManager.CreateAsync(kullanici, TestParolasi).GetAwaiter().GetResult();
        if (sonuc.Succeeded)
        {
            sonuc = userManager.AddToRoleAsync(kullanici, rol).GetAwaiter().GetResult();
        }

        if (!sonuc.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", sonuc.Errors.Select(h => h.Description)));
        }
    }

    /// <summary>Yönlendirmeleri OTOMATİK izlemeyen, çerez tutan istemci (302/403 kontrolü için).</summary>
    public HttpClient Istemci() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _baglanti.Dispose();
        }
    }
}

/// <summary>Formlu istekler için yardımcılar (CSRF jetonunu sayfadan okur).</summary>
public static partial class FormYardimcisi
{
    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex JetonDeseni();

    /// <summary>Sayfayı GET eder ve içindeki anti-forgery jetonunu döndürür (çerez istemcide kalır).</summary>
    public static async Task<string> JetonAlAsync(HttpClient istemci, string adres)
    {
        var html = await istemci.GetStringAsync(adres);
        var eslesme = JetonDeseni().Match(html);
        Assert.True(eslesme.Success, $"{adres} sayfasında CSRF jetonu bulunamadı.");
        return WebUtility.HtmlDecode(eslesme.Groups[1].Value);
    }

    /// <summary>Jetonu ekleyerek form POST eder.</summary>
    public static async Task<HttpResponseMessage> GonderAsync(HttpClient istemci, string sayfa, string hedef,
        IDictionary<string, string> alanlar)
    {
        var jeton = await JetonAlAsync(istemci, sayfa);
        var tumAlanlar = new Dictionary<string, string>(alanlar) { ["__RequestVerificationToken"] = jeton };
        return await istemci.PostAsync(hedef, new FormUrlEncodedContent(tumAlanlar));
    }

    /// <summary>Verilen hesapla /Hesap/Giris üzerinden gerçek giriş yapar.</summary>
    public static async Task<HttpResponseMessage> GirisYapAsync(HttpClient istemci, string eposta,
        string parola = TestUygulamasi.TestParolasi)
    {
        return await GonderAsync(istemci, "/Hesap/Giris", "/Hesap/Giris",
            new Dictionary<string, string> { ["Eposta"] = eposta, ["Parola"] = parola });
    }
}
