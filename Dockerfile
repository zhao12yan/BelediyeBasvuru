# Dockerfile — web uygulamasının Docker imajını tarif eder.
# Doğrudan kullanmanız gerekmez: "docker compose up --build" bu dosyayı kendisi kullanır.
# İki aşamalı (multi-stage): 1) SDK ile derle  2) sadece çalışma zamanı ile çalıştır → küçük imaj.

# ===== 1. AŞAMA: Derleme (içinde .NET 10 SDK var; bilgisayarınıza SDK kurmanız gerekmez) =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS derle
# Konteyner içindeki çalışma klasörü.
WORKDIR /kaynak

# Önce SADECE proje dosyalarını kopyalayıp paketleri indiriyoruz.
# Neden? Kod değişse bile .csproj değişmediyse Docker bu adımı önbellekten alır → derleme hızlanır.
COPY global.json Directory.Build.props ./
COPY src/BelediyeBasvuru.Core/BelediyeBasvuru.Core.csproj src/BelediyeBasvuru.Core/
COPY src/BelediyeBasvuru.Infrastructure/BelediyeBasvuru.Infrastructure.csproj src/BelediyeBasvuru.Infrastructure/
COPY src/BelediyeBasvuru.Web/BelediyeBasvuru.Web.csproj src/BelediyeBasvuru.Web/
# NuGet paketlerini indir (EF Core, Identity ...).
RUN dotnet restore src/BelediyeBasvuru.Web/BelediyeBasvuru.Web.csproj

# Şimdi kodun geri kalanını kopyala (tests/ ve docs/ imaja girmez; bkz. .dockerignore).
COPY src/ src/
# Release modunda derle ve yayın dosyalarını /yayin klasörüne çıkar.
RUN dotnet publish src/BelediyeBasvuru.Web/BelediyeBasvuru.Web.csproj -c Release -o /yayin --no-restore

# ===== 2. AŞAMA: Çalıştırma (sadece ASP.NET Core çalışma zamanı; SDK yok → imaj küçük) =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS calistir
WORKDIR /app
# 1. aşamada üretilen derlenmiş dosyaları buraya al.
COPY --from=derle /yayin .
# Uygulama konteyner içinde 8080 portunu dinler.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
# Güvenlik: uygulamayı root yerine imajın hazır "app" kullanıcısıyla çalıştır.
USER $APP_UID
# Konteyner açılınca çalışacak komut.
ENTRYPOINT ["dotnet", "BelediyeBasvuru.Web.dll"]
