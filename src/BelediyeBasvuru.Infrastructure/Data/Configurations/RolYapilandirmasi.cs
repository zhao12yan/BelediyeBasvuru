using BelediyeBasvuru.Core.Yetki;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BelediyeBasvuru.Infrastructure.Data.Configurations;

/// <summary>
/// H2-K1: Üç rol (Vatandas, BirimPersoneli, Yonetici) MIGRATION ile veritabanına girer.
/// Böylece "dotnet ef database update" biter bitmez roller hazırdır; uygulama açılışında
/// rol oluşturma kodu çalışmaz, kayıt sırasında da rol yaratılmaz.
/// </summary>
/// <remarks>
/// Id ve ConcurrencyStamp SABİT olmalı; değişirse EF her seferinde yeni migration üretir.
/// </remarks>
public class RolYapilandirmasi : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            Rol("6f1c2a52-5b0e-4f4e-9a0e-0c5a1d6b7e01", Roller.Vatandas),
            Rol("6f1c2a52-5b0e-4f4e-9a0e-0c5a1d6b7e02", Roller.BirimPersoneli),
            Rol("6f1c2a52-5b0e-4f4e-9a0e-0c5a1d6b7e03", Roller.Yonetici));
    }

    private static IdentityRole Rol(string id, string ad) => new()
    {
        Id = id,
        Name = ad,
        NormalizedName = ad.ToUpperInvariant(),
        ConcurrencyStamp = id
    };
}
