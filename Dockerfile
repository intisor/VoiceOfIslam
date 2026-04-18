# Dockerfile for VoiceOfIslam (.NET 10, Blazor Server)
# Build stage

# Dockerfile for VoiceOfIslam Server Only (.NET 10, Blazor Server)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the server and shared projects
COPY VoiceOfIslam/VoiceOfIslam.csproj VoiceOfIslam/
COPY VoiceOfIslam.Shared/VoiceOfIslam.Shared.csproj VoiceOfIslam.Shared/

# Restore dependencies
RUN dotnet restore VoiceOfIslam/VoiceOfIslam.csproj

# Copy the rest of the server and shared code
COPY VoiceOfIslam/ VoiceOfIslam/
COPY VoiceOfIslam.Shared/ VoiceOfIslam.Shared/

WORKDIR /src/VoiceOfIslam
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "VoiceOfIslam.dll"]
