# Güvenlik Gereksinimleri

**Sahibi:** Kişi 5 (Serhat Nas) · **Görev kodu:** K5-04 · **Aşama:** Sistem Çözümleme (Kontrol 1)
**İlgili belge:** `docs/tasarim/guvenlik.md` (STRIDE tehdit modeli, bu gereksinimlerin dayandığı analiz)
**Referans:** OWASP Top 10 (2021), OWASP ASVS (Application Security Verification Standard) — seviye 1

## 1. Amaç ve Kapsam

Bu belge, BelediyeBaşvuru sisteminin **fonksiyonel olmayan güvenlik gereksinimlerini** tanımlar.
Her gereksinim; sistemin hangi bileşenini ilgilendirdiği, hangi tehdide karşı olduğu (bkz.
`docs/tasarim/guvenlik.md`), kimin sorumlu olduğu ve nasıl doğrulanacağı (test) ile birlikte
listelenmiştir. Gereksinimler `docs/test/test-plani.md`'deki güvenlik testlerinin temelidir.

Sistemde 3 rol vardır: **Vatandaş** (başvuru oluşturur/takip eder), **Birim Personeli**
(kendi birimine atanan başvuruları işler), **Yönetici** (istatistik paneli, tüm başvurulara erişim).

## 2. Kimlik Doğrulama (Authentication)

| Kod | Gereksinim | Sorumlu | Doğrulama |
|---|---|---|---|
| GG-01 | Parolalar ASP.NET Core Identity'nin varsayılan `PasswordHasher`'ı (PBKDF2) ile hash'lenir; düz metin parola hiçbir yerde (log, veritabanı, e-posta) saklanmaz. | Kişi 1 (K1-08) | Kod incelemesi + veritabanı örneklemesi |
| GG-02 | Parola politikası: en az 8 karakter, en az 1 rakam, en az 1 büyük harf. (`PasswordOptions`) | Kişi 1 | Birim testi: zayıf parola reddedilir |
| GG-03 | Art arda 5 başarısız giriş denemesinden sonra hesap 15 dakika kilitlenir (`Lockout`). | Kişi 1 (K1-08) | Entegrasyon testi |
| GG-04 | Oturum çerezi (`cookie`) `HttpOnly`, `Secure`, `SameSite=Lax` olarak işaretlenir; JavaScript'ten okunamaz. | Kişi 1 | Tarayıcı geliştirici aracıyla manuel kontrol |
| GG-05 | Giriş/kayıt/parola sıfırlama uç noktaları hız sınırlamasına (rate limiting) tabidir (K1-13, `giris` politikası). | Kişi 1 + Kişi 5 | Yük testi: dakikada N istekten sonra 429 dönmeli |

## 3. Yetkilendirme (Authorization)

