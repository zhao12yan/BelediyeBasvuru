using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace BelediyeBasvuru.Web.Guvenlik;

/// <summary>
/// H3-K1: İstek sınırı (rate limiting) politikaları. Sınır aşılınca HTTP 429 döner (GG-05, GG-20).
/// Her politika IP adresi başına, sabit zaman penceresiyle sayar; değerler appsettings "IstekSiniri" bölümünden değiştirilebilir.
/// Kullanım: action'a <c>[EnableRateLimiting(IstekSinirlari.Basvuru)]</c>
/// </summary>
public static class IstekSinirlari
{
    /// <summary>POST /Hesap/Giris ve /Hesap/Kayit (kaba kuvvet ve toplu kayıt).</summary>
    public const string Giris = "giris";
    /// <summary>Başvuru oluşturma POST'u (bot ile toplu sahte başvuru).</summary>
    public const string Basvuru = "basvuru";
    /// <summary>Takip numarasıyla sorgu (numara tahmin denemesi).</summary>
    public const string Sorgu = "sorgu";

    // Varsayılanlar: (izin verilen istek, pencere saniye)
    private static readonly Dictionary<string, (int Limit, int Saniye)> Varsayilanlar = new()
    {
        [Giris] = (10, 60),
        [Basvuru] = (5, 60),
        [Sorgu] = (30, 60),
    };

    public static IServiceCollection AddIstekSinirlari(this IServiceCollection services, IConfiguration ayarlar)
    {
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.OnRejected = async (baglam, iptal) =>
            {
                if (baglam.Lease.TryGetMetadata(MetadataName.RetryAfter, out var bekle))
                {
                    baglam.HttpContext.Response.Headers.RetryAfter =
                        ((int)bekle.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                baglam.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
                await baglam.HttpContext.Response.WriteAsync(
                    "Çok fazla istek gönderdiniz. Lütfen biraz bekleyip tekrar deneyin.", iptal);
            };

            foreach (var (ad, varsayilan) in Varsayilanlar)
            {
                var limit = ayarlar.GetValue($"IstekSiniri:{ad}:Limit", varsayilan.Limit);
                var pencere = TimeSpan.FromSeconds(ayarlar.GetValue($"IstekSiniri:{ad}:PencereSaniye", varsayilan.Saniye));

                o.AddPolicy(ad, http => RateLimitPartition.GetFixedWindowLimiter(
                    // Docker/yerel kurulumda ters vekil (proxy) yok; doğrudan istemci IP'si kullanılır.
                    http.Connection.RemoteIpAddress?.ToString() ?? "bilinmiyor",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limit,
                        Window = pencere,
                        QueueLimit = 0 // Beklet değil, hemen reddet.
                    }));
            }
        });

        return services;
    }
}
