using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BelediyeBasvuru.Tests;

/// <summary>
/// İskeletin ayağa kalktığını doğrulayan duman (smoke) testi.
/// Ana sayfa veritabanına dokunmaz; bu yüzden her ortamda çalışır.
/// </summary>
public class IskeletTestleri(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task AnaSayfa_200_Doner()
    {
        var client = factory.CreateClient();

        var yanit = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
    }
}
