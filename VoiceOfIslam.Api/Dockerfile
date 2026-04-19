# Dockerfile for VoiceOfIslam Minimal API (.NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY VoiceOfIslam.Api/VoiceOfIslam.Api.csproj VoiceOfIslam.Api/
COPY VoiceOfIslam.Shared/VoiceOfIslam.Shared.csproj VoiceOfIslam.Shared/
RUN dotnet restore VoiceOfIslam.Api/VoiceOfIslam.Api.csproj

# Copy everything else and build
COPY VoiceOfIslam.Api/ VoiceOfIslam.Api/
COPY VoiceOfIslam.Shared/ VoiceOfIslam.Shared/
WORKDIR /src/VoiceOfIslam.Api
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "VoiceOfIslam.Api.dll"]
