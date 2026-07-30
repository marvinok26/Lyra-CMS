FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Lyra.Cms.Web/Lyra.Cms.Web.csproj Lyra.Cms.Web/
RUN dotnet restore Lyra.Cms.Web/Lyra.Cms.Web.csproj

COPY src/Lyra.Cms.Web Lyra.Cms.Web
RUN dotnet publish Lyra.Cms.Web/Lyra.Cms.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Unlike the other sibling projects, this container writes tenant configs/media into a mounted
# named volume (App_Data) at runtime, not just to external services (SQL Server). Docker creates
# fresh named volumes as root-owned, so the non-root $APP_UID user (the pattern used elsewhere)
# can't write to it without extra entrypoint chown logic — running as root here is the simpler,
# accepted tradeoff for this project rather than adding that complexity.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Lyra.Cms.Web.dll"]
