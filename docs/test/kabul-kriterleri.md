# Kabul Kriterleri (%60 ve %100 Teslim Listesi)

**Sahibi:** Kişi 5 (Serhat Nas) · **Görev:** H1-K5 (#27)  
**İlgili belgeler:** `docs/test/test-plani.md`, `docs/test/test-raporu.md`, `docs/analiz/gostergeler.md`

## 1. %60 Kabul Listesi (Kontrol 2 — MVP)

| Kod | Kapsam | Sahip | Doğrulama / Test | Teslim Artımı |
|---|---|---|---|---|
| K60-01 | Gerçek kayıt, giriş, çıkış ve 3 rol (`Vatandas`, `Personel`, `Yonetici`) | K1 (#29) | Kimlik entegrasyon testleri + `/Hesap/Giris` | Artım 1 (%60) |
| K60-02 | Giriş ekranı ve başvuru formunun kalıcı servise bağlanması | K2 (#21, #31, #36) | Form gönderimi + takip no üretimi | Artım 1 (%60) |
| K60-03 | Kalıcı başvuru kaydı, harita konumu ve güvenli fotoğraf deposu | K3 (#30), K4 (#28) | SQL'de başvuru + dosya boyutu/tür kontrolü | Artım 1 (%60) |
| K60-04 | Temel otomatik atama ve durum geçiş kuralları (`Yeni → Atandi → Islemde → Cozuldu → Kapatildi`) | K3 (#34) | `BasvuruAkisTestleri` + servis testleri | Artım 1 (%60) |
| K60-05 | Personel iş ekranının gerçek başvuru ve fotoğraflara bağlanması | K4 (#35, #38) | Yalnız kendi biriminin başvurularını görme (403 kontrolü) | Artım 1 (%60) |
| K60-06 | Güvenli takip sorgusu ve Başvurularım ekranı + istek sınırı (429) | K3 (#39), K2 (#40) | Sahiplik kontrolü + 429 testi | Artım 1 (%60) |
| K60-07 | Yönetim panelinin gerçek G1–G5 istatistiklerine bağlanması ve `Yonetici` yetkisi | K5 (#41), K1 (#29) | `YonetimTestleri` + 403 yetki testi | Artım 1 (%60) |

## 2. %100 Kabul Listesi (Kontrol 3 — Nihai Teslim)

| Kod | Kapsam | Sahip | Doğrulama / Test | Teslim Artımı |
|---|---|---|---|---|
| K100-01 | Gelişmiş durum akışı (yeniden açma / reddetme gerekçeleri) ve bildirimler | K3, K4 | Durum geçmişi + bildirim servisi testleri | Artım 2 (%100) |
| K100-02 | İleri istatistikler (G6 ortalama çözülme, G7 SLA ihlal, G8 birim performansı, G9 yeniden açılma) | K5 | `IIstatistikServisi` birim ve SQL testleri | Artım 2 (%100) |
| K100-03 | Tam güvenlik doğrulaması (GG-01..GG-14: hesap kilitleme, CSRF, XSS, SQLi, dosya sınırı) | K1, K5 | `GuvenlikTestleri` + manuel kontrol | Artım 2 (%100) |
| K100-04 | Eşzamanlı güncelleme koruması ve kapsamlı örnek veri | K4 (#38) | Çakışma/concurrency testi | Artım 2 (%100) |
| K100-05 | Mobil uyum, UAT ve nihai teslim provası | K2, K5, K1 | Uçtan uca tarayıcı senaryosu + `test-raporu.md` | Artım 2 (%100) |

## 3. Ortak Test Verisi Tablosu

Tüm ekip tarafından birim, entegrasyon ve manuel demo testlerinde kullanılacak ortak senaryo verileri:

| Tip | Kimlik / Ad | Rol / Birim | Kullanım Senaryosu |
|---|---|---|---|
| Kullanıcı 1 | `vatandas1@ornek.test` — Ayşe Kaya | `Vatandas` | Yeni başvuru açar, sadece kendi başvurularını görür |
| Kullanıcı 2 | `vatandas2@ornek.test` — Mehmet Demir | `Vatandas` | Kullanıcı 1'in başvurusuna/fotoğrafına erişemez (403) |
| Kullanıcı 3 | `personel1@ornek.test` — Ali Yılmaz | `Personel` (Birim 1: Fen İşleri) | Birim 1 başvurularını `Atandi → Islemde → Cozuldu` yapar |
| Kullanıcı 4 | `personel2@ornek.test` — Zeynep Çelik | `Personel` (Birim 2: Temizlik) | Birim 1 başvurularını göremez/düzenleyemez (403) |
| Kullanıcı 5 | `yonetici@ornek.test` — Fatma Arslan | `Yonetici` | `/Yonetim` panelinde G1–G5 göstergelerini görür |

### Örnek Başvuru Kayıtları (Durum Sözlüğü ile Uyumlu)

| Takip No (ör.) | Kategori | Durum (`BasvuruDurumu`) | Birim | G1 Açık mı? |
|---|---|---|---|---|
| `BB-2026-0001` | Yol / Kaldırım | `Yeni` | Fen İşleri | Evet |
| `BB-2026-0002` | Aydınlatma | `Atandi` | Fen İşleri | Evet |
| `BB-2026-0003` | Çevre / Temizlik | `Islemde` | Temizlik | Evet |
| `BB-2026-0004` | Su / Kanalizasyon | `Cozuldu` | Su-Kanal | Hayır |
| `BB-2026-0005` | Diğer | `Kapatildi` | Destek | Hayır |
| `BB-2026-0006` | Yol / Kaldırım | `Reddedildi` | Fen İşleri | Hayır |
