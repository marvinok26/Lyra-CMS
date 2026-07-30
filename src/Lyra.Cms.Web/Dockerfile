FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the whole src tree up front rather than enumerating each project's .csproj individually —
# Lyra.Cms.Web references Modules/* and Themes/* as they're added, so a per-project COPY list
# would need updating every time a new module/theme project is created. .dockerignore already
# excludes bin/obj/node_modules, so this stays a cheap, correct layer.
COPY src src
# Orchard Core's dependency tree is large (20+ min cold restore) — a BuildKit cache mount keeps
# downloaded packages across builds regardless of source-layer invalidation, so a one-line code
# change doesn't force a full re-download on every rebuild during later module/theme development.
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore src/Lyra.Cms.Web/Lyra.Cms.Web.csproj
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish src/Lyra.Cms.Web/Lyra.Cms.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# publish doesn't carry over an empty wwwroot directory, but Orchard Core's media-caching
# middleware (ImageSharp.Web) requires WebRootPath to physically exist even if nothing has been
# uploaded yet — without it every tenant request 500s with "cache root path cannot be determined".
RUN mkdir -p /app/wwwroot

# Unlike the other sibling projects, this container writes tenant configs/media into a mounted
# named volume (App_Data) at runtime, not just to external services (SQL Server). Docker creates
# fresh named volumes as root-owned, so the non-root $APP_UID user (the pattern used elsewhere)
# can't write to it without extra entrypoint chown logic — running as root here is the simpler,
# accepted tradeoff for this project rather than adding that complexity.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Lyra.Cms.Web.dll"]
