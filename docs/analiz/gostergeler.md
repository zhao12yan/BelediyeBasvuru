# Göstergeler (Yönetim Paneli Metrikleri)

**Sahibi:** Kişi 5 (Serhat Nas) · **Görev kodu:** K5-01 · **Aşama:** Sistem Çözümleme (Kontrol 1)
**İlgili dosyalar:** `docs/tasarim/guvenlik.md`, `docs/test/test-plani.md`, `src/BelediyeBasvuru.Web/Controllers/YonetimController.cs`

## 1. Amaç ve Kapsam

Bu belge, yöneticinin (Yönetici rolü) kullanacağı **istatistik / yönetim panelinde** hangi göstergelerin
gösterileceğini, her göstergenin **nasıl hesaplandığını**, **hangi veriden geldiğini** ve **ne sıklıkla
güncelleneceğini** tanımlar. Amaç: yöneticinin sistemin genel sağlığını ve birimlerin performansını
tek ekrandan, sayıya dökülmüş biçimde görebilmesi.

Kapsam dışı: vatandaş tarafındaki "başvurumu takip et" ekranı (Kişi 2'nin konusu) ve birim personeli
iş kuyruğu ekranı (Kişi 4'ün konusu). Bu belge sadece **yönetici özet panelini** kapsar.

## 2. Veri Bağımlılığı Notu

Bu belgenin yazıldığı tarihte (H1, O-07 şema/migration henüz birleşmedi) gerçek `Basvuru` tablosu
yoktur. Bu yüzden göstergeler önce **arayüz + sahte servis** (`IIstatistikServisi` /
`SahteIstatistikServisi`) ile tanımlanmış, panel örnek/sabit verilerle çalışır hâldedir.
O-07 (şema) ve O-08 (durum geçiş tablosu) birleşince `SahteIstatistikServisi`, gerçek
`AppDbContext` sorgularını yapan bir implementasyonla değiştirilecektir (`DependencyInjection.cs`
içindeki tek satır kayıt değişir, panel kodu değişmez).

## 3. Gösterge Tablosu

| # | Gösterge | Tanım / Formül | Veri Kaynağı (ileride) | Görselleştirme | Güncelleme Sıklığı | Öncelik |
|---|---|---|---|---|---|---|
| G1 | Toplam açık başvuru | Durumu `Çözüldü`/`Kapatıldı`/`Reddedildi` **dışındaki** başvuru sayısı | `Basvuru` tablosu, `Durum` alanı | Sayı kartı | Anlık (sayfa yüklenince) | MVP |
| G2 | Bugünkü yeni başvuru | `OlusturmaTarihi` = bugün olan başvuru sayısı | `Basvuru.OlusturmaTarihi` | Sayı kartı | Anlık | MVP |
| G3 | Durum dağılımı | Her durum (`Yeni`, `İnceleniyor`, `Atandı`, `Çözüldü`, `Reddedildi`) için adet ve % | `Basvuru.Durum` | Pasta/halka grafik | Anlık | MVP |
| G4 | Kategori dağılımı | Kategoriye (Yol, Aydınlatma, Çevre, Su-Kanalizasyon, Diğer) göre başvuru adedi | `Basvuru.Kategori` | Bar grafik | Günlük | MVP |
| G5 | Haftalık başvuru trendi | Son 7 gün, gün başına yeni başvuru sayısı | `Basvuru.OlusturmaTarihi` (gruplanmış) | Çizgi grafik | Günlük | MVP |
| G6 | Ortalama çözülme süresi | `ÇözülmeTarihi - OluşturmaTarihi` ortalaması (gün), yalnız `Çözüldü` durumundakiler | `Basvuru.OlusturmaTarihi`, `Basvuru.CozulmeTarihi` | Sayı kartı | Günlük | Artım 2 |
| G7 | SLA ihlal oranı | Hedef süreyi (kategoriye göre eşik, ör. 7 gün) aşan **açık** başvuru % | G1 + kategori eşik tablosu | Sayı kartı (uyarı rengiyle) | Günlük | Artım 2 |
| G8 | Birim performansı | Birim başına: açık / çözülen / ortalama süre | `Basvuru.BirimId` + G6 | Tablo | Günlük | Artım 2 |
| G9 | Yeniden açılma oranı | `Çözüldü` iken yeniden `Açık`a dönen başvuru % | Durum geçmişi (O-08 durum makinesi) | Sayı kartı | Haftalık | Artım 2 (bağımlı: O-08) |

**MVP** = Kontrol 2'ye (30 Kasım, Artım 1) kadar panelde gerçek veriyle çalışması hedeflenen göstergeler.
**Artım 2** = Kontrol 3'e (21 Aralık) kadar, ek alan/tablo gerektiren göstergeler.

## 4. Eşik / Hedef Değerler (taslak, toplantıda onaylanacak)

| Kategori | Hedef çözülme süresi (SLA) |
|---|---|
| Aydınlatma | 3 gün |
| Yol/Kaldırım | 10 gün |
| Çevre/Temizlik | 2 gün |
| Su-Kanalizasyon | 5 gün |
| Diğer | 7 gün |

> Bu tablo varsayımdır; gerçek değerler belediye ile teyit edilip `docs/toplanti-notlari.md`'ye
> karar olarak yazılacak (CONTRIBUTING.md: "Gereksinim değişikliği yalnız haftalık toplantıda
> karara bağlanır").

## 5. Arayüz Sözleşmesi (özet)

```csharp
// src/BelediyeBasvuru.Core/Interfaces/IIstatistikServisi.cs
public interface IIstatistikServisi
{
    Task<IstatistikOzeti> OzetGetirAsync();
}
```

`IstatistikOzeti` kaydı G1-G5'i karşılayan alanları taşır (bkz. ilgili dosya). G6-G9, şema netleşince
bu sözleşmeye eklenecektir; mevcut arayüz geriye dönük uyumlu şekilde genişletilecektir.

## 6. Kabul Kriterleri (bu belge için)

- [x] Panelde gösterilecek her gösterge için formül/tanım var.
- [x] Her göstergenin hangi veri alanına dayandığı belirtildi (şema gelince eşlenecek).
- [x] MVP / sonraki artım ayrımı yapıldı.
- [ ] Eşik değerleri belediye/hoca ile teyit edilecek (toplantı maddesi).
- [ ] G6-G9 için O-07/O-08 birleşince bu belge güncellenecek.

## 7. İlgili Görevler

- K5-01 (bu belge) → K5-02: `SahteIstatistikServisi` + panel iskeleti (bu PR'da birlikte yapıldı).
- K5-03 (sonraki): O-07 birleşince gerçek `IstatistikServisi` implementasyonu + xUnit testleri.
- K1 + K5 (O-11): İş kırılım yapısı / Gantt — bu göstergelerin hangi haftada "gerçek veri"ye
  bağlanacağı Gantt'a işlenecek.
