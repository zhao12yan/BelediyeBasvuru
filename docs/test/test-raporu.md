# Test Raporu

**Sahibi:** Kişi 5 (Serhat Nas) · **Görevler:** H1-K5 (#27), H2-K5 (#32), H3-K5 (#37), H4-K5 (#41)  
**Tarih:** 27.11.2026 · **Hedef dal:** `main`

> Bu rapor, `main` dalında **gerçekten çalışan ve doğrulanan** testler ile **diğer ekip modüllerinin birleşmesini bekleyen** işleri net biçimde ayırır. Örnek/sahte veriyle geçen kontroller gerçek veritabanı testi olarak sunulmaz.

## 1. Tamamlanan ve Çalışan Testler (Kanıtlı)

| Test Sınıfı / Metodu | Tür | Kapsam | Veri Kaynağı | Durum |
|---|---|---|---|---|
| `IskeletTestleri.AnaSayfa_200_Doner` | Entegrasyon (smoke) | Ana sayfa (`/`) 200 OK döner | Sunucu (in-memory) | ✅ Geçti |
| `YonetimPaneliTestleri.YonetimPaneli_200_Doner` | Entegrasyon (smoke) | `/Yonetim` paneli 200 OK döner | `SahteIstatistikServisi` (örnek veri) | ✅ Geçti |
| `SahteIstatistikServisiTestleri` (4 test) | Birim | G1 açık toplam, G2 bugünkü yeni, G5 7 günlük trend, `BasvuruDurumu` enum adları uyumu | `SahteIstatistikServisi` (örnek veri) | ✅ Geçti |
| `BasvuruAkisTestleri.GecisGecerliMi_DurumKurallariniDogrular` (12 senaryo) | Birim | Geçerli ve yasak durum geçişleri (`Yeni`, `Atandi`, `Islemde`, `Cozuldu`, `Kapatildi`, `Reddedildi`) | `BasvuruGecisKurallari` | ✅ Geçti |
| `BasvuruAkisTestleri.MockBasvuruService_OlusturulanBasvuru_ListedeVeIdSorgusundaBulunur` | Birim | Başvuru oluşturma, listeleme ve ID ile sorgulama | `MockBasvuruService` (bellek içi) | ✅ Geçti |
| `BasvuruAkisTestleri.BasvuruYasamDongusu_VatandastanKapanisa_TumAdimlarGecerli` | Birim | Vatandaş → Atama → İşlem → Çözüm → Kapatma zinciri | `BasvuruGecisKurallari` | ✅ Geçti |
| `VeritabaniTestleri.Model_SqlServerda_Olusturulabilir` | Veritabanı | `AppDbContext` şemasının gerçek SQL Server'da oluşturulması | SQL Server (CI'da zorunlu; yerelde bağlantı yoksa atlanır) | ✅ CI'da çalışır |
| `VeritabaniTestleri.Kullanici_SqlServera_KaydedilipOkunabilir` | Veritabanı | `Kullanici` kaydı yazma ve okuma doğrulaması | SQL Server (CI'da zorunlu; yerelde bağlantı yoksa atlanır) | ✅ CI'da çalışır |

## 2. Bağımlılık Nedeniyle Bekleyen Kontroller (Blokerler)

Aşağıdaki maddeler, ilgili modüller `main` dalına birleştiğinde K5 tarafından çalıştırılıp bu rapora işlenecektir:

| Bekleyen Test / Kontrol | İlgili Görev | Beklenen Modül / Sahip | Açıklama |
|---|---|---|---|
| Gerçek kayıt, giriş, rol ve `/Yonetim` 403 yetki kontrolü | #32, #41 | K1 (#29, #33 — `kisi1/h2-kimlik`, `kisi1/h3-guvenlik`) | `HesapController` ve `[Authorize(Policy = Politikalar.Yonetici)]` henüz `main`'e birleşmedi |
| Güvenli fotoğraf deposu (tür/boyut sınırı, yetkisiz erişim) | #32, #37 | K4 (#28 — `IDosyaDepolamaServisi`) | Dosya servisi ve depo henüz `main`'de yok |
| Vatandaş → SQL → Personel uçtan uca akışı | #37 | K2 (#21, #31, #36), K3 (#30, #34), K4 (#35) | `Basvuru` varlığı, kalıcı servis ve personel ekranı henüz `main`'e birleşmedi |
| Gerçek SQL sorgulu G1–G5 `IstatistikServisi` | #41 | K4 (#25, #38), K3 (#30) | `AppDbContext.Basvurular` DbSet'i eklendiğinde `SahteIstatistikServisi` yerine gerçek servis bağlanacak |
