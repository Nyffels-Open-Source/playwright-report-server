ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS restore
WORKDIR /src

COPY src/PlaywrightReportServer.sln ./src/
COPY src/PlaywrightReportServer.Api/PlaywrightReportServer.Api.csproj ./src/PlaywrightReportServer.Api/
COPY src/PlaywrightReportServer.Application/PlaywrightReportServer.Application.csproj ./src/PlaywrightReportServer.Application/
COPY src/PlaywrightReportServer.Infrastructure/PlaywrightReportServer.Infrastructure.csproj ./src/PlaywrightReportServer.Infrastructure/
COPY src/PlaywrightReportServer.Domain/PlaywrightReportServer.Domain.csproj ./src/PlaywrightReportServer.Domain/

RUN dotnet restore ./src/PlaywrightReportServer.sln

FROM restore AS publish
COPY src ./src
RUN dotnet publish ./src/PlaywrightReportServer.Api/PlaywrightReportServer.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /data/reports /data/uploads && chown -R "$APP_UID" /data

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
VOLUME ["/data"]

USER $APP_UID
ENTRYPOINT ["dotnet", "PlaywrightReportServer.Api.dll"]
