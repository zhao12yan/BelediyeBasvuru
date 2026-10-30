# Migrations

Migration ekleme (çözüm kök klasöründen):

```bash
dotnet tool restore
dotnet ef migrations add InitialCreate -p src/BelediyeBasvuru.Infrastructure -s src/BelediyeBasvuru.Web -o Data/Migrations
dotnet ef database update -p src/BelediyeBasvuru.Infrastructure -s src/BelediyeBasvuru.Web
```

**Kural:** Migration eklemeden önce `main` çekilir; aynı anda yalnızca bir kişi migration ekler ve gruba duyurur.
