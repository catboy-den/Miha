FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MidnightHaven.Background/MidnightHaven.Background.csproj", "MidnightHaven.Background/"]
COPY ["MidnightHaven.Chan/MidnightHaven.Chan.csproj", "MidnightHaven.Chan/"]
COPY ["MidnightHaven.Discord/MidnightHaven.Discord.csproj", "MidnightHaven.Discord/"]
COPY ["MidnightHaven.Logic/MidnightHaven.Logic.csproj", "MidnightHaven.Logic/"]
COPY ["MidnightHaven.Redis/MidnightHaven.Redis.csproj", "MidnightHaven.Redis/"]
COPY ["MidnightHaven.Shared/MidnightHaven.Shared.csproj", "MidnightHaven.Shared/"]
RUN dotnet restore "MidnightHaven.Chan/MidnightHaven.Chan.csproj"
COPY . .
WORKDIR "/src/MidnightHaven.Chan"
RUN dotnet build "MidnightHaven.Chan.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MidnightHaven.Chan.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MidnightHaven.Chan.dll"]
