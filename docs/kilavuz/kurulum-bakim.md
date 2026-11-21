# Kurulum ve bakım

## Yeni kurulum

[BASLANGIC.md](../../BASLANGIC.md) içindeki Docker veya .NET 10 kurulumunu kullanın. Prova için ayrı bir Compose proje adı seçin ve 8080 portunun boş olduğunu doğrulayın. Yerel .env dosyasına SQL ve üç örnek hesap için ayrı parolalar yazın. `docker compose -p belediye-prova up --build -d` ile başlatın. SQL hazır olduğunda uygulama migration'ları uygular, ardından örnek hesapları oluşturur. `docker compose -p belediye-prova logs web` ile durumu kontrol edin. Olağan durdurma komutu `docker compose -p belediye-prova down` veri birimini korur.

## Yönetici işlemi

Yönetici hesabıyla giriş yapıp Yönetim Paneli → Kullanıcı, rol ve birim yönetimi bağlantısını açın. Hedef hesabın e-posta adresini aratın. Birim personeli için geçerli birim seçin. Yönetici veya vatandaş için Birimsiz seçin. Kaydettiğinizde önceki oturum geçerliliğini kaybeder. Pasif hesaba giriş reddedilir. Tekrar etkinleştirmek için aynı formu kullanın. Başka bir yönetici güncellemişse hesabı yeniden aratın.

Aynı işlem içinde rol, birim, hesap durumu ve denetim kaydı kaydedilir. DenetimKayitlari tablosu yönetici ve hedef kimliği, UTC zamanı, bağlantı IP adresi ve eski/yeni yetkileri içerir. Aktif çalışma tabanında tek demo birimi 1'dir. Geçerli birim numaraları Yonetim:Birimler ayarından okunur. Yusuf'un birim tablosu ortak sürüme girdiğinde bu liste onunla eşleştirilmelidir.

## Sürüm yükseltme ve geri dönüş

Yükseltmeden önce çalışan sürümün commit kimliğini, uygulama ayarlarını ve son sağlıklı veritabanı yedeğini kaydedin. Fotoğraf deposu bağlandığında dosya yedeğini aynı zamana ait veritabanı yedeğiyle eşleştirin. Önce ayrı bir veritabanına geri yükleyip o kopyada yeni sürümle giriş, atama ve geçmiş kontrollerini çalıştırın. Şema değişikliğinde önceki uygulamaya dönmek için doğrulanmış yedeği ayrı bir veritabanına geri yükleyin ve eski sürümün bağlantısını ona yöneltin.

CI'ın Docker işinde `scripts/veritabani-prova.sql` geçici veritabanının sağlama toplamlı yedeğini alır, benzersiz isimli bir kopyaya geri yükler ve kopyadaki şemayı ve rol kayıtlarını sorgular. Bu prova kimlik verilerini kapsar. Ekip dosya deposunu ve başvuru şemasını birleştirdikten sonra aynı kontrole dosya içeriği ve başvuru geçmişi eklenmelidir.

## Ekip bağlantıları

Serhat yönetim ekranından KullaniciYonetimi/Index adresini açabilir. HesapYonetimi servisi gerçek yönetici kimliğini ve hedef sürümünü denetler. Ahmet'in bildirim bağlantısı kimliği doğrulanmış kullanıcıya ait olmalı, hedef kullanıcı ve birim kimliği sunucudaki kimlik bilgilerinden alınmalıdır. Rol veya birim değişince açık SignalR bağlantılarının yeniden yetkilendirilmesi bildirim modülünün kabul kontrolüdür.
