# Katkı ve Birlikte Çalışma Kuralları

## Dal ve PR
- `main` korumalıdır: **PR + 1 onay + yeşil CI** olmadan birleştirme yapılmaz.
- Dal adı: `kisiN/kisa-aciklama` — ör. `kisi2/basvuru-formu`, `kisi4/sema-v1`.
- Her iş bir **GitHub issue** + bir **PR** ile yapılır. PR başlığı görev koduyla başlar: `K3-05 Servis arayüzleri`.
- İş kuralı içeren her PR **en az bir xUnit testi** içerir.
- PR küçük tutulur (tercihen < 400 satır). Büyük işler parçalanır.

## Kod inceleme
- Sıra: **1 → 2 → 3 → 4 → 5 → 1** (Kişi N'nin PR'ını Kişi N+1 inceler).
- İnceleyen **48 saat** içinde yanıt verir; vermezse sıradaki kişi devralır.

## Günlük akış
```bash
git checkout main
git pull
git checkout -b kisiN/kisa-aciklama
# ... değişiklikler ...
dotnet build && dotnet test
git add .
git commit -m "K3-05: IBasvuruServisi arayüzü eklendi"
git push -u origin kisiN/kisa-aciklama
# GitHub'da PR aç
```
Uzun süren dalda: `git pull origin main` ile güncel kalın, çakışmayı kendi dalınızda çözün.

## Migration
- Eklemeden önce `main` çekilir.
- **Aynı anda tek kişi** migration ekler ve WhatsApp/Discord grubuna duyurur.
- Migration dosyaları elle düzenlenmez; hatalıysa `dotnet ef migrations remove` ile geri alınır.

## Gizli bilgi
- Bağlantı dizesi, parola, API anahtarı **depoya girmez**. Yerelde `dotnet user-secrets`, CI'da ortam değişkeni.
- Yanlışlıkla push edilirse hemen Kişi 1'e haber verin (geçmişten temizlenmesi gerekir).

## Kod kuralları
- Varlık, servis ve alan adları **Türkçe, ASCII**: `Basvuru`, `TakipNo`, `IAtamaServisi`.
- Ham SQL yasak (gerekirse yalnız `FromSqlInterpolated`). `Html.Raw` yasak.
- Durum geçiş kuralları yalnız sunucu tarafında, tek yerde. Arayüzde butonu gizlemek kontrol sayılmaz.

## Toplantı
- Her **Perşembe 20:00**, çevrim içi 45 dk. Herkes 2 dk: biten görevler (kodlarıyla), engeller, bu haftanın işleri.
- Kararlar `docs/toplanti-notlari.md` dosyasına yazılır.
- Gereksinim değişikliği yalnız haftalık toplantıda karara bağlanır.
- Görüş ayrılığında: konu toplantıda 5 dk tartışılır, uzlaşma yoksa ilgili alanın sahibi karar verir, karar ve gerekçe nota yazılır.
