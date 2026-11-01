# Güvenlik Tasarımı — STRIDE Tehdit Modeli

**Sahibi:** Kişi 5 (Serhat Nas) · **Görev kodu:** K5-05 · **Aşama:** Tasarım (Kontrol 1)
**Girdi:** `docs/analiz/guvenlik-gereksinimleri.md` (GG-xx kodları bu belgede referans verilir)
**Yöntem:** STRIDE (Microsoft) — her sistem bileşeni için 6 tehdit kategorisi sistematik olarak sorgulanır:
**S**poofing (kimlik taklidi), **T**ampering (veri bozma), **R**epudiation (inkâr),
**I**nformation Disclosure (bilgi sızıntısı), **D**enial of Service (hizmet engelleme),
**E**levation of Privilege (yetki yükseltme).

## 1. Sistem Sınırları ve Veri Akış Diyagramı (DFD) — Metinsel Özet

```
[Vatandaş Tarayıcısı] --(HTTPS)--> [Web Sunucusu: BelediyeBasvuru.Web]
[Birim Personeli Tarayıcısı] --(HTTPS)--> [Web Sunucusu]
[Yönetici Tarayıcısı] --(HTTPS)--> [Web Sunucusu]
                                        |
                                        |--> [Kimlik Doğrulama: ASP.NET Core Identity] --> [AspNetUsers tablosu]
                                        |--> [Başvuru Servisi] --> [SQL Server: Basvuru, Durum tabloları]
                                        |--> [Dosya Depolama Servisi] --> [Dosya sistemi / Blob alanı]
                                        |--> [Bildirim Servisi] --> [SignalR (anlık) / e-posta (Artım 2)]
                                        |--> [İstatistik Servisi] --> [SQL Server (salt okunur sorgular)]
```

**Güven sınırları (trust boundary) — tehditlerin en yoğun olduğu noktalar:**

1. **TB-1**: İnternet ↔ Web Sunucusu (herhangi bir kullanıcı, kimliksiz de olsa, buraya istek atabilir).
2. **TB-2**: Kimliği doğrulanmış kullanıcı ↔ Rol bazlı yetki sınırı (Vatandaş → Personel → Yönetici).
3. **TB-3**: Web Sunucusu ↔ Veritabanı / Dosya depolama (uygulama kimliğiyle erişim).

## 2. Bileşen × STRIDE Analizi

### 2.1 Başvuru Formu ve Dosya Yükleme (TB-1, Vatandaş — kimliksiz erişilebilir)

| Tehdit (STRIDE) | Senaryo | Etki | Olasılık | Risk | Önlem | İlgili Gereksinim |
|---|---|---|---|---|---|---|
| **S**poofing | Botun gerçek vatandaş gibi toplu sahte başvuru göndermesi | Orta | Yüksek | **Yüksek** | CAPTCHA/hız sınırlama (`basvuru` politikası) | GG-20 |
| **T**ampering | Form alanına (kategori, açıklama) zararlı/aşırı uzun veri gönderme | Düşük | Yüksek | Orta | Sunucu tarafı doğrulama, alan uzunluk sınırı | GG-10 |
| **R**epudiation | Vatandaşın "ben böyle bir başvuru yapmadım" demesi | Düşük | Orta | Düşük | Takip numarası + oluşturma zaman damgası + (varsa) IP/e-posta doğrulama | GG-18 |
| **I**nformation Disclosure | Başka birinin takip numarasını tahmin ederek başvuru detayını görme | Yüksek | Orta | **Yüksek** | Takip numarası yeterince uzun/rastgele (tahmin edilemez) üretilir; detay sayfası ek doğrulama (TC/telefon son 4 hane) isteyebilir | GG-10, GG-16 |
| **D**enial of Service | Çok sayıda/çok büyük fotoğraf yükleyerek diski/sunucuyu doldurma | Yüksek | Orta | **Yüksek** | Dosya boyutu/sayısı sınırı (GG-21), uzantı/MIME kontrolü (GG-14) | GG-14, GG-21 |
| **E**levation of Privilege | Yüklenen dosyanın adını `../../` içerecek şekilde ayarlayıp sunucu dosyalarının üzerine yazma (path traversal) | Yüksek | Düşük | Orta | Dosya adı sunucuda yeniden üretilir (GUID), kullanıcı girdisi dosya yolunda ASLA kullanılmaz | GG-14 |

