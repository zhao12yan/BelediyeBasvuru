using BelediyeBasvuru.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BelediyeBasvuru.Infrastructure.Data;

/// <summary>
/// Uygulamanın tek veritabanı bağlamı. Identity tabloları (AspNetUsers, AspNetRoles ...)
/// IdentityDbContext'ten gelir; böylece InitialCreate migration'ı baştan Identity'yi içerir.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<Kullanici>(options)
{
    // Kişi 4 / Kişi 3 (O-07): varlıklar eklendikçe DbSet'ler buraya yazılır.
    // public DbSet<Basvuru> Basvurular => Set<Basvuru>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tüm IEntityTypeConfiguration<T> sınıflarını (Data/Configurations) otomatik uygular.
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
