-- Yalnız CI'ın geçici Compose SQL Server'ında, uygulama migration ve seed adımları tamamlanınca çalışır.
USE master;
SET NOCOUNT ON;
IF DB_ID(N'BelediyeBasvuru') IS NULL THROW 51000, 'Prova kaynagi bulunamadi.', 1;
IF (SELECT COUNT(*) FROM BelediyeBasvuru.dbo.AspNetRoles) <> 3 THROW 51001, 'Rol seed kontrolu basarisiz.', 1;
IF (SELECT COUNT(*) FROM BelediyeBasvuru.dbo.AspNetUsers WHERE Email IN
    ('yonetici@ornek.test','personel@ornek.test','vatandas@ornek.test')) <> 3 THROW 51002, 'Ornek hesaplar eksik.', 1;

DECLARE @etiket varchar(32) = REPLACE(CONVERT(varchar(36), NEWID()), '-', '');
DECLARE @kopya sysname = N'Prova_' + @etiket;
DECLARE @yedek nvarchar(260) = N'/var/opt/mssql/data/' + @etiket + N'.bak';
BACKUP DATABASE BelediyeBasvuru TO DISK = @yedek WITH COPY_ONLY, CHECKSUM;
RESTORE VERIFYONLY FROM DISK = @yedek WITH CHECKSUM;

DECLARE @dosyalar nvarchar(max);
SELECT @dosyalar = STRING_AGG(CAST(N'MOVE ' + QUOTENAME(name, '''') + N' TO ' +
    QUOTENAME(N'/var/opt/mssql/data/' + @etiket + N'_' + CONVERT(varchar(10),file_id) +
    CASE type WHEN 1 THEN N'.ldf' ELSE N'.mdf' END, '''') AS nvarchar(max)), N', ')
FROM sys.master_files WHERE database_id = DB_ID(N'BelediyeBasvuru');
DECLARE @sql nvarchar(max) = N'RESTORE DATABASE ' + QUOTENAME(@kopya) +
    N' FROM DISK = @b WITH CHECKSUM, ' + @dosyalar;
EXEC sp_executesql @sql, N'@b nvarchar(260)', @b = @yedek;
SET @sql = N'USE ' + QUOTENAME(@kopya) + N';
    IF OBJECT_ID(N''dbo.DenetimKayitlari'') IS NULL THROW 51003, ''Denetim tablosu eksik.'', 1;
    IF (SELECT COUNT(*) FROM dbo.AspNetUserRoles) <> 3 THROW 51004, ''Hesap rolleri eksik.'', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId LIKE ''%HesapYonetimi'')
       THROW 51005, ''Migration eksik.'', 1;';
EXEC sp_executesql @sql;
PRINT N'Ayrı veritabanına geri yükleme, roller ve migration doğrulandı.';
-- Geçici CI konteyneri ve verisi iş bitiminde Compose tarafından kaldırılır.
