FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR "/src"
COPY ["src/MidnightHaven.Chan/MidnightHaven.Chan.csproj", "MidnightHaven.Chan/"]
COPY ["src/MidnightHaven.Discord/MidnightHaven.Discord.csproj", "MidnightHaven.Discord/"]
COPY ["src/MidnightHaven.Logic/MidnightHaven.Logic.csproj", "MidnightHaven.Logic/"]
COPY ["src/MidnightHaven.Redis/MidnightHaven.Redis.csproj", "MidnightHaven.Redis/"]
COPY ["src/MidnightHaven.Shared/MidnightHaven.Shared.csproj", "MidnightHaven.Shared/"]
COPY . .
WORKDIR "src/MidnightHaven.Chan/"
RUN dotnet restore "MidnightHaven.Chan.csproj"
RUN dotnet build "MidnightHaven.Chan.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MidnightHaven.Chan.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MidnightHaven.Chan.dll"]
