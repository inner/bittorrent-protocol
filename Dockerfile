FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY codecrafters-bittorrent.csproj ./
RUN dotnet restore codecrafters-bittorrent.csproj

COPY . .
RUN dotnet publish codecrafters-bittorrent.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0

WORKDIR /workspace

COPY --from=build /app/publish /app

ENTRYPOINT ["dotnet", "/app/codecrafters-bittorrent.dll"]
