// AddInfrastructure(...) uzantı metodunu kullanabilmek için Infrastructure ad alanını içe alıyoruz.
using BelediyeBasvuru.Infrastructure;

// ===== 1. AŞAMA: Servisleri kaydet (uygulama başlamadan önce) =====

// Uygulama kurucusunu oluştur. appsettings.json, ortam değişkenleri ve komut satırı ayarlarını otomatik okur.
var builder = WebApplication.CreateBuilder(args);

// MVC'yi aç: Controller sınıfları + Razor görünümleri (Views klasörü).
builder.Services.AddControllersWithViews();
// Veritabanı ve diğer altyapı servislerini kaydet (bkz. Infrastructure/DependencyInjection.cs).
builder.Services.AddInfrastructure(builder.Configuration);

// K1-08 (H3): ASP.NET Core Identity burada yapılandırılacak.
// K1-09 (H4): Roller ve policy'ler (VatandasPolicy, BirimPersoneliPolicy, YoneticiPolicy, AyniBirim).
// K1-10 (H4): Global AutoValidateAntiforgeryToken + güvenlik başlıkları.
// K1-13 (H5): İstek sınırlama (basvuru / giris / sorgu).

// Kayıtlar bitti; uygulama nesnesini oluştur.
var app = builder.Build();

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

// Yetki kontrolü ([Authorize] olan sayfalar). Identity eklenince UseAuthentication bunun önüne gelecek.
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
