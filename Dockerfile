# Dockerfile for combined VoiceOfIslam.Api (minimal API) + Blazor WASM client
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore and build Blazor Client
COPY VoiceOfIslam.Client/VoiceOfIslam.Client.csproj VoiceOfIslam.Client/
COPY VoiceOfIslam.Shared/VoiceOfIslam.Shared.csproj VoiceOfIslam.Shared/
RUN dotnet restore VoiceOfIslam.Client/VoiceOfIslam.Client.csproj
COPY VoiceOfIslam.Client/ VoiceOfIslam.Client/
COPY VoiceOfIslam.Shared/ VoiceOfIslam.Shared/
RUN dotnet publish VoiceOfIslam.Client/VoiceOfIslam.Client.csproj -c Release -o /blazorout

# Restore and build API
COPY VoiceOfIslam.Api/VoiceOfIslam.Api.csproj VoiceOfIslam.Api/
RUN dotnet restore VoiceOfIslam.Api/VoiceOfIslam.Api.csproj
COPY VoiceOfIslam.Api/ VoiceOfIslam.Api/
RUN dotnet publish VoiceOfIslam.Api/VoiceOfIslam.Api.csproj -c Release -o /app --no-restore

# Copy Blazor WASM output into API wwwroot
RUN rm -rf /app/wwwroot/* && cp -r /blazorout/wwwroot/* /app/wwwroot/

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "VoiceOfIslam.Api.dll"]
