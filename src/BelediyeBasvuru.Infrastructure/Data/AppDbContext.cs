
using BelediyeBasvuru.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BelediyeBasvuru.Infrastructure.Data;

/// <summary>
/// Uygulamanın veritabanı bağlantı sınıfı.
/// Identity kullanıcı tablolarını ve başvuru tablolarını yönetir.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<Kullanici>(options)
{
    // Belediye başvurularını tutan veritabanı tablosu.
    public DbSet<Basvuru> Basvurular => Set<Basvuru>();

    // Veritabanı tablolarının ve ilişkilerinin yapılandırılması.
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity tablolarının varsayılan ayarlarını korur.
        base.OnModelCreating(builder);

        // Configuration klasöründeki ayarları otomatik uygular.
        builder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly
        );
    }
}
