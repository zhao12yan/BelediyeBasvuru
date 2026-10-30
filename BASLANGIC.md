# Başlangıç Kılavuzu — Projeyi bilgisayarına çek, aç, çalıştır

Bu kılavuz ekibe yeni katılan herkes içindir. Sırayla ilerleyin; her adımın sonunda ne görmeniz gerektiği yazıyor.

---

## 1. Gerekenler

Projeyi çalıştırmak için **iki yoldan birini** seçin:

| | **Yol A — Docker (önerilen)** | **Yol B — .NET SDK ile** |
|---|---|---|
| Kurulacaklar | Git + [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Git + [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Veritabanı | Kendiliğinden gelir (SQL Server 2022 konteyneri) | Windows'ta LocalDB (Visual Studio ile gelir) |
| Adres | http://localhost:8080 | http://localhost:5248 |
| Ne zaman? | Sadece çalıştırıp bakmak, ilk deneme | Kod yazarken hızlı deneme, hata ayıklama (debug) |

Kod düzenlemek için bir editör de gerekir: **VS Code** (+ "C# Dev Kit" eklentisi) veya **Visual Studio 2026**.

> Visual Studio 2022 (17.x) .NET 10 projelerini açamayabilir. VS Code veya Visual Studio 2026 kullanın.

---

## 2. Depoyu bilgisayara çekme (bir kez)

1. GitHub'dan gelen **davet e-postasını kabul edin** (depo özel/private).
2. Bir terminal açın (Windows: PowerShell) ve projeyi koymak istediğiniz klasöre gidin, sonra:

```bash
git clone https://github.com/zhao12yan/BelediyeBasvuru.git
cd BelediyeBasvuru
```

İlk seferde GitHub giriş penceresi açılırsa kendi hesabınızla giriş yapın.

✅ **Görmeniz gereken:** `BelediyeBasvuru` klasörü içinde `BelediyeBasvuru.sln`, `src`, `tests`, `docker-compose.yml`.

---

## 3A. Docker ile çalıştırma (tek komut)

1. **Docker Desktop**'ı açın; sol altta "Engine running" yazana kadar bekleyin.
2. Proje klasöründe:

```bash
docker compose up --build
```

İlk seferde imajlar indirildiği için **3–5 dakika** sürebilir; sonraki açılışlar birkaç saniyedir.

✅ **Görmeniz gereken:** terminalde `Now listening on: http://[::]:8080` satırı. Tarayıcıda **http://localhost:8080** adresini açın.

| İş | Komut |
|---|---|
| Durdurmak | Terminalde `Ctrl+C` |
| Arka planda çalıştırmak | `docker compose up --build -d` |
| Arka plandakini durdurmak | `docker compose down` |
| Veritabanını tamamen sıfırlamak | `docker compose down -v` |
| Kodu değiştirdikten sonra | `docker compose up --build` (yeniden derler) |
| Logları görmek | `docker compose logs -f web` |

---

## 3B. .NET SDK ile çalıştırma

Proje klasöründe sırayla:

```bash
dotnet tool restore
```

```bash
dotnet build
```

```bash
dotnet test
```

```bash
dotnet run --project src/BelediyeBasvuru.Web
```

✅ **Görmeniz gereken:** `dotnet test` → `Başarılı: 2`; `dotnet run` → `Now listening on: http://localhost:5248`. Tarayıcıda **http://localhost:5248**.

Durdurmak için `Ctrl+C`.

> `dotnet` komutu bulunamıyorsa .NET 10 SDK kurulu değildir veya PATH'te değildir. Kurduktan sonra terminali kapatıp yeniden açın.

---

## 4. Kodu düzenlemek için açma

### VS Code
1. **File → Open Folder** → `BelediyeBasvuru` klasörünü seçin.
2. Sağ altta önerilen **C# Dev Kit** eklentisini kurun.
3. Çalıştırmak: **Terminal → New Terminal** → `dotnet run --project src/BelediyeBasvuru.Web`
   veya **F5** (ilk seferde "C#" seçin, proje olarak `BelediyeBasvuru.Web`'i seçin).
4. Testler: sol menüdeki **Test (deney tüpü) simgesi** → ▶.

### Visual Studio 2026
1. `BelediyeBasvuru.sln` dosyasına çift tıklayın.
2. Üstteki başlatma projesi **BelediyeBasvuru.Web** olsun → yeşil ▶ (**http** profili).
3. Testler: **Test → Test Explorer** → Run All.

---

## 5. Hangi kod nerede?

Her dosyanın içinde satır satır Türkçe açıklama vardır; önce açıklamaları okuyun.

| Ne arıyorsun? | Dosya / Klasör |
|---|---|
| Uygulama nereden başlıyor? | `src/BelediyeBasvuru.Web/Program.cs` |
| Ana sayfanın HTML'i | `src/BelediyeBasvuru.Web/Views/Home/Index.cshtml` |
| Tüm sayfaların ortak menüsü ve alt bilgisi | `src/BelediyeBasvuru.Web/Views/Shared/_Layout.cshtml` |
| Ortak renkler ve stiller | `src/BelediyeBasvuru.Web/wwwroot/css/site.css` (renkler en üstte `:root`) |
| Ana sayfa stilleri | `src/BelediyeBasvuru.Web/wwwroot/css/anasayfa.css` |
| Fotoğraflar ve lisansları | `src/BelediyeBasvuru.Web/wwwroot/img/anasayfa/` (`KAYNAKLAR.md`) |
| Hangi adres hangi sayfayı açar? | `src/BelediyeBasvuru.Web/Controllers/` (ör. `HomeController.cs`) |
| Kullanıcı sınıfı ve varlıklar | `src/BelediyeBasvuru.Core/Entities/` |
| Durum enum'ları (O-08) | `src/BelediyeBasvuru.Core/Enums/` |
| Servis arayüzleri (O-09) | `src/BelediyeBasvuru.Core/Interfaces/` |
| Veritabanı bağlamı (tablolar) | `src/BelediyeBasvuru.Infrastructure/Data/AppDbContext.cs` |
| Servislerin kaydı (DI) | `src/BelediyeBasvuru.Infrastructure/DependencyInjection.cs` |
| Veritabanı bağlantı adresi | `src/BelediyeBasvuru.Web/appsettings.json` (Docker'da `docker-compose.yml` ezer) |
| Testler | `tests/BelediyeBasvuru.Tests/` |
| Docker tarifleri | `Dockerfile`, `docker-compose.yml` |
| GitHub'daki otomatik kontrol (CI) | `.github/workflows/ci.yml` |
| Ekip kuralları (dal, PR, inceleme) | `CONTRIBUTING.md` |

Katman kuralı: **Web → Infrastructure → Core**. Core hiçbir projeye referans vermez.

Bir sayfanın akışı:
```
Tarayıcı "/"  →  Program.cs (yönlendirme)  →  HomeController.Index()  →  Views/Home/Index.cshtml
                                                                         (+ _Layout.cshtml + site.css + anasayfa.css)
```

---

## 6. İlk değişikliğini yapma (deneme)

1. Yeni dal açın (kendi numaranızla):
   ```bash
   git checkout -b kisi2/ilk-deneme
   ```
2. `Views/Home/Index.cshtml` içinde bir yazıyı değiştirin, kaydedin.
3. Çalıştırıp görün (Yol A: `docker compose up --build`, Yol B: `dotnet run --project src/BelediyeBasvuru.Web`).
4. Gönderin:
   ```bash
   git add .
   git commit -m "K2-00: ilk deneme"
   git push -u origin kisi2/ilk-deneme
   ```
5. GitHub'da **Compare & pull request** → PR açın. CI yeşil olunca bir kişi onaylar, birleştirilir.

Ayrıntılı kurallar: [CONTRIBUTING.md](CONTRIBUTING.md)

---

## 7. Sık karşılaşılan sorunlar

| Sorun | Çözüm |
|---|---|
| `port is already allocated` / `address already in use` | 8080 veya 5248 portunu başka program kullanıyor. Onu kapatın ya da `docker-compose.yml`'de `"8081:8080"` yapıp http://localhost:8081 açın. |
| `A compatible .NET SDK was not found` | .NET 10 SDK kurun; terminali yeniden açın. |
| Docker: `db` konteyneri sürekli yeniden başlıyor | Docker Desktop → Settings → Resources → Memory en az **2 GB** olsun. |
| Apple Silicon (M1/M2) Mac'te SQL Server açılmıyor | Docker Desktop → Settings → General → "Use Rosetta for x86_64/amd64 emulation" açık olsun. |
| Windows dışı makinede `dotnet run` veritabanına bağlanamıyor | LocalDB sadece Windows'ta var. Yol A'yı (Docker) kullanın. |
| CSS değişikliğim görünmüyor | Tarayıcıda `Ctrl+F5` (önbelleksiz yenile). Docker'daysanız `--build` ile yeniden başlatın. |
| `git push` izin hatası | Davetiyeyi kabul ettiğinizden ve doğru hesapla giriş yaptığınızdan emin olun. |
