using System.Collections.Generic;
using BelediyeBasvuru.Core.Enums;

namespace BelediyeBasvuru.Core.Interfaces
{
    // C#'ta 'interface' (arayüz), bir sınıfın hangi metotları (fonksiyonları) barındırmak zorunda olduğunu belirten bir sözleşmedir.
    // İçerisinde kod bloğu { } bulunmaz, sadece metot imzaları yer alır.
    public interface IBasvuruService
    {
        // Sistemdeki tüm başvuruları liste olarak getiren metot sözleşmesi.
        // List<object> geçici bir yapıdır; ileride bunu gerçek başvuru modelimize (Entity/DTO) dönüştüreceğiz.
        List<object> TumBasvurulariGetir();

        // Id numarasına göre tek bir başvurunun detayını getiren metot sözleşmesi.
        object? BasvuruGetirById(int id);

        // Yeni bir başvuru oluşturup geriye oluştusal başvurunun Id'sini dönen metot sözleşmesi.
        int BasvuruOlustur(string tcKimlikNo, string adSoyad, string konu, string detay);

        // Var olan bir başvurunun durumunu (statüsünü) değiştiren metot sözleşmesi.
        // Durum değiştirme başarılı olduysa 'true', kurala takıldıysa 'false' döner.
        bool BasvuruDurumGuncelle(int basvuruId, BasvuruDurumu yeniDurum);
    }
}