
using BelediyeBasvuru.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BelediyeBasvuru.Infrastructure.Data;
using BelediyeBasvuru.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddIdentity<Kullanici, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.User.RequireUniqueEmail = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// Roller ve geçici şifre sıfırlama işlemi
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<UserManager<Kullanici>>();

    string[] roller =
    {
        "Vatandas",
        "BirimPersoneli",
        "Yonetici"
    };

    foreach (var rol in roller)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    // Mevcut kullanıcının şifresini sıfırla
    string email = "yddemir518@gmail.com";
    string yeniSifre = "Belediye123";

    var kullanici = await userManager.FindByEmailAsync(email);

    if (kullanici != null)
    {
        var token =
            await userManager.GeneratePasswordResetTokenAsync(kullanici);

        var sonuc =
            await userManager.ResetPasswordAsync(
                kullanici,
                token,
                yeniSifre);

        if (sonuc.Succeeded)
        {
            Console.WriteLine("SIFRE BASARIYLA DEGISTIRILDI!");

            if (!await userManager.IsInRoleAsync(kullanici, "Vatandas"))
            {
                await userManager.AddToRoleAsync(kullanici, "Vatandas");
            }
        }
        else
        {
            foreach (var hata in sonuc.Errors)
            {
                Console.WriteLine(hata.Description);
            }
        }
    }
    else
    {
        Console.WriteLine("KULLANICI BULUNAMADI!");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program;
