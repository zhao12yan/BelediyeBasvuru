// AddInfrastructure(...) uzantı metodunu kullanabilmek için Infrastructure ad alanını içe alıyoruz.
using BelediyeBasvuru.Infrastructure;
using BelediyeBasvuru.Infrastructure.Data;
using BelediyeBasvuru.Web.Guvenlik;

// ===== 1. AŞAMA: Servisleri kaydet (uygulama başlamadan önce) =====

// Uygulama kurucusunu oluştur. appsettings.json, ortam değişkenleri ve komut satırı ayarlarını otomatik okur.
var builder = WebApplication.CreateBuilder(args);

// MVC'yi aç: Controller sınıfları + Razor görünümleri (Views klasörü).
builder.Services.AddControllersWithViews();
// Veritabanı ve diğer altyapı servislerini kaydet (bkz. Infrastructure/DependencyInjection.cs).
builder.Services.AddInfrastructure(builder.Configuration);

// H2-K1: Identity (çerezli giriş), parola/kilitleme kuralları, roller ve policy'ler (bkz. Guvenlik/KimlikKurulumu.cs).
builder.Services.AddKimlik();

// Kayıtlar bitti; uygulama nesnesini oluştur.
var app = builder.Build();

// H1-K1: Kurulum sırası → veritabanı → migration (tablolar + roller) → örnek hesaplar.
// Docker ve yerel geliştirmede açık (appsettings.Development.json / docker-compose.yml).
// Testlerde ve CI'da kapalıdır; orada migration "dotnet ef database update" ile uygulanır.
if (app.Configuration.GetValue<bool>("Veritabani:BaslangictaHazirla"))
{
    await VeritabaniHazirlayici.HazirlaAsync(app.Services, app.Configuration, app.Logger);
}

// ===== 2. AŞAMA: İstek hattını (middleware pipeline) kur — SIRA ÖNEMLİDİR =====

// Geliştirme ortamında DEĞİLSEK (yani canlıdaysak):
if (!app.Environment.IsDevelopment())
{
    // Hata olursa kullanıcıya ayrıntı göstermek yerine /Home/Error sayfasına yönlendir.
    app.UseExceptionHandler("/Home/Error");
    // HSTS: tarayıcıya "bu siteye hep HTTPS ile gel" bilgisini gönder.
    app.UseHsts();
}

// HTTP ile gelen istekleri HTTPS'e yönlendir (HTTPS portu tanımlı değilse sadece uyarı yazar).
app.UseHttpsRedirection();
// Gelen adresin hangi controller/action'a gideceğini belirle.
app.UseRouting();

// Önce "kim bu?" (kimlik çerezini oku), sonra "buna izni var mı?" ([Authorize] kontrolü). Sıra önemli.
app.UseAuthentication();
app.UseAuthorization();

// wwwroot içindeki statik dosyaları (CSS, JS, resim, yazı tipi) sun.
app.MapStaticAssets();

// Varsayılan adres kalıbı: /{controller}/{action}/{id?}
// Örn. "/" → HomeController.Index, "/Home/Privacy" → HomeController.Privacy
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Web sunucusunu başlat ve istekleri dinlemeye başla. Bu satır uygulama kapanana kadar bekler.
app.Run();

// Entegrasyon testlerinde WebApplicationFactory<Program> kullanılabilmesi için.
// (Program sınıfını test projesinin görebileceği hâle getirir.)
public partial class Program;
