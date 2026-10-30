namespace BelediyeBasvuru.Web.Models;

// View model: controller'dan görünüme (Error.cshtml) veri taşıyan basit sınıf.
public class ErrorViewModel
{
    // Hatanın izleme numarası. "string?" → boş (null) olabilir.
    public string? RequestId { get; set; }

    // Numara varsa true döner; görünüm buna bakarak numarayı gösterip göstermemeye karar verir.
    // "=>" → sadece okunabilen, her seferinde hesaplanan özellik.
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
