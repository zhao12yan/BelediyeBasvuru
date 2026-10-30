// Kullanici sınıfı Core projesinde; onu kullanmak için içe alıyoruz.
using BelediyeBasvuru.Core.Entities;
// IdentityDbContext (kullanıcı/rol tablolarını hazır veren DbContext) bu kütüphanede.
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// DbContext, DbSet, ModelBuilder gibi EF Core sınıfları.
using Microsoft.EntityFrameworkCore;

// Veritabanı ile ilgili sınıflar bu ad alanında durur.
namespace BelediyeBasvuru.Infrastructure.Data;

/// <summary>
/// Uygulamanın tek veritabanı bağlamı. Identity tabloları (AspNetUsers, AspNetRoles ...)
/// IdentityDbContext'ten gelir; böylece InitialCreate migration'ı baştan Identity'yi içerir.
/// </summary>
// "(DbContextOptions<AppDbContext> options)" → birincil kurucu (primary constructor):
//   bağlantı ayarları (hangi veritabanı, hangi sunucu) dışarıdan, DependencyInjection.cs'ten gelir.
// ": IdentityDbContext<Kullanici>(options)" → kullanıcı tipi olarak bizim Kullanici sınıfımızı kullan.
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<Kullanici>(options)
{
    // Kişi 4 / Kişi 3 (O-07): varlıklar eklendikçe DbSet'ler buraya yazılır.
    // Her DbSet veritabanında bir tabloya karşılık gelir. Örnek:
    // public DbSet<Basvuru> Basvurular => Set<Basvuru>();

    // EF Core modeli (tablolar, sütunlar, ilişkiler) oluştururken bu metodu çağırır.
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Önce Identity'nin kendi tablolarını tanımlaması için temel sınıfı çağırıyoruz. SİLMEYİN.
        base.OnModelCreating(builder);

        // Tüm IEntityTypeConfiguration<T> sınıflarını (Data/Configurations) otomatik uygular.
        // Yani her tablonun ayarını ayrı dosyaya yazarsınız, burayı değiştirmeniz gerekmez.
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
