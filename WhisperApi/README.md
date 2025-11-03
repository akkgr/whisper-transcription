# Whisper Audio Transcription Monitor API

A .NET 8 minimal API that automatically monitors a folder for new audio files, sends them to a Whisper transcription service, and saves the transcriptions.

## Features

- 🔍 **Automatic Monitoring**: Watches a folder for new audio files
- 🎵 **Multi-format Support**: Supports MP3, WAV, M4A, FLAC, OGG, MP4, WebM
- 🌐 **Language Detection**: Shows detected language in transcription
- 📝 **Auto-transcription**: Automatically processes and translates to English
- 🔄 **Real-time Processing**: Processes files as soon as they're added
- 📊 **Status API**: Monitor processing status via REST endpoints
- 🐳 **Docker Ready**: Fully containerized with Docker Compose

## Architecture

```
┌─────────────────┐
│  Input Folder   │
│  (Audio Files)  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐      ┌──────────────┐
│  Whisper API    │─────▶│   Whisper    │
│  (.NET Monitor) │◀─────│   Service    │
└────────┬────────┘      └──────────────┘
         │
         ▼
┌─────────────────┐
│  Output Folder  │
│  (Transcripts)  │
└─────────────────┘
```

## Quick Start

### Using Docker Compose (Recommended)

1. **Start both services**:
   ```bash
   docker-compose up -d --build
   ```

2. **Drop audio files** into `./audio/input/`

3. **Get transcriptions** from `./audio/output/`

### Local Development

1. **Start Whisper service**:
   ```bash
   docker-compose up whisper -d
   ```

2. **Run the API**:
   ```bash
   cd WhisperApi
   dotnet run
   ```

3. **Create folders**:
   ```bash
   mkdir -p audio/input audio/output
   ```

## API Endpoints

### Get Status
```bash
GET http://localhost:8080/status
```

Response:
```json
{
  "isRunning": true,
  "processedFiles": 5,
  "inputFolder": "/audio/input",
  "outputFolder": "/audio/output",
  "whisperUrl": "http://whisper:7860/"
}
```

### Start Monitor
```bash
POST http://localhost:8080/start
```

### Stop Monitor
```bash
POST http://localhost:8080/stop
```

### Swagger UI
```
http://localhost:8080/swagger
```

## Configuration

### appsettings.json

```json
{
  "AudioMonitor": {
    "InputFolder": "/audio/input",
    "OutputFolder": "/audio/output",
    "WhisperUrl": "http://whisper:7860/"
  }
}
```

### Environment Variables

You can override settings using environment variables:

```bash
AudioMonitor__InputFolder=/custom/input
AudioMonitor__OutputFolder=/custom/output
AudioMonitor__WhisperUrl=http://localhost:7860/
```

## Usage Examples

### Add audio file for transcription
```bash
# Copy a file to the input folder
cp my-audio.mp3 audio/input/

# The API automatically:
# 1. Detects the new file
# 2. Sends it to Whisper
# 3. Saves transcription to audio/output/my-audio.txt
```

### Check status
```bash
curl http://localhost:8080/status
```

### View transcription
```bash
cat audio/output/my-audio.txt
```

Output example:
```
**Detected Language:** Spanish

**Transcription (English):**
Hello, this is a test transcription...
```

## Supported Audio Formats

- MP3 (`.mp3`)
- WAV (`.wav`)
- M4A (`.m4a`)
- FLAC (`.flac`)
- OGG (`.ogg`)
- MP4 (`.mp4`)
- WebM (`.webm`)

## How It Works

1. **File Detection**: FileSystemWatcher monitors the input folder for new files
2. **Validation**: Checks if the file is a supported audio format
3. **Wait**: Ensures file is fully written before processing
4. **Transcription**: Sends file to Whisper service via HTTP
5. **Save**: Writes transcription result to output folder with same name + `.txt`
6. **Error Handling**: Saves errors to `{filename}_error.txt` if processing fails

## Docker Compose Services

### whisper
- Port: 7860
- The Gradio-based Whisper transcription service
- Handles actual AI transcription

### whisper-api
- Port: 8080
- .NET minimal API
- Monitors folders and orchestrates transcription

## Troubleshooting

### Check logs
```bash
# All services
docker-compose logs -f

# Just the API
docker-compose logs -f whisper-api

# Just Whisper
docker-compose logs -f whisper
```

### Check status
```bash
curl http://localhost:8080/status
```

### Restart services
```bash
docker-compose restart
```

### Manual file processing
If automatic processing doesn't work, check:
1. File permissions on input/output folders
2. Audio file format is supported
3. Whisper service is running: `docker-compose ps`
4. Check logs for errors

## Advanced Configuration

### Process existing files on startup
The monitor automatically processes all existing audio files in the input folder when it starts.

### Custom processing (optional)
You can modify `Program.cs` to:
- Move processed files to a "processed" subfolder
- Delete original audio files after transcription
- Add retry logic
- Send notifications

Example in `ProcessAudioFileAsync`:
```csharp
// Move to processed folder
var processedFolder = Path.Combine(InputFolder, "processed");
Directory.CreateDirectory(processedFolder);
File.Move(filePath, Path.Combine(processedFolder, fileName));

// Or delete
File.Delete(filePath);
```

## Requirements

- Docker & Docker Compose
- Or: .NET 8 SDK (for local development)

## License

MIT
