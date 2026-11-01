using System;
using System.Collections.Generic;
using System.Linq;
using BelediyeBasvuru.Core.Enums;
using BelediyeBasvuru.Core.Interfaces;

namespace BelediyeBasvuru.Infrastructure.Services
{
    // 'IBasvuruService' arayüzünü uygulayan (implement eden) sahte servis sınıfımız.
    // Arayüzdeki tüm metotları burada doldurmak zorundayız.
    public class MockBasvuruService : IBasvuruService
    {
        // Bellekte geçici olarak tutacağımız sahte başvuru listesi.
        private readonly List<object> _basvurular = new List<object>();

        public MockBasvuruService()
        {
            // Test edebilmemiz için başlangıçta birkaç sahte veri ekliyoruz.
            _basvurular.Add(new { Id = 1, TcKimlikNo = "12345678901", AdSoyad = "Ahmet Yılmaz", Konu = "Yol Tamiri", Durum = BasvuruDurumu.Yeni });
            _basvurular.Add(new { Id = 2, TcKimlikNo = "98765432109", AdSoyad = "Ayşe Kaya", Konu = "Park Temizliği", Durum = BasvuruDurumu.Atandi });
        }

        public List<object> TumBasvurulariGetir()
        {
            return _basvurular;
        }

        public object? BasvuruGetirById(int id)
        {
            // Listeden Id değerine uyan ilk başvuruyu arar, bulamazsa null döner.
            return _basvurular.FirstOrDefault(b => (int)b.GetType().GetProperty("Id")!.GetValue(b)! == id);
        }

        public int BasvuruOlustur(string tcKimlikNo, string adSoyad, string konu, string detay)
        {
            int yeniId = _basvurular.Count + 1;
            var yeniBasvuru = new
            {
                Id = yeniId,
                TcKimlikNo = tcKimlikNo,
                AdSoyad = adSoyad,
                Konu = konu,
                Detay = detay,
                Durum = BasvuruDurumu.Yeni
            };

            _basvurular.Add(yeniBasvuru);
            return yeniId;
        }

        public bool BasvuruDurumGuncelle(int basvuruId, BasvuruDurumu yeniDurum)
        {
            // Önceki görevde yazdığımız durum geçiş kurallarını (BasvuruGecisKurallari) kullanarak kontrol sağlayacağız.
            // Şimdilik sahte serviste işlemimizin başarılı olduğunu varsayıp true dönüyoruz.
            return true;
        }
    }
}