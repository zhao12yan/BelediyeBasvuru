# Interfaces (Servis Sözleşmeleri)

Görev: **O-09 Servis Arayüzleri + Sahte Servisler** (22–25 Ekim)

| Arayüz | Sahibi | Sahte implementasyon (Infrastructure/Services) |
|---|---|---|
| `IBildirimServisi` | Kişi 3 | `SahteBildirimServisi` |
| `IDosyaDepolamaServisi` (Kaydet / Getir / Sil) | Kişi 4 | `SahteDosyaServisi` |

Sonraki haftalarda eklenecekler: `IBasvuruServisi`, `IAtamaServisi`, `IDurumServisi`,
`ITakipSorguServisi` (Kişi 3), `IKullaniciYonetimServisi` (Kişi 1).

Kural: Modüller arası bağımlılık önce **arayüz + sahte servis** ile çözülür; gerçek
implementasyon hazır olunca `DependencyInjection.cs` içindeki kayıt değiştirilir.
