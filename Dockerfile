# Dockerfile for VoiceOfIslam (.NET 10, Blazor Server)
# Build stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["VoiceOfIslam/VoiceOfIslam.csproj", "VoiceOfIslam/"]
COPY ["VoiceOfIslam.Shared/VoiceOfIslam.Shared.csproj", "VoiceOfIslam.Shared/"]
COPY ["VoiceOfIslam.Client/VoiceOfIslam.Client.csproj", "VoiceOfIslam.Client/"]
RUN dotnet restore "VoiceOfIslam/VoiceOfIslam.csproj"
COPY . .
WORKDIR "/src/VoiceOfIslam"
RUN dotnet build "VoiceOfIslam.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VoiceOfIslam.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VoiceOfIslam.dll"]
