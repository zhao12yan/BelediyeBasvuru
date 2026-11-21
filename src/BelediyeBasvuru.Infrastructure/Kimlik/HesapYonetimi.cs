using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json;
using BelediyeBasvuru.Core.Entities;
using BelediyeBasvuru.Core.Yetki;
using BelediyeBasvuru.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BelediyeBasvuru.Infrastructure.Kimlik;

public sealed class DenetimKaydi
{
    public long Id { get; set; }
    public DateTime ZamanUtc { get; set; }
    [MaxLength(450)] public required string YapanId { get; set; }
    [MaxLength(450)] public required string HedefId { get; set; }
    [MaxLength(45)] public string? Ip { get; set; }
    [MaxLength(80)] public required string Islem { get; set; }
    public required string EskiDeger { get; set; }
    public required string YeniDeger { get; set; }
}

public sealed class HesapEtkinlikKontrolu : IUserConfirmation<Kullanici>
{
    public Task<bool> IsConfirmedAsync(UserManager<Kullanici> manager, Kullanici user)
        => Task.FromResult(!user.PasifMi);
}

public sealed record HesapYetkisi(string Rol, int? BirimId, bool PasifMi);
public sealed record HesapGorunumu(string Id, string Eposta, string Surum, HesapYetkisi Yetki);

public sealed class HesapYonetimi(AppDbContext db, UserManager<Kullanici> kimlik, IConfiguration ayarlar)
{
    public int[] Birimler => ayarlar.GetSection("Yonetim:Birimler").Get<int[]>() ?? [];

    public async Task<HesapGorunumu?> BulAsync(string eposta)
    {
        var hesap = await kimlik.FindByEmailAsync(eposta.Trim());
        if (hesap is null) return null;
        var roller = await kimlik.GetRolesAsync(hesap);
        return new(hesap.Id, hesap.Email!, hesap.ConcurrencyStamp!,
            new(string.Join(", ", roller.Order()), hesap.BirimId, hesap.PasifMi));
    }

    public async Task<IdentityResult> DegistirAsync(string yapanId, string hedefId, string surum, HesapYetkisi yeni, string? ip)
    {
        if (!Roller.Tumu.Contains(yeni.Rol) ||
            (yeni.Rol == Roller.BirimPersoneli ? !Birimler.Contains(yeni.BirimId ?? -1) : yeni.BirimId.HasValue))
            return Hata("Rol veya birim geçersiz.");

        await using var islem = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var yapan = await kimlik.FindByIdAsync(yapanId);
        if (yapan is null || yapan.PasifMi || !await kimlik.IsInRoleAsync(yapan, Roller.Yonetici) || yapanId == hedefId)
            return Hata("Bu hesap üzerinde işlem yetkiniz yok.");

        var hedef = await kimlik.FindByIdAsync(hedefId);
        if (hedef is null || hedef.ConcurrencyStamp != surum)
            return Hata("Hesap değişmiş olabilir. Hesabı yeniden arayın.");

        var roller = await kimlik.GetRolesAsync(hedef);
        var eski = new HesapYetkisi(string.Join(", ", roller.Order()), hedef.BirimId, hedef.PasifMi);
        if (eski == yeni) return IdentityResult.Success;

        var kaldirilacak = roller.Where(r => r != yeni.Rol).ToList();
        if (kaldirilacak.Count > 0)
        {
            var sonuc = await kimlik.RemoveFromRolesAsync(hedef, kaldirilacak);
            if (!sonuc.Succeeded) return sonuc;
        }
        if (!roller.Contains(yeni.Rol))
        {
            var sonuc = await kimlik.AddToRoleAsync(hedef, yeni.Rol);
            if (!sonuc.Succeeded) return sonuc;
        }

        hedef.BirimId = yeni.BirimId;
        hedef.PasifMi = yeni.PasifMi;
        var guncelleme = await kimlik.UpdateSecurityStampAsync(hedef);
        if (!guncelleme.Succeeded) return guncelleme;

        db.Set<DenetimKaydi>().Add(new()
        {
            ZamanUtc = DateTime.UtcNow, Islem = "HesapYetkisi", YapanId = yapanId, HedefId = hedefId, Ip = ip,
            EskiDeger = JsonSerializer.Serialize(eski), YeniDeger = JsonSerializer.Serialize(yeni)
        });
        await db.SaveChangesAsync();
        await islem.CommitAsync();
        return IdentityResult.Success;
    }

    private static IdentityResult Hata(string mesaj) => IdentityResult.Failed(new IdentityError { Description = mesaj });
}
