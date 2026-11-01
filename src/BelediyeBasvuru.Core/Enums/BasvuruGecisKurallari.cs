using System.Collections.Generic;

namespace BelediyeBasvuru.Core.Enums
{
    // 'static' ifadesi, bu kurallara başka bir yerden erişirken 'new' deyip nesne üretmeden 
    // doğrudan sınıf ismiyle (BasvuruGecisKurallari.GecisGecerliMi) ulaşmamızı sağlar.
    public static class BasvuruGecisKurallari
    {
        // Dictionary (Sözlük) yapısı:
        // Sol taraftaki duruma karşılık (Key), sağ tarafta o durumdan geçilebilecek durumların bir listesini (Value) tutar.
        private static readonly Dictionary<BasvuruDurumu, List<BasvuruDurumu>> GecisHaritasi = new()
        {
            // 'Yeni' olan bir başvuru ya birine 'Atanabilir' ya da direkt 'Reddedilebilir'.
            { BasvuruDurumu.Yeni, new List<BasvuruDurumu> { BasvuruDurumu.Atandi, BasvuruDurumu.Reddedildi } },

            // 'Atandı' olan bir başvuru 'İşleme' alınabilir ya da 'Reddedilebilir'.
            { BasvuruDurumu.Atandi, new List<BasvuruDurumu> { BasvuruDurumu.Islemde, BasvuruDurumu.Reddedildi } },

            // 'İşlemde' olan bir başvuru 'Çözüldü' olarak işaretlenebilir ya da 'Reddedilebilir'.
            { BasvuruDurumu.Islemde, new List<BasvuruDurumu> { BasvuruDurumu.Cozuldu, BasvuruDurumu.Reddedildi } },

            // 'Çözüldü' olan bir başvuru artık sadece 'Kapatıldı' yapılabilir.
            { BasvuruDurumu.Cozuldu, new List<BasvuruDurumu> { BasvuruDurumu.Kapatildi } },

            // 'Reddedildi' ve 'Kapatıldı' son aşamalardır.
            // Yanlarındaki boş liste 'new List<BasvuruDurumu>()', bu durumlardan sonra başka hiçbir duruma geçilemeyeceğini belirtir.
            { BasvuruDurumu.Reddedildi, new List<BasvuruDurumu>() },
            { BasvuruDurumu.Kapatildi, new List<BasvuruDurumu>() }
        };

        // Bu metot (fonksiyon), bir durum değişimi yapılmak istendiğinde geçişin kurallara uygun olup olmadığını denetler.
        public static bool GecisGecerliMi(BasvuruDurumu mevcutDurum, BasvuruDurumu hedefDurum)
        {
            // 1. Mevcut durum haritada var mı kontrol eder.
            // 2. Varsa, izin verilen hedefler listesinin içinde gitmek istediğimiz 'hedefDurum' var mı bakar.
            // Geçiş uygunsa true (doğru), mantıksız bir geçişse false (yanlış) döndürür.
            return GecisHaritasi.TryGetValue(mevcutDurum, out var izinVerilenler) 
                   && izinVerilenler.Contains(hedefDurum);
        }
    }
}