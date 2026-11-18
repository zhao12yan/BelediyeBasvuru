using BelediyeBasvuru.Core.Yetki;
using Microsoft.AspNetCore.Authorization;

namespace BelediyeBasvuru.Web.Guvenlik;

/// <summary>H3-K1: "Bu kayda erişebilir mi?" gereksinimi (kaynak tabanlı yetki).</summary>
public sealed class KayitErisimGereksinimi : IAuthorizationRequirement;

/// <summary>
/// H3-K1: <see cref="Politikalar.KayitErisimi"/> policy'sinin işleyicisi; kuralın kendisi
/// <see cref="ErisimKurallari.ErisebilirMi"/> içindedir (servisler de aynısını kullanır).
/// </summary>
/// <example>
/// var basvuru = await db.Basvurular.FindAsync(id);
/// if (basvuru is null) return NotFound();
/// if (!(await yetki.AuthorizeAsync(User, basvuru, Politikalar.KayitErisimi)).Succeeded) return Forbid();
/// </example>
public sealed class KayitErisimIsleyicisi : AuthorizationHandler<KayitErisimGereksinimi, IYetkiliKayit>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        KayitErisimGereksinimi requirement, IYetkiliKayit resource)
    {
        if (ErisimKurallari.ErisebilirMi(context.User, resource))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
