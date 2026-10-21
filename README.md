# BelediyeBaşvuru

**Batman Belediyesi Vatandaş Talep ve Şikayet Portalı** — Yazılım Mühendisliği Lab Dönem Projesi
(Batman Üniversitesi, Bilgisayar Mühendisliği · Dr. Hafzullah İŞ · Araş. Gör. Mine Onat)

Vatandaşların kategori, açıklama, fotoğraf ve harita konumuyla başvuru oluşturduğu, takip numarasıyla izlediği;
birim personelinin başvuruları işlediği ve yöneticinin istatistik paneli kullandığı web tabanlı sistem.

| Kontrol | Tarih | Kapsam |
|---|---|---|
| Kontrol 1 | 2 Kasım 2026 | Planlama + Sistem Çözümleme + ilk tasarım |
| Kontrol 2 | 30 Kasım 2026 | Tasarım + çalışan temel sistem (Artım 1) |
| Kontrol 3 | 21 Aralık 2026 | Doğrulama-geçerleme, kurulum/bakım, nihai rapor (Artım 2) |

> Teslim tarihi notu: Yönergede "11 Aralık 2025" yazıyor; plan kontrol tarihlerine göre hazırlandı. Final tarihi lab asistanıyla yazılı teyit edilecek.

## Ekip

| Rol | Kişi | Sorumluluk |
|---|---|---|
| Kişi 1 | Buğra Doğan | Takım lideri, altyapı, Identity, CI, kurulum-bakım |
| Kişi 2 | _belirlenecek_ | Ön yüz: vatandaş arayüzü, başvuru formu, harita, kurumsal sayfalar |
| Kişi 3 | _belirlenecek_ | Arka yüz: başvuru servisi, durum makinesi, atama, bildirim |
| Kişi 4 | _belirlenecek_ | Veritabanı, dosya yükleme, birim personel ekranı |
| Kişi 5 | _belirlenecek_ | Yönetim paneli, istatistik, test-güvenlik, rapor editörü |

Ekip üyeleri: Serhat Nas, Muhammed Ahmed Cihan, Yusuf Demir, Süleyman Aydoğdu, Buğra Doğan.
Roller ilk toplantıda (O-01) eşleştirilip bu tabloya yazılacak.

## Teknolojiler

C# / ASP.NET Core MVC (.NET 10) · Entity Framework Core · SQL Server (LocalDB geliştirme, SQL Server 2022 konteyneri CI) ·
ASP.NET Core Identity · SignalR · Bootstrap 5 · Leaflet + OpenStreetMap · Chart.js · xUnit + Moq · GitHub Actions

## Klasör yapısı

```
BelediyeBasvuru.sln
src/
  BelediyeBasvuru.Core/            Varlıklar, enum'lar, servis arayüzleri (altyapıdan bağımsız)
  BelediyeBasvuru.Infrastructure/  EF Core (AppDbContext, migration'lar), servis implementasyonları
  BelediyeBasvuru.Web/             MVC: controller'lar, Razor görünümleri, wwwroot
tests/
  BelediyeBasvuru.Tests/           xUnit birim ve entegrasyon testleri
docs/                              Planlama, analiz, tasarım, test belgeleri (bkz. docs/README.md)
scripts/                           Yedekleme / geri yükleme betikleri (Artım 2)
.github/                           CI iş akışı, PR ve issue şablonları
```

Katman kuralı: **Web → Infrastructure → Core**. Core hiçbir projeye referans vermez.

## Hızlı başlangıç

> **Yeni katıldıysanız önce [BASLANGIC.md](BASLANGIC.md) dosyasını okuyun** — depoyu çekme, açma, çalıştırma ve "hangi kod nerede" tablosu orada.

Docker Desktop kuruluysa tek komut yeterli (SDK veya SQL Server kurmanız gerekmez):

```bash
docker compose up --build
```

Ardından tarayıcıda **http://localhost:8080**.

## Kurulum (geliştirici makinesi, Windows)

Gerekenler: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), Visual Studio 2022/2026 Community
(“ASP.NET ve web geliştirme” iş yükü — SQL Server LocalDB bununla gelir) veya VS Code + C# Dev Kit, Git.

```bash
git clone <depo-adresi>
cd BelediyeBasvuru
dotnet tool restore          # dotnet-ef aracını kurar
dotnet build
dotnet test
dotnet run --project src/BelediyeBasvuru.Web
```

Veritabanı: Varsayılan bağlantı dizesi `appsettings.json` içinde LocalDB'yi gösterir. İlk migration (O-07) eklendikten sonra:

```bash
dotnet ef database update -p src/BelediyeBasvuru.Infrastructure -s src/BelediyeBasvuru.Web
```

Windows dışı makinede LocalDB yoktur; SQL Server 2022'yi Docker ile çalıştırıp bağlantı dizesini **user-secrets** ile verin:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=BelediyeBasvuru;User Id=sa;Password=<parola>;TrustServerCertificate=True" --project src/BelediyeBasvuru.Web
```

## Çalışma şekli

Ayrıntılar: **[CONTRIBUTING.md](CONTRIBUTING.md)**. Kısaca:

1. GitHub Projects panosundan görevini al (görev kodu: ör. `K3-05`).
2. `main`'den dal aç: `kisiN/kisa-aciklama` (ör. `kisi3/durum-gecis-tablosu`).
3. Commit → push → PR. PR açıklamasında görev kodu, ne değişti, nasıl denenir.
4. CI yeşil + 1 onay → birleştir. `main`'e doğrudan push yok.

## H1 (19–25 Ekim) ortak görevler

| Kod | Görev | Sorumlu | Durum |
|---|---|---|---|
| O-03 | GitHub deposu ve pano | Kişi 1 | ✅ depo hazır · pano ve etiketler |
| O-04 | Çözüm iskeleti (Web / Core / Infrastructure / Tests) | Kişi 1 + Kişi 3 | ✅ hazır |
| O-05 | CI (GitHub Actions + SQL Server 2022 konteyneri) | Kişi 1 + Kişi 5 | ✅ hazır |
| O-06 | Mevcut sistem incelemesi + kısa anket | Tümü | ☐ |
| O-07 | Ortak şema v1 + InitialCreate migration | Kişi 4 + Kişi 3 | ☐ `Core/Entities`, `Infrastructure/Data` |
| O-08 | Başvuru durum geçiş tablosu | Kişi 3 (onay: 4, 5) | ☐ `Core/Enums` |
| O-09 | Servis arayüzleri + sahte servisler | Kişi 3, Kişi 4 | ☐ `Core/Interfaces`, `Infrastructure/Services` |
| O-10 | Herkesten ilk PR | Tümü | ☐ |
| O-11 | İş kırılım yapısı + Gantt v1 | Kişi 1 (+ Kişi 5) | ☐ `docs/planlama` |

Her klasördeki `README.md` o klasörde hangi görevin yapılacağını ve sahibini yazar.