### 2.2 Kimlik Doğrulama / Oturum (TB-1 ↔ TB-2 sınırı)

| Tehdit | Senaryo | Etki | Olasılık | Risk | Önlem | İlgili Gereksinim |
|---|---|---|---|---|---|---|
| **S**poofing | Kaba kuvvet (brute-force) ile başkasının parolasını deneme | Yüksek | Yüksek | **Yüksek** | Hesap kilitleme (GG-03), giriş hız sınırı (GG-05) | GG-03, GG-05 |
| **T**ampering | Oturum çerezinin istemci tarafında değiştirilmesi (ör. kullanıcı ID'si) | Yüksek | Düşük | Orta | İmzalı/şifreli Identity çerezi (ASP.NET Core varsayılanı), `HttpOnly` | GG-04 |
| **R**epudiation | Kullanıcının "bu işlemi ben yapmadım" demesi | Orta | Orta | Orta | Her girişte zaman damgalı audit log (GG-19) | GG-19 |
| **I**nformation Disclosure | Parola hash'inin veya oturum çerezinin ağ üzerinde düz metin taşınması | Yüksek | Düşük (HTTPS varsa) | Orta | Zorunlu HTTPS + HSTS (`Program.cs`: `UseHttpsRedirection`, `UseHsts`) | GG-01, GG-04 |
| **D**enial of Service | Sahte/çok sayıda kayıt isteğiyle Identity tablolarının şişirilmesi | Orta | Orta | Orta | Kayıt uç noktasında hız sınırı, e-posta doğrulama adımı | GG-05 |
| **E**levation of Privilege | Kayıt formunda rol alanının manipüle edilip "Yönetici" olarak kayıt | Çok Yüksek | Düşük | **Yüksek** | Rol ASLA formdan okunmaz; yeni kayıtlar sabit "Vatandaş" rolüyle oluşturulur, rol ataması sadece Yönetici panelinden/veritabanından yapılır | GG-06 |

### 2.3 Yönetici / İstatistik Paneli (TB-2, en yüksek yetki seviyesi)

| Tehdit | Senaryo | Etki | Olasılık | Risk | Önlem | İlgili Gereksinim |
|---|---|---|---|---|---|---|
| **S**poofing | Personel hesabının çalınan oturumla panele girmeye çalışması | Yüksek | Düşük | Orta | `[Authorize(Roles="Yonetici")]` sunucu tarafı kontrolü | GG-08 |
| **T**ampering | — (panel salt okunur; veri değiştirme işlevi yok) | — | — | — | Panel yalnızca `OzetGetirAsync()` okur, yazmaz | — |
| **R**epudiation | Yöneticinin panele hangi tarihte baktığının izlenememesi | Düşük | Orta | Düşük | (Artım 2) Panel erişim logu eklenebilir | GG-19 |
| **I**nformation Disclosure | URL doğrudan yazılarak (`/Yonetim`) yetkisiz rolün toplu istatistiğe erişmesi | Orta | Orta | Orta | `[Authorize(Roles="Yonetici")]` + ayrıca "arayüzde linki gizlemek yetki SAYILMAZ" kuralına uyulur | GG-08, GG-09 |
| **D**enial of Service | Pahalı istatistik sorgularının (ör. tüm tablo taraması) sık çağrılması | Orta | Düşük | Düşük | Sorgu sonuçları kısa süreli önbelleğe alınabilir (Artım 2) | — |
| **E**levation of Privilege | Vatandaş/Personel rolünün panel verisine erişmesi | Yüksek | Düşük (kontrol varsa) | Orta | GG-08 testiyle güvence altına alınır | GG-08 |

### 2.4 Veritabanı ve Dosya Depolama Erişimi (TB-3)

| Tehdit | Senaryo | Etki | Olasılık | Risk | Önlem | İlgili Gereksinim |
|---|---|---|---|---|---|---|
| **S**poofing | — (uygulama tek bir hizmet hesabıyla bağlanır, taklit riski düşük) | — | — | — | Bağlantı dizesi sır olarak saklanır | GG-15 |
| **T**ampering | SQL enjeksiyonu ile veri değiştirme | Çok Yüksek | Düşük | **Yüksek** (etki büyük) | EF Core LINQ, ham SQL yasağı (GG-11) | GG-11 |
| **R**epudiation | Veritabanında kim neyi değiştirdiğinin izlenememesi | Orta | Orta | Orta | Değişiklik zaman damgaları (`OlusturmaTarihi`, `GuncellemeTarihi`) her tabloda | GG-18 |
| **I**nformation Disclosure | Bağlantı dizesinin/parolanın kaynak koduna/depoya sızması | Çok Yüksek | Orta (insan hatası) | **Yüksek** | `user-secrets` / ortam değişkeni, sızarsa anında döndürme (GG-15) | GG-15 |
| **D**enial of Service | Büyük dosya yüklemelerinin disk alanını tüketmesi | Orta | Orta | Orta | Boyut/sayı sınırı (GG-21) | GG-21 |
| **E**levation of Privilege | Uygulama veritabanı hesabının gereğinden fazla yetkiye (ör. `sysadmin`) sahip olması | Yüksek | Düşük | Orta | Uygulama hesabına yalnız gerekli tablolarda CRUD yetkisi verilir (en az yetki ilkesi) | GG-15 |

## 3. Risk Özeti (Yüksek Riskli Maddeler — Önceliklendirme)

| Risk | Bileşen | Azaltma Durumu |
|---|---|---|
| Kaba kuvvetle hesap ele geçirme | Kimlik doğrulama | Planlı: K1-08 (Identity) + K1-13 (hız sınırı) |
| Kayıt formunda rol manipülasyonu ile yetki yükseltme | Kimlik doğrulama | Planlı: K1-08/K1-09 — rol formdan okunmayacak |
| Takip numarası tahmini ile başkasının başvurusunu görme | Başvuru takip | Planlı: Kişi 2/3 — takip no üretim algoritması gözden geçirilecek |
| Dosya yükleme ile disk doldurma (DoS) | Dosya yükleme | Planlı: K4 (`IDosyaDepolamaServisi`) — boyut/uzantı sınırı |
| SQL enjeksiyonu | Veritabanı erişimi | Zaten azaltılmış: EF Core + CONTRIBUTING.md kuralı (ham SQL yasak) |
| Sır sızıntısı (bağlantı dizesi/parola) | Tüm sistem | Zaten azaltılmış: user-secrets/ortam değişkeni kuralı; ekip disiplinine bağlı |

## 4. Varsayımlar ve Sınırlamalar

- Bu analiz, şema (O-07) ve kimlik doğrulama (K1-08/K1-09) **henüz tamamlanmadan** yapılmıştır;
  bu yüzündenki önlemler "planlı" olarak işaretlenmiştir. Kontrol 2'de (30 Kasım) gerçek
  implementasyona göre bu belge güncellenecek ve "Planlı" sütunları "Doğrulandı"ya dönecektir.
- E-posta/SMS doğrulama, 2FA gibi Artım 2 kapsamındaki önlemler bu ilk sürümde detaylandırılmamıştır.
- Altyapı güvenliği (sunucu işletim sistemi, ağ güvenlik duvarı) kapsam dışıdır — bu belge sadece
  uygulama (application-level) tehditlerini kapsar.

## 5. Kabul Kriterleri

- [x] Her bileşen için 6 STRIDE kategorisi de değerlendirildi (uygulanamayanlar "—" ile işaretlendi, atlanmadı).
- [x] Her risk, `docs/analiz/guvenlik-gereksinimleri.md`'deki bir GG-xx koduna bağlandı.
- [x] Yüksek riskli maddeler önceliklendirilip özet tabloda listelendi.
- [ ] Bu belge Kontrol 1'de ekip ve asistanla gözden geçirilecek, eksik bileşen varsa eklenecek.
