# Kişi 2 — Vatandaş Portalı Ön Yüzü

**Sorumlu: Süleyman Aydoğdu.** Tasarım ve arayüz kodlaması Kişi 2 katkısıdır.
Tasarım referansı: [#20 — taslak_sikayetsitesi](https://github.com/zhao12yan/BelediyeBasvuru/pull/20).

PR'daki lacivert menü, turkuaz vurgu, dört adımlı başvuru kartı, giriş pencereleri,
sorgulama kartı ve sarı vurgulu personel ekranı Razor / Bootstrap / CSS / JavaScript ile uygulanmıştır.
Ana sayfanın başvuru ve sorgulama bağlantıları bu ekranlara gider.
Ekran görüntülerindeki arka planın ayrı kaynak dosyası paylaşılmadığı için projedeki yerel Batman panoraması kullanılır.

## Ekranlar

| Adres | İçerik |
|---|---|
| `/Portal/Olustur` | Kategori ve detay → konum → fotoğraf → özet ve onay |
| `/Portal/Sorgula` | Takip numarasıyla sorgulama |
| `/Portal/Giris` | Vatandaş / belediye personeli giriş penceresi |
| `/Portal/Personel` | Birim başvuruları ve işlem paneli; yalnız geliştirme ortamı |

Başvuru formunda zorunlu alanlar, karakter sınırları, dosya türü ve boyutu kontrol edilir.
En fazla üç fotoğraf seçilebilir; küçük önizlemeler kaldırılabilir. Başvuru özeti formdaki değerleri gösterir.
Personel penceresi mevcut sunucu tarafı durum geçiş kurallarını kullanır; işlem notu zorunludur.

## Çalıştırma ve doğrulama

```powershell
dotnet run --project src/BelediyeBasvuru.Web --launch-profile http
dotnet test BelediyeBasvuru.sln -c Release
node --check src/BelediyeBasvuru.Web/wwwroot/js/portal.js
```

`http://localhost:5248/Portal/Olustur` adresinden formu deneyin.
Örnek sorgu: `BLD-2026-000123-7`. Personel önizlemesine alt bilgideki “Önizleme bilgisi” bölümünden ulaşılır.
Başka bir tarayıcı oturumu kendi örnek kayıtlarıyla başlar; oluşturulan başvuruları paylaşmaz.

## Arka uç bağlantılarının durumu

- Identity henüz main dalında kurulmadı. Giriş penceresi hazırdır; parola doğrulama veya gerçek oturum açma yapmaz.
- Geliştirme ortamında kayıtlar sunucu belleğinde, tarayıcı oturumuna özel ve 30 dakika zaman aşımıyla tutulur.
  Bunlar belediyeye gönderilmiş gerçek başvurular değildir; sunucu yeniden başlatılınca silinir.
- Fotoğraflar formda önizlenir ve sunucuda tür / boyut bakımından denetlenir; kalıcı dosya deposuna yazılmaz.
- Mahalle ve adres seçimi hazırdır. Harita bileşeni ve koordinat seçimi henüz bağlı değildir; rastgele koordinat gösterilmez.
- Canlı bildirim servisi bağlı değildir; personel listesi yenileme düğmesiyle güncellenir.
- Üretimde örnek personel işlemleri kapalıdır. Kalıcı kayıt ve giriş hizmeti bağlanana kadar gönderim / giriş istekleri
  başarı bildirmek yerine açık bir hizmet kullanılamıyor mesajıyla `503` döner.

O-07/K1-08 ve başvuru servisi birleştirildiğinde `PortalController` içindeki oturum önizlemesi bu servislere bağlanmalıdır.
Sunucu tarafı dosya, kategori, durum geçişi ve istek doğrulama kontrolleri korunmalıdır.
