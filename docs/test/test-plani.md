# Test Planı

**Sahibi:** Kişi 5 (Serhat Nas) · **Görev kodu:** K5-06 · **Aşama:** Sistem Çözümleme (Kontrol 1)
**İlgili belgeler:** `docs/analiz/guvenlik-gereksinimleri.md` (GG-xx), `docs/analiz/gostergeler.md` (G1-G9)
**Sonuç belgesi:** `docs/test/test-raporu.md` (Kontrol 2/3'te bu plana göre doldurulacak)
**Kapsam dışı:** Kullanıcı kabul testi (UAT) — bkz. `docs/test/kabul-testi.md` (Kişi 2)

## 1. Amaç

Bu belge, BelediyeBaşvuru sisteminin hangi seviyelerde, hangi araçlarla, hangi kriterlere göre
test edileceğini tanımlar. Test stratejisi "test piramidi" ilkesine dayanır: çok sayıda hızlı
birim testi, daha az sayıda entegrasyon testi, en üstte az sayıda manuel/uçtan uca kontrol.

## 2. Test Seviyeleri ve Araçlar

| Seviye | Ne test edilir | Araç | Nerede çalışır |
|---|---|---|---|
| Birim testi (unit) | Tek bir sınıf/metot, bağımlılıkları sahte (mock/fake) ile değiştirilerek | xUnit + Moq | Her geliştiricinin makinesi + CI (her PR) |
| Entegrasyon testi | Birden fazla katman birlikte (controller → servis → veritabanı), gerçek ASP.NET Core host | xUnit + `WebApplicationFactory<Program>` | CI (SQL Server 2022 konteyneri ile) |
| Veritabanı testi | EF Core modelinin gerçek SQL Server'a karşı oluşturulabilirliği | xUnit + `Microsoft.Data.SqlClient` | CI; yerelde bağlantı dizesi yoksa atlanır (bkz. `VeritabaniTestleri.cs`) |
| Güvenlik testi | GG-xx gereksinimlerinin doğrulanması (yetkisiz erişim, CSRF, SQL enjeksiyonu denemeleri) | xUnit entegrasyon testleri + manuel kontrol listesi | CI (otomatikleşenler) + Kontrol öncesi manuel tur |
| Manuel/uçtan uca | Vatandaş gözünden tam senaryo (başvuru oluştur → takip et → personel çözsün) | Tarayıcıda elle | Her artım sonunda (Kontrol 2, Kontrol 3) |

**CI akışı:** Her PR'da GitHub Actions, SQL Server 2022 konteynerini ayağa kaldırır, `dotnet test`
çalıştırır. CONTRIBUTING.md kuralı: "CI yeşil + 1 onay → birleştir."

## 3. Giriş/Çıkış Kriterleri

| | Kriter |
|---|---|
| **Test aşamasına giriş** | İlgili PR açılmış, `dotnet build` hatasız, en az 1 test eklenmiş (CONTRIBUTING.md kuralı) |
| **PR'ın birleşmesi için çıkış** | Tüm testler yeşil (CI) + en az 1 inceleyici onayı |
| **Artım 1 (Kontrol 2) çıkışı** | MVP göstergeleri (G1-G5, bkz. gostergeler.md) gerçek veriyle çalışıyor + ilgili testler yeşil |
| **Artım 2 (Kontrol 3) çıkışı** | GG-xx güvenlik testlerinin tamamı otomatik veya manuel olarak doğrulanmış |

## 4. İzlenebilirlik Matrisi (Gereksinim → Test)

Bu tablo, her güvenlik/gösterge gereksiniminin **hangi testle** doğrulanacağını gösterir;
böylece "yazdık ama test etmedik" durumu görünür olur.

| Gereksinim | Test türü | Test adı / durumu |
|---|---|---|
| GG-01 (parola hash'leme) | Birim | Planlı — K1-08 sonrası |
| GG-03 (hesap kilitleme) | Entegrasyon | Planlı — K1-08 sonrası |
| GG-06 (rol bazlı yetki) | Entegrasyon | Planlı — K1-09 sonrası |
| GG-08 (panel sadece Yönetici) | Entegrasyon | Planlı — K1-09 sonrası (şu an panel herkese açık, bkz. not aşağıda) |
| GG-11 (SQL enjeksiyonu yok) | Kod incelemesi + statik arama | Her PR'da inceleyici kontrol eder |
| GG-13 (CSRF belirteci) | Entegrasyon | Planlı — K1-10 sonrası |
| GG-14 (dosya yükleme sınırı) | Birim | Planlı — K4 (`IDosyaDepolamaServisi`) sonrası |
| G1-G5 (panel göstergeleri doğru hesaplanıyor) | Birim | ✅ Yazıldı: `SahteIstatistikServisiTestleri` (`YonetimTestleri.cs`) — G1 (açık başvuru toplamı) ve G5 (7 günlük trend) için |
| Panel sayfası açılıyor | Entegrasyon (smoke) | ✅ Yazıldı: `YonetimPaneliTestleri.YonetimPaneli_200_Doner` |

> **Not (K5-02 ile ilgili bilinen eksik):** `/Yonetim` adresi şu an Identity henüz kurulmadığı için
> `[Authorize]` ile korunmuyor (bkz. `YonetimController.cs` içindeki yorum). GG-08 testi, K1-09
> (roller/policy'ler) birleşince yazılacak ve CI'a eklenecektir. Bu, bilinen ve kabul edilmiş bir
> geçici durumdur, unutulmaması için burada ve `guvenlik-gereksinimleri.md`'de işaretlenmiştir.

## 5. Test Senaryosu Şablonu

Her yeni test senaryosu şu alanlarla yazılır (xUnit test adında da aynı desen: `Ne_Olunca_NeOlmalı`):

| Alan | Açıklama |
|---|---|
| Kod | ör. `T-G1-01` |
| Ön koşul | Test başlamadan önce sistemin durumu |
| Adımlar | Hazırla (Arrange) → Çalıştır (Act) → Doğrula (Assert) |
| Beklenen sonuç | Net, ölçülebilir |
| İlgili gereksinim | GG-xx veya G-x kodu |

## 6. Test Ortamı

- **Yerel:** LocalDB (Windows) veya Docker'da SQL Server 2022 + `dotnet user-secrets`.
- **CI:** GitHub Actions, SQL Server 2022 konteyneri, `ConnectionStrings__Default` ortam değişkeni.
- Testler **kalıcı veri bırakmaz**: `VeritabaniTestleri.cs` örneğindeki gibi her test kendi geçici
  veritabanını/verisini oluşturur ve sonunda temizler.

## 7. Roller

| Rol | Sorumluluk |
|---|---|
| Her geliştirici | Kendi PR'ındaki kod için birim/entegrasyon testi yazar (CONTRIBUTING.md kuralı) |
| Kişi 5 | Bu planın güncel tutulması, güvenlik testlerinin takibi, `test-raporu.md`'nin Kontrol 2/3'te doldurulması |
| İnceleyen (sıradaki kişi, 1→2→3→4→5→1) | PR'daki testin anlamlı olup olmadığını (sadece "geçmek için" yazılmadığını) değerlendirir |

## 8. Kabul Kriterleri (bu belge için)

- [x] Her test seviyesi için araç ve çalışma ortamı belirtildi.
- [x] İzlenebilirlik matrisi kuruldu; mevcut durumda hangi testlerin yazıldığı/planlı olduğu açık.
- [x] Bilinen eksik (GG-08/panel yetkisi) açıkça işaretlendi, gizlenmedi.
- [ ] Kontrol 1'de ekip ve asistanla gözden geçirilecek.
