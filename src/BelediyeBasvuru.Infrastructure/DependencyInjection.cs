
using BelediyeBasvuru.Core.Interfaces;
using BelediyeBasvuru.Infrastructure.Data;
using BelediyeBasvuru.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BelediyeBasvuru.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "'ConnectionStrings:Default' bağlantı dizesi bulunamadı.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IIstatistikServisi, SahteIstatistikServisi>();

        return services;
    }
}