| Kod | Gereksinim | Sorumlu | Doğrulama |
|---|---|---|---|
| GG-06 | Her controller/action, erişebilecek rolü `[Authorize(Roles = "...")]` ile açıkça belirtir; varsayılan "herkese açık" DEĞİLDİR. | Kişi 1 (K1-09 policy'ler) | Kod incelemesi: `[Authorize]` olmayan her action gerekçelendirilir |
| GG-07 | Birim Personeli **sadece kendi biriminin** başvurularını görebilir/işleyebilir (`AyniBirim` policy). Başka birimin başvuru ID'si URL'den denenirse 403 döner. | Kişi 1 + Kişi 3 | Entegrasyon testi: farklı birim ID'siyle erişim denemesi |
| GG-08 | Yönetim paneli (`/Yonetim/*`) yalnızca `Yonetici` rolüne açıktır. | Kişi 5 (K5-02, bkz. YonetimController.cs'teki not) | Entegrasyon testi: Vatandaş/Personel rolüyle 403 beklenir |
| GG-09 | Durum geçişleri (ör. "Çözüldü" işaretleme) sadece sunucu tarafında, yetkili role göre kontrol edilir; arayüzde butonu gizlemek yetkilendirme SAYILMAZ (CONTRIBUTING.md kuralı). | Kişi 3 | Birim testi: yetkisiz rol ile durum değiştirme isteği reddedilir |

## 4. Girdi Doğrulama ve Veri Bütünlüğü

| Kod | Gereksinim | Sorumlu | Doğrulama |
|---|---|---|---|
| GG-10 | Tüm form girdileri sunucu tarafında doğrulanır (Data Annotations / FluentValidation); istemci tarafı doğrulama tek başına güvenlik önlemi sayılmaz. | Kişi 2 (form), Kişi 3 (servis) | Birim testi: geçersiz veri 400 döner |
| GG-11 | Ham SQL yasaktır (CONTRIBUTING.md); tüm veritabanı erişimi EF Core LINQ ile yapılır → SQL enjeksiyonu önlenir. Zorunlu durumda yalnız parametreli `FromSqlInterpolated`. | Kişi 4 | Kod incelemesi |
| GG-12 | Kullanıcıdan gelen hiçbir metin `Html.Raw` ile sayfaya basılmaz (CONTRIBUTING.md) → depolanan XSS (stored XSS) önlenir. Razor'ın varsayılan otomatik kaçışı (`@model.Alan`) kullanılır. | Herkes | Kod incelemesi (statik analiz: `Html.Raw` araması) |
| GG-13 | Tüm durum değiştiren (POST/PUT/DELETE) formlar CSRF belirteci taşır; global `AutoValidateAntiforgeryToken` açıktır (K1-10). | Kişi 1 | Entegrasyon testi: belirteçsiz POST 400 döner |
| GG-14 | Yüklenen fotoğraflar: izin verilen uzantı/MIME listesi (`.jpg`, `.png`, `.webp`), azami boyut (ör. 5 MB), dosya adı yeniden üretilir (kullanıcı adını DOSYA SİSTEMİ yolu olarak kullanmaz) → path traversal ve kötü amaçlı dosya yüklemesi önlenir. | Kişi 4 (`IDosyaDepolamaServisi`) | Birim testi: yasaklı uzantı reddedilir |

## 5. Oturum ve Veri Gizliliği (KVKK)

| Kod | Gereksinim | Sorumlu | Doğrulama |
|---|---|---|---|
| GG-15 | Bağlantı dizesi, parola, API anahtarı kaynak koduna/depoya YAZILMAZ; yerelde `dotnet user-secrets`, CI/Docker'da ortam değişkeni (CONTRIBUTING.md). | Herkes | Depo taraması (ör. `git log -p` ile sır deseni arama) |
| GG-16 | Vatandaşın konum/fotoğraf/iletişim bilgisi yalnızca başvuru işleme amacıyla kullanılır; KVKK aydınlatma metni başvuru formunda gösterilir. | Kişi 2 | Belge incelemesi: `/Home/Privacy` içeriği |
| GG-17 | Hata sayfaları (`/Home/Error`) üretim ortamında yığın izi (stack trace) veya iç sistem bilgisi göstermez; sadece istek numarası (`TraceIdentifier`) gösterilir. | Kişi 1 | Manuel test: `ASPNETCORE_ENVIRONMENT=Production` ile hata tetikleme |

## 6. Günlükleme ve İzlenebilirlik (Audit)

| Kod | Gereksinim | Sorumlu | Doğrulama |
|---|---|---|---|
| GG-18 | Durum değişiklikleri (kim, ne zaman, hangi duruma) kalıcı olarak günlüklenir; bu kayıtlar Yönetici tarafından görülebilir (G9 "yeniden açılma" göstergesi buna dayanır). | Kişi 3 | Birim testi: durum değişince log kaydı oluşur |
| GG-19 | Başarısız giriş denemeleri ve yetkisiz erişim denemeleri (403/401) günlüklenir; kişisel veri (parola) ASLA günlüğe yazılmaz. | Kişi 1 + Kişi 5 | Kod incelemesi |

## 7. Servis Erişilebilirliği (Denial of Service)

| Kod | Gereksinim | Sorumlu | Doğrulama |
|---|---|---|---|
| GG-20 | `basvuru`, `giris`, `sorgu` uç noktaları için ayrı hız sınırlama politikaları tanımlıdır (K1-13, `Program.cs`'teki not). | Kişi 1 | Yük testi |
| GG-21 | Dosya yükleme boyutu sunucu tarafında (Kestrel/form seçenekleri) sınırlıdır; sınırsız büyük dosya kabul edilmez. | Kişi 4 | Entegrasyon testi: limit üstü dosya 413 döner |

## 8. Kabul Kriterleri (bu belge için)

- [x] Her gereksinim bir soruna (tehdide) karşılık gelir ve `docs/tasarim/guvenlik.md`'deki STRIDE
      analiziyle izlenebilir (bkz. o belgedeki "İlgili Gereksinim" sütunu).
- [x] Her gereksinimin bir sorumlusu ve doğrulama yöntemi var.
- [ ] GG-01..GG-09 (kimlik/yetki) K1-08/K1-09 tamamlanınca fiilen test edilecek.
- [ ] Bu belge Kontrol 1'de (2 Kasım) ekip tarafından gözden geçirilecek.
