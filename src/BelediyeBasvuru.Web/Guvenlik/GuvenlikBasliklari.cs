namespace BelediyeBasvuru.Web.Guvenlik;

/// <summary>
/// H3-K1: Her yanıta tarayıcı güvenlik başlıkları ekler.
/// </summary>
/// <remarks>
/// CSP bilerek script-src içermez: Views/Yonetim/Index.cshtml satır içi betik ve jsdelivr'den Chart.js kullanıyor,
/// _Layout.cshtml'de satır içi importmap var. Bunlar wwwroot'a taşınınca "script-src 'self'" eklenecek (Kişi 5 ile).
/// </remarks>
public static class GuvenlikBasliklari
{
    public static IApplicationBuilder UseGuvenlikBasliklari(this IApplicationBuilder app) =>
        app.Use(async (baglam, sonraki) =>
        {
            var basliklar = baglam.Response.Headers;
            // Tarayıcı dosya türünü tahmin etmesin (yüklenen dosya betik gibi çalışmasın).
            basliklar.XContentTypeOptions = "nosniff";
            // Site başka bir sitenin iframe'ine gömülemesin (clickjacking).
            basliklar.XFrameOptions = "DENY";
            // Başka sitelere giderken tam adres (takip no gibi) sızmasın.
            basliklar["Referrer-Policy"] = "strict-origin-when-cross-origin";
            // Kamera/mikrofon kapalı; konum yalnız kendi sitemizde (başvuru haritası için).
            basliklar["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(self)";
            // Formlar yalnız kendi sitemize gönderilsin, <base> değiştirilemesin, eklenti çalışmasın, gömülme yok.
            basliklar.ContentSecurityPolicy =
                "frame-ancestors 'none'; base-uri 'self'; form-action 'self'; object-src 'none'";
            await sonraki();
        });
}
