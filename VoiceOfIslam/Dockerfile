FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and restore as distinct layers
COPY VoiceOfIslam.slnx ./
COPY VoiceOfIslam/VoiceOfIslam.csproj VoiceOfIslam/
COPY VoiceOfIslam.Shared/VoiceOfIslam.Shared.csproj VoiceOfIslam.Shared/
COPY VoiceOfIslam.Client/VoiceOfIslam.Client.csproj VoiceOfIslam.Client/
RUN dotnet restore VoiceOfIslam/VoiceOfIslam.csproj

# Copy everything else and build
COPY VoiceOfIslam/ VoiceOfIslam/
COPY VoiceOfIslam.Shared/ VoiceOfIslam.Shared/
COPY VoiceOfIslam.Client/ VoiceOfIslam.Client/
WORKDIR /src/VoiceOfIslam
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

# Expose port 8080 for Render/Fly.io
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Set environment to Production by default
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "VoiceOfIslam.dll"]
