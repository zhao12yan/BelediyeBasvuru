# Kimlik ve Yetki Tasarımı

**Sahibi:** Kişi 1 (Buğra Doğan) · **Görev:** H2-K1 (#29), H3-K1 (#33)
**İlgili:** `docs/tasarim/guvenlik.md` (STRIDE), `docs/analiz/guvenlik-gereksinimleri.md` (GG-xx)

## 1. Roller

| Rol (sabit) | Kim? | Nasıl alınır? |
|---|---|---|
| `Vatandas` | Başvuru yapan kişi | Siteden kayıt olan herkes otomatik olarak |
| `BirimPersoneli` | Bir birime bağlı belediye çalışanı | Yalnız yönetici verir (H6 kullanıcı yönetimi); `BirimId` zorunlu |
| `Yonetici` | Tüm sistemi gören yetkili | Yalnız yönetici / örnek hesap ayarı ile |

- Rol adları kodda **yalnız** `BelediyeBasvuru.Core.Yetki.Roller` sabitleriyle yazılır (`"Admin"`, `"Personel"` gibi farklı yazım yok).
- Roller veritabanına **migration** ile girer (`RolYapilandirmasi.cs`, `KimlikBaslangic` migration'ı). Uygulama açılışında
  veya kayıt sırasında rol **oluşturulmaz**.
- Kayıt formunda rol alanı **yoktur**; formdan `Rol=Yonetici` gönderilse bile bağlanacak bir özellik bulunmaz (GG-06).

## 2. Hesap adresleri (Kişi 2 ekranları için sözleşme)

| İşlem | Adres | Yöntem | Form alanları |
|---|---|---|---|
| Kayıt | `/Hesap/Kayit` | GET, POST | `AdSoyad`, `Eposta`, `Parola`, `ParolaTekrar` |
| Giriş | `/Hesap/Giris` | GET, POST | `Eposta`, `Parola`, `BeniHatirla` (ops.), `DonusAdresi` (ops.) |
| Çıkış | `/Hesap/Cikis` | **yalnız POST** | — (yalnız CSRF jetonu) |
| Erişim reddi | `/Hesap/ErisimEngellendi` | GET | — (HTTP 403 döner) |

Tüm POST formları `<form asp-controller=... method="post">` ile yazılmalı; tag helper CSRF jetonunu kendisi ekler.
Kullanıcı adı = e-posta. (Kişi 2'nin taslağındaki "T.C. Kimlik No / Sicil No ile giriş" şu an desteklenmez.)

### Hata yanıtları
- Yanlış e-posta **veya** parola → aynı genel mesaj: "E-posta veya parola hatalı." (hesabın var olup olmadığı belli edilmez)
- 5 hatalı deneme → hesap **15 dk kilitlenir**; mesaj: "...hesabınız geçici olarak kilitlendi..." (GG-03)
- Kayıt hataları (zayıf parola, e-posta kullanımda) form üstünde listelenir.

## 3. Parola ve oturum kuralları

| Kural | Değer |
|---|---|
| Parola | En az 8 karakter, büyük harf + küçük harf + rakam |
| Kilitleme | 5 hatalı deneme → 15 dk (yeni hesaplarda da açık) |
| Çerez | `.BelediyeBasvuru.Kimlik`, `HttpOnly`, `SameSite=Lax`, HTTPS'te `Secure`, 2 saat (kayan) |
| Girişsiz erişim | `/Hesap/Giris?ReturnUrl=...` adresine yönlenir |
| Yanlış rol | `/Hesap/ErisimEngellendi` → **403** |
| Dönüş adresi | Yalnız site içi adres (`Url.IsLocalUrl`); dış adrese yönlendirme yapılmaz |

## 4. Policy'ler

| Policy (`Politikalar.*`) | Koşul | Kullanım |
|---|---|---|
| `Vatandas` | `Vatandas` rolü | Başvurularım vb. |
| `BirimPersoneli` | `BirimPersoneli` rolü **ve** `birim_id` claim'i | Personel işlemleri |
| `PersonelEkrani` | `Yonetici` **veya** (birimi olan `BirimPersoneli`) | Personel ekranları |
| `Yonetici` | `Yonetici` rolü | `/Yonetim` paneli |

Örnek: `[Authorize(Policy = Politikalar.PersonelEkrani)]`. Menüde bağlantıyı gizlemek yetki **sayılmaz** (GG-09).

`birim_id` claim'i girişte `Kullanici.BirimId`'den çereze eklenir (`KullaniciClaimFabrikasi`). Birimi olmayan personel
personel ekranlarına giremez.

## 5. Kurulum sırası ve örnek hesaplar

```
veritabanı erişimi → migration (tablolar + 3 rol) → örnek hesaplar (yalnız Development / Docker)
```

- Docker ve `dotnet run` (Development): açılışta otomatik (`Veritabani:BaslangictaHazirla=true`).
- CI: `VeritabaniTestleri` migration'ları gerçek SQL Server 2022'ye uygular ve rolleri doğrular; ayrıca
  `dotnet ef migrations has-pending-model-changes` ile eksik migration kontrol edilir.
- Örnek hesaplar `appsettings.Development.json` → `OrnekHesaplar` listesinde. **Parolalar depoda yoktur**
  (user-secrets / `.env`). Parolası verilmeyen hesap oluşturulmaz. Var olan hesabın parolası veya rolü
  **asla** sıfırlanmaz/değiştirilmez.

## 6. Testler

`tests/BelediyeBasvuru.Tests/KimlikTestleri.cs` (bellek içi SQLite + gerçek Identity):

- Kayıt → Vatandaş olur; formdan gelen `Rol`/`BirimId` yok sayılır
- Zayıf parola reddedilir
- Doğru parola oturum açar; yanlış parola oturum açmaz
- Girişsiz `/Yonetim` → giriş sayfası; Vatandaş/Personel `/Yonetim` → 403
- 5 hatalı denemeden sonra doğru parola da reddedilir (kilit)
- Dış adrese dönüş yapılmaz; `GET /Hesap/Cikis` çıkış yapmaz; jetonlu POST çıkış yapar

## 7. İstek güvenliği (H3-K1)

### 7.1 CSRF
- Global `AutoValidateAntiforgeryTokenAttribute`: **tüm** POST/PUT/DELETE istekleri jeton ister; `[ValidateAntiForgeryToken]`
  unutulsa bile korunur. Jetonsuz istek → **400**.
- Formları tag helper ile yazın (`<form asp-controller=... method="post">`); jeton kendiliğinden eklenir.
  JavaScript ile POST gerekiyorsa jetonu formdaki `__RequestVerificationToken` alanından okuyup gönderin.

### 7.2 İstek sınırı (429)

| Politika (`IstekSinirlari.*`) | Varsayılan | Nerede? |
|---|---|---|
| `Giris` | 10 istek / 60 sn / IP | `POST /Hesap/Giris`, `POST /Hesap/Kayit` ✅ |
| `Basvuru` | 5 istek / 60 sn / IP | Başvuru oluşturma POST'u — **Kişi 2/3 ekleyecek** |
| `Sorgu` | 30 istek / 60 sn / IP | Takip no sorgusu — **Kişi 2/3 ekleyecek** |

Sınır aşılınca **429** + `Retry-After` başlığı + Türkçe mesaj. Değerler `appsettings` → `IstekSiniri:{politika}:Limit` /
`PencereSaniye` ile değiştirilebilir. Kullanım: action'a `[EnableRateLimiting(IstekSinirlari.Basvuru)]`.
Sayfa görüntüleme (GET) sayılmaz; yalnız deneme/gönderim sayılır.

### 7.3 Güvenlik başlıkları
`X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`,
`Permissions-Policy: camera=(), microphone=(), geolocation=(self)`,
`Content-Security-Policy: frame-ancestors 'none'; base-uri 'self'; form-action 'self'; object-src 'none'`.
CSP henüz `script-src` içermez: yönetim panelindeki satır içi betik ve CDN'den gelen Chart.js `wwwroot`'a taşınınca eklenecek (Kişi 5).

## 8. Yetki matrisi (H3-K1)

Kayıt düzeyindeki kural **tek yerde**: `Core/Yetki/ErisimKurallari.cs`. Sahibi/birimi olan varlık `IYetkiliKayit` uygular.

| İşlem | Girişsiz | Vatandaş | Birim Personeli | Yönetici |
|---|---|---|---|---|
| Ana sayfa, kayıt, giriş | ✅ | ✅ | ✅ | ✅ |
| Kendi başvurusunu görme | ❌ | ✅ (yalnız `SahipKullaniciId` = kendisi) | — | ✅ |
| Başka vatandaşın başvurusu | ❌ | ❌ (403) | ✅ yalnız kendi birimindeyse | ✅ |
| Kendi birimindeki başvurular | ❌ | ❌ | ✅ (`BirimId` = `birim_id`) | ✅ |
| Başka birimin başvurusu | ❌ | ❌ | ❌ (403) | ✅ |
| Birime atanmamış başvuru | ❌ | yalnız sahibi | ❌ | ✅ |
| Personel ekranları (`PersonelEkrani`) | → giriş | ❌ (403) | ✅ (birimi varsa) | ✅ |
| Yönetim paneli `/Yonetim` | → giriş | ❌ (403) | ❌ (403) | ✅ |

**Servis/controller'da kullanım (Kişi 3, Kişi 4):**

```csharp
// Liste: yalnız görebileceği kayıtlar (SQL WHERE'e çevrilir)
var liste = await db.Basvurular.YetkiliOlanlar(User).ToListAsync();

// Tek kayıt: adresle doğrudan erişim denemesi (/Basvuru/Detay/42) burada durdurulur
var basvuru = await db.Basvurular.FindAsync(id);
if (basvuru is null) return NotFound();
if (!(await yetki.AuthorizeAsync(User, basvuru, Politikalar.KayitErisimi)).Succeeded) return Forbid();
```

`Basvuru` sınıfına eklenmesi gereken (Kişi 4): `: IYetkiliKayit`, `int? BirimId`, ve
`string? IYetkiliKayit.SahipKullaniciId => KullaniciId;`

Testler: `GuvenlikTestleri.cs` — matrisin her hücresi (`ErisimKurallariTestleri`), uygulamaya kayıtlı policy,
jetonsuz POST → 400, güvenlik başlıkları, giriş sınırında 429.

**Henüz doğrulanamayanlar:** Başvuru/personel uç noktaları `main`'de olmadığı için "başka birimin başvurusuna adresle
erişim" kuralı uygulama üzerinden değil kural/policy düzeyinde test edildi. Kişi 3/4'ün başvuru uç noktaları birleşince
aynı senaryo HTTP testi olarak eklenecek (H3-K5 / Serhat ile).

## 9. Diğer ekiplere notlar

- **Kişi 4 (Yusuf):** Identity tabloları ve roller `KimlikBaslangic` migration'ındadır. PR #19'daki `InitialCreate` /
  `BasvuruModelGuncelleme` bu migration'ın **üzerine** yeniden üretilmeli (yalnız `Basvurular` tablosu).
  PR #19'daki `AccountController` ve `Program.cs` Identity kayıtları bu dalda birleştirildi; PR'dan çıkarılmalı.
  `Birim` tablosu gelince `Kullanici.BirimId` için FK eklenmeli.
- **Kişi 2 (Süleyman):** Giriş formu `Portal/GirisKontrol` yerine `Hesap/Giris`'e, alan adları `Eposta`/`Parola` olarak gönderilmeli.
- **Kişi 3 (Ahmet):** Kayıt sahibini `User.FindFirstValue(ClaimTypes.NameIdentifier)`, personelin birimini
  `User.FindFirstValue(OzelClaimler.BirimId)` ile alın.
