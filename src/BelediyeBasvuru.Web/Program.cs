using BelediyeBasvuru.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddInfrastructure(builder.Configuration);

// K1-08 (H3): ASP.NET Core Identity burada yapılandırılacak.
// K1-09 (H4): Roller ve policy'ler (VatandasPolicy, BirimPersoneliPolicy, YoneticiPolicy, AyniBirim).
// K1-10 (H4): Global AutoValidateAntiforgeryToken + güvenlik başlıkları.
// K1-13 (H5): İstek sınırlama (basvuru / giris / sorgu).

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Entegrasyon testlerinde WebApplicationFactory<Program> kullanılabilmesi için.
public partial class Program;
