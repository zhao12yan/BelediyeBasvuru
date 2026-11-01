namespace BelediyeBasvuru.Core.Enums
{
    // C#'ta 'enum' (enumeration), birbiriyle ilişkili sabit değerleri bir arada tutan bir tiptir.
    // Başvurunun alabileceği tüm durumları (statüleri) temsil eder.
    public enum BasvuruDurumu
    {
        Yeni = 1,        // Vatandaş başvuruyu ilk oluşturduğunda alacağı durum
        Atandi = 2,      // Başvurunun ilgili birime veya personele yönlendirildiği durum
        Islemde = 3,     // Personelin başvuru üzerinde çalışmaya başladığı durum
        Cozuldu = 4,     // Personelin sorunu çözdüğü ve işlem yaptığı durum
        Kapatildi = 5,   // Başvurunun tamamen sonlandırıldığı ve kapatıldığı durum
        Reddedildi = 6   // Başvurunun geçersiz görülüp reddedildiği durum
    }
}