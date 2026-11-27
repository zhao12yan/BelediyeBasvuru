using BelediyeBasvuru.Core.Enums;
using BelediyeBasvuru.Infrastructure.Services;

namespace BelediyeBasvuru.Tests;

/// <summary>
/// Başvuru durum geçiş kuralları ve örnek servis akışını doğrulayan testler (H1/H3-K5).
/// </summary>
public class BasvuruAkisTestleri
{
    [Theory]
    [InlineData(BasvuruDurumu.Yeni, BasvuruDurumu.Atandi, true)]
    [InlineData(BasvuruDurumu.Yeni, BasvuruDurumu.Reddedildi, true)]
    [InlineData(BasvuruDurumu.Atandi, BasvuruDurumu.Islemde, true)]
    [InlineData(BasvuruDurumu.Atandi, BasvuruDurumu.Reddedildi, true)]
    [InlineData(BasvuruDurumu.Islemde, BasvuruDurumu.Cozuldu, true)]
    [InlineData(BasvuruDurumu.Islemde, BasvuruDurumu.Reddedildi, true)]
    [InlineData(BasvuruDurumu.Cozuldu, BasvuruDurumu.Kapatildi, true)]
    [InlineData(BasvuruDurumu.Yeni, BasvuruDurumu.Cozuldu, false)]
    [InlineData(BasvuruDurumu.Yeni, BasvuruDurumu.Kapatildi, false)]
    [InlineData(BasvuruDurumu.Atandi, BasvuruDurumu.Cozuldu, false)]
    [InlineData(BasvuruDurumu.Kapatildi, BasvuruDurumu.Yeni, false)]
    [InlineData(BasvuruDurumu.Reddedildi, BasvuruDurumu.Atandi, false)]
    public void GecisGecerliMi_DurumKurallariniDogrular(
        BasvuruDurumu mevcutDurum,
        BasvuruDurumu hedefDurum,
        bool beklenen)
    {
        var sonuc = BasvuruGecisKurallari.GecisGecerliMi(mevcutDurum, hedefDurum);
        Assert.Equal(beklenen, sonuc);
    }

    [Fact]
    public void MockBasvuruService_OlusturulanBasvuru_ListedeVeIdSorgusundaBulunur()
    {
        var servis = new MockBasvuruService();
        var baslangicSayisi = servis.TumBasvurulariGetir().Count;

        var yeniId = servis.BasvuruOlustur(
            "11111111110",
            "Zeynep Demir",
            "Sokak Aydınlatması",
            "Park girişindeki direk yanmıyor.");

        Assert.True(yeniId > baslangicSayisi);
        Assert.Equal(baslangicSayisi + 1, servis.TumBasvurulariGetir().Count);
        Assert.NotNull(servis.BasvuruGetirById(yeniId));
    }

    [Fact]
    public void BasvuruYasamDongusu_VatandastanKapanisa_TumAdimlarGecerli()
    {
        var adimlar = new[]
        {
            BasvuruDurumu.Yeni,
            BasvuruDurumu.Atandi,
            BasvuruDurumu.Islemde,
            BasvuruDurumu.Cozuldu,
            BasvuruDurumu.Kapatildi
        };

        for (var i = 0; i < adimlar.Length - 1; i++)
        {
            Assert.True(BasvuruGecisKurallari.GecisGecerliMi(adimlar[i], adimlar[i + 1]));
        }
    }
}
