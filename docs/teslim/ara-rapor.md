# BelediyeBaşvuru · Ara sürüm raporu

**21 Kasım 2026 · Buğra Doğan · 7 Aralık kontrolü için çalışma sürümü**

## Çalışan bölüm

Kullanıcılar Identity ile kayıt olabilir, giriş yapabilir ve çıkış yapabilir. Vatandaş, birim personeli ve yönetici rolleri migration ile oluşturulur. Yönetici, başka bir hesabın rolünü ve personel birimini değiştirip hesabı devre dışı bırakabilir. Değişiklik, yetki kaydı ve denetim satırı birlikte kaydedilir. Mevcut hesaplar yükseltmede etkin kalır. Hesap durumunun değişmesi önceki oturumu geçersiz kılar.

## Doğrulama

Windows'ta ayrı bir proje kopyası, .NET SDK 10.0.402 ve Release yapılandırması kullanıldı. Derleme hatasız tamamlandı. VSTest testhost bağlantısı bu ortamda zaman aşımına uğradığı için aynı test derlemesi xUnit yürütücüsüyle işlem içinde çalıştırıldı. 41 testin 40'ı geçti. Gerçek SQL Server testi bağlantı verilmediği için atlandı. LocalDB örneği sistem kayıtlarına erişemedi. EF model denetimi bekleyen değişiklik bulmadı ve SQL Server için tekrar çalıştırılabilir migration betiği üretildi. Docker ve SQL yedekten dönüş provası için CI sonucu bekleniyor.

## Ortak sürümün açık işleri

| Sahip | Beklenen teslim | Aynı sürümde tekrar denenecek işlem |
|---|---|---|
| Buğra | #58–61 için kod incelemesi ve CI, kurulum provası | Boş veritabanına kurulumdan sonra üç ayrı rolle giriş |
| Süleyman | #21 vatandaş ekranlarının gerçek giriş ve başvuruya bağlantısı | Formu gönder, sayfayı kapatıp aynı başvuruyu yeniden aç |
| Ahmet | Kalıcı başvuru servisi, otomatik atama ve geçmiş | Kayıt oluştur, birime ata, durum değişikliğini yeniden sorgula |
| Yusuf | #19 ortak şema, güvenli dosya ve birim ekranı | Kendi birimindeki kaydı aç, başka birimin kaydına erişimin reddedildiğini doğrula |
| Serhat | G1–G5 ve uçtan uca kabul raporu | Yeni başvurunun panelde sayıldığını doğrula ve sonucu aynı commit ile kaydet |

Birleşme denemesinde #21 için Program.cs ve ortak sayfa düzeni, #19 için Program.cs, durum enum'u ve EF model snapshot'ı çakıştı. Kimlik migration zinciri ve politika tanımları korunmalı. Veri kaydının aynı veritabanında tamamlandığı, yüklenen dosyanın yetkiyle okunduğu ve yönetim panelinin bu kaydı saydığı birlikte gösterildiğinde %60 paket bu sonuçlarla güncellenebilir.

## Teslim içeriği

[Kurulum ve bakım](../kilavuz/kurulum-bakim.md) · [Demo adımları](../test/kontrol2-demo.md) · [Ara sunum](ara-sunum.html) · [Görev takvimi](../planlama/gantt-v3.md). H7 final paketi için Serhat'ın nihai raporu, kullanıcı kılavuzu, dosyalarla birlikte geri dönüş provası ve üç rolün ortak sürümde kabulü bekleniyor.
