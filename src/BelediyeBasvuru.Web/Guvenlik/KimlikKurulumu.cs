using BelediyeBasvuru.Core.Entities;
using BelediyeBasvuru.Core.Yetki;
using BelediyeBasvuru.Infrastructure.Data;
using BelediyeBasvuru.Infrastructure.Kimlik;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BelediyeBasvuru.Web.Guvenlik;

/// <summary>
/// H2-K1: ASP.NET Core Identity (çerezli giriş), parola/kilitleme kuralları ve rol policy'leri.
/// Program.cs'te tek satırla çağrılır: builder.Services.AddKimlik();
/// Ayrıntılı kurallar: docs/tasarim/kimlik-yetki.md
/// </summary>
public static class KimlikKurulumu
{
    public static IServiceCollection AddKimlik(this IServiceCollection services)
    {
        services.AddIdentity<Kullanici, IdentityRole>(o =>
            {
                // Parola: en az 8 karakter, büyük + küçük harf + rakam (GG-02).
                o.Password.RequiredLength = 8;
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireUppercase = true;
                o.Password.RequireNonAlphanumeric = false;

                // Kaba kuvvete karşı: 5 hatalı denemede 15 dk kilit (GG-03). Yeni hesaplarda da geçerli.
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                o.Lockout.AllowedForNewUsers = true;

                // Kullanıcı adı = e-posta; aynı e-postayla ikinci hesap açılamaz.
                o.User.RequireUniqueEmail = true;
                o.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddClaimsPrincipalFactory<KullaniciClaimFabrikasi>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserConfirmation<Kullanici>, HesapEtkinlikKontrolu>();
        services.AddScoped<HesapYonetimi>();
        services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.Zero);

        services.ConfigureApplicationCookie(o =>
        {
            o.Cookie.Name = ".BelediyeBasvuru.Kimlik";
            o.Cookie.HttpOnly = true;                              // JavaScript çerezi okuyamaz (GG-04).
            o.Cookie.SameSite = SameSiteMode.Lax;                  // Başka siteden gelen POST'ta çerez gitmez.
            o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS'te Secure; Docker'daki http://localhost:8080'de de çalışır.
            o.ExpireTimeSpan = TimeSpan.FromHours(2);
            o.SlidingExpiration = true;
            o.LoginPath = "/Hesap/Giris";                          // Girişsiz kullanıcı buraya yönlenir.
            o.LogoutPath = "/Hesap/Cikis";
            o.AccessDeniedPath = "/Hesap/ErisimEngellendi";        // Yanlış rol → 403 sayfası (giriş sayfası DEĞİL).
        });

        services.AddAuthorizationBuilder()
            .AddPolicy(Politikalar.Vatandas, p => p.RequireRole(Roller.Vatandas))
            // Personel, bir birime bağlı DEĞİLSE personel ekranlarına giremez.
            .AddPolicy(Politikalar.BirimPersoneli, p => p.RequireRole(Roller.BirimPersoneli).RequireClaim(OzelClaimler.BirimId))
            .AddPolicy(Politikalar.Yonetici, p => p.RequireRole(Roller.Yonetici))
            .AddPolicy(Politikalar.PersonelEkrani, p => p.RequireAssertion(c =>
                c.User.IsInRole(Roller.Yonetici) ||
                (c.User.IsInRole(Roller.BirimPersoneli) && c.User.HasClaim(h => h.Type == OzelClaimler.BirimId))))
            // H3-K1: Tek kayda erişim (sahiplik / aynı birim / yönetici) — bkz. KayitErisimIsleyicisi.
            .AddPolicy(Politikalar.KayitErisimi, p => p.RequireAuthenticatedUser().AddRequirements(new KayitErisimGereksinimi()));
        services.AddSingleton<IAuthorizationHandler, KayitErisimIsleyicisi>();

        return services;
    }
}
