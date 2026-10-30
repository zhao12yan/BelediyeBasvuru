# Entities (Varlıklar)

**Sahibi: Kişi 4 (+ Kişi 3)** · Görev: **O-07 Ortak Şema v1** (21–25 Ekim)

Eklenecek sınıflar: `Basvuru`, `Kategori`, `Birim`, `Mahalle`, `DurumGecmisi`
(`Kullanici` iskelette hazır, alan eklenebilir).

Kurallar:
- Sınıf ve alan adları Türkçe, ASCII (ör. `Basvuru`, `TakipNo`, `OlusturmaZamani`).
- EF yapılandırması (Fluent API) burada değil, `Infrastructure/Data/Configurations` altında yapılır.
- Her varlık eklendiğinde `AppDbContext` içine `DbSet` eklenir.
