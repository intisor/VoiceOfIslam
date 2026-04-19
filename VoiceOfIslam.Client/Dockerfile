# Dockerfile for VoiceOfIslam.Client (Blazor WebAssembly, static site)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY VoiceOfIslam.Client/VoiceOfIslam.Client.csproj VoiceOfIslam.Client/
COPY VoiceOfIslam.Shared/VoiceOfIslam.Shared.csproj VoiceOfIslam.Shared/
RUN dotnet restore VoiceOfIslam.Client/VoiceOfIslam.Client.csproj

COPY VoiceOfIslam.Client/ VoiceOfIslam.Client/
COPY VoiceOfIslam.Shared/ VoiceOfIslam.Shared/
WORKDIR /src/VoiceOfIslam.Client
RUN dotnet publish -c Release -o /app --no-restore

# Serve with nginx (or static file server)
FROM nginx:alpine AS final
COPY --from=build /app/wwwroot /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
