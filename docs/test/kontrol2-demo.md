# %60 Demo Senaryosu (Kontrol 2) — TASLAK

**Sahibi:** Kişi 1 · **Görev:** H4-K1 (#42), H5-K1 (#47) · **Durum:** Taslak — K2/K3/K4 modülleri birleştikçe doldurulacak.
**Kabul raporu:** Kişi 5 (Serhat) bu senaryoyu çalıştırıp sonucu `docs/test/` altına yazar.

> Kural: Demoda yalnız **gerçekten çalışan** adımlar gösterilir. "Bekliyor" satırları birleşmeden demo senaryosuna alınmaz;
> önizleme/oturum verisi kalıcı sistem gibi sunulmaz.

## 0. Temiz kurulum provası

| # | Adım | Beklenen | Durum |
|---|---|---|---|
| 0.1 | Temiz klasöre `git clone`, `.env.ornek` → `.env`, `ORNEK_PAROLA_*` doldur | — | Hazır |
| 0.2 | `docker compose up --build` | Log'da migration + "Örnek hesap oluşturuldu" ×3; http://localhost:8080 açılır | Hazır (H2-K1) |
| 0.3 | Yol B: `dotnet test` (LocalDB bağlantısıyla) | Başarısız: 0, Atlanan: 0 | Hazır |

## 1. Vatandaş

| # | Adım | Beklenen | Durum |
|---|---|---|---|
| 1.1 | Kayıt Ol → yeni vatandaş | Ana sayfaya döner, üstte e-posta ve "Çıkış Yap" görünür | Hazır (H2-K1) |
| 1.2 | Zayıf parolayla kayıt | Hata mesajı, hesap açılmaz | Hazır |
| 1.3 | Başvuru oluştur (kategori, konum, fotoğraf) | Takip numarası verilir, kayıt veritabanında | **Bekliyor** — H3-K2/K3/K4 |
| 1.4 | Başvurularım | Yalnız kendi başvuruları listelenir | **Bekliyor** — H4-K2/K3 |
| 1.5 | Takip no ile sorgu | Durum ve geçmiş görünür; 30+/dk sorguda 429 | **Bekliyor** — H4-K3 (`IstekSinirlari.Sorgu`) |
| 1.6 | `/Yonetim` adresini elle yaz | 403 "Erişim Engellendi" | Hazır (H2-K1) |

## 2. Birim personeli (`personel@ornek.test`, birim 1)

| # | Adım | Beklenen | Durum |
|---|---|---|---|
| 2.1 | Giriş | Başarılı | Hazır |
| 2.2 | Personel ekranı | Yalnız birim 1'e atanmış başvurular | **Bekliyor** — H3-K4 (`YetkiliOlanlar`) |
| 2.3 | Başka birimin başvurusunu adresle aç (`.../Detay/{id}`) | 403 | **Bekliyor** — kural hazır (H3-K1), uç nokta K3/K4 |
| 2.4 | Durum güncelle (İnceleniyor → Çözüldü) | Geçmişe kayıt düşer, vatandaş görür | **Bekliyor** — H3-K3 |
| 2.5 | `/Yonetim` | 403 | Hazır |

## 3. Yönetici (`yonetici@ornek.test`)

| # | Adım | Beklenen | Durum |
|---|---|---|---|
| 3.1 | Giriş → `/Yonetim` | Panel açılır | Hazır (sahte istatistik) |
| 3.2 | Panel gerçek G1–G5 verisini gösterir | 1.3'te açılan başvuru sayılara yansır | **Bekliyor** — H4-K5 |

## 4. Güvenlik gösterimi (kısa)

| # | Adım | Beklenen | Durum |
|---|---|---|---|
| 4.1 | 5 kez yanlış parola | Hesap 15 dk kilitlenir | Hazır |
| 4.2 | 1 dk içinde 11. giriş denemesi | 429 "Çok fazla istek" | Hazır (H3-K1) |
| 4.3 | Tarayıcı geliştirici araçlarında yanıt başlıkları | `X-Frame-Options: DENY`, CSP vb. | Hazır |

## 5. Engelleyiciler (27 Kasım'da kişilere dağıtılacak)

- PR #19 (Yusuf): `KimlikBaslangic` migration'ının üzerine yeniden üretilmeli; `Basvuru : IYetkiliKayit` + `BirimId`.
- PR #21 (Süleyman): giriş formu `/Hesap/Giris`'e bağlanmalı; önizleme oturum verisi demo akışından çıkarılmalı.
- Başvuru/sorgu uç noktalarına `IstekSinirlari.Basvuru` / `Sorgu` eklenmeli.
