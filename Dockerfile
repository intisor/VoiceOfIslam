# Dockerfile for combined VoiceOfIslam.Api (minimal API) + Blazor WASM client
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore and build Blazor Client
COPY VoiceOfIslam.Client/VoiceOfIslam.Client.csproj VoiceOfIslam.Client/
COPY VoiceOfIslam.Shared/VoiceOfIslam.Shared.csproj VoiceOfIslam.Shared/
RUN dotnet restore VoiceOfIslam.Client/VoiceOfIslam.Client.csproj
COPY VoiceOfIslam.Client/ VoiceOfIslam.Client/
COPY VoiceOfIslam.Shared/ VoiceOfIslam.Shared/
RUN dotnet publish VoiceOfIslam.Client/VoiceOfIslam.Client.csproj -c Release -o /blazorout \
	&& echo "--- /blazorout contents after publish ---" \
	&& ls -l /blazorout \
	&& echo "--- /blazorout/wwwroot contents after publish ---" \
	&& ls -l /blazorout/wwwroot || echo "/blazorout/wwwroot missing"

# Restore and build API
COPY VoiceOfIslam.Api/VoiceOfIslam.Api.csproj VoiceOfIslam.Api/
RUN dotnet restore VoiceOfIslam.Api/VoiceOfIslam.Api.csproj
COPY VoiceOfIslam.Api/ VoiceOfIslam.Api/
RUN dotnet publish VoiceOfIslam.Api/VoiceOfIslam.Api.csproj -c Release -o /app --no-restore


# Ensure wwwroot exists and copy Blazor WASM output into API wwwroot
RUN mkdir -p /app/wwwroot \
	&& echo "--- /blazorout/wwwroot contents ---" \
	&& ls -l /blazorout/wwwroot \
	&& echo "--- /app/wwwroot before copy ---" \
	&& ls -l /app/wwwroot \
	&& cp -r /blazorout/wwwroot/* /app/wwwroot/ \
	&& echo "--- /app/wwwroot after copy ---" \
	&& ls -l /app/wwwroot \
	&& find /app/wwwroot -name "*.razor.js" -type f -delete \
	&& echo "--- /app/wwwroot after .razor.js cleanup ---" \
	&& ls -lR /app/wwwroot \
	&& echo "--- FINAL /app/wwwroot ---" \
	&& ls -l /app/wwwroot \
	&& ls -l /app/wwwroot/_framework \
	&& cat /app/wwwroot/index.html \
	&& echo "--- FULL RECURSIVE /app/wwwroot/_framework ---" \
	&& ls -lR /app/wwwroot/_framework
	&& echo "--- FULL RECURSIVE /app/wwwroot/_framework ---" \
	&& ls -lR /app/wwwroot/_framework

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "VoiceOfIslam.Api.dll"]
