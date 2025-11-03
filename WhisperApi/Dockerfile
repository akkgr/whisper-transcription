FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Install Python and pip in base image
RUN apt-get update && apt-get install -y python3 python3-pip && rm -rf /var/lib/apt/lists/*
RUN pip3 install --break-system-packages gradio_client

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WhisperApi.csproj", "./"]
RUN dotnet restore "WhisperApi.csproj"
COPY . .
RUN dotnet build "WhisperApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WhisperApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY transcribe.py /app/transcribe.py
RUN chmod +x /app/transcribe.py
ENTRYPOINT ["dotnet", "WhisperApi.dll"]
