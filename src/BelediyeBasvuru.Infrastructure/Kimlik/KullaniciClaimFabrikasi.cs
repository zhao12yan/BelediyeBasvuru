using System.Globalization;
using System.Security.Claims;
using BelediyeBasvuru.Core.Entities;
using BelediyeBasvuru.Core.Yetki;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BelediyeBasvuru.Infrastructure.Kimlik;

/// <summary>
/// H2-K1: Giriş yapan kullanıcının çerezine birim bilgisini (birim_id) ekler.
/// Böylece "personel yalnızca kendi birimi" kuralı her istekte veritabanına gitmeden kontrol edilir.
/// </summary>
/// <remarks>
/// Birim değişirse kullanıcının yeniden giriş yapması (veya güvenlik damgasının yenilenmesi) gerekir.
/// </remarks>
public class KullaniciClaimFabrikasi(
    UserManager<Kullanici> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<Kullanici, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Kullanici user)
    {
        var kimlik = await base.GenerateClaimsAsync(user);
        if (user.BirimId is int birimId)
        {
            kimlik.AddClaim(new Claim(OzelClaimler.BirimId, birimId.ToString(CultureInfo.InvariantCulture)));
        }

        return kimlik;
    }
}
