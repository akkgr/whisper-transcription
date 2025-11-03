# 🎙️ Whisper Audio Transcription System

Complete audio transcription system with automatic folder monitoring, language detection, and translation to English using OpenAI's Whisper model.

## 🌟 Features

- **🔍 Automatic Monitoring**: Drop audio files in a folder and get transcriptions automatically
- **🌐 Multi-language**: Detects source language and translates to English
- **🎵 Multi-format**: Supports MP3, WAV, M4A, FLAC, OGG, MP4, WebM
- **🖥️ Web UI**: Interactive Gradio interface for manual transcription
- **🔌 REST API**: Monitor status and control the system via API
- **🐳 Docker**: Fully containerized with Docker Compose
- **⚡ GPU Support**: Automatic GPU acceleration if available

## 📦 What's Included

1. **Whisper Service**: Python-based Gradio UI for manual transcription
2. **.NET Monitor API**: Automatic folder monitoring and batch processing
3. **Docker Setup**: Complete orchestration with Docker Compose

## 🚀 Quick Start

### 1. Start the System

```bash
./start.sh
```

Or manually:
```bash
docker-compose up -d --build
```

### 2. Use It

**Option A: Automatic Processing** (drop files)
```bash
# Copy audio files to input folder
cp your-audio.mp3 audio/input/

# Transcriptions appear automatically in audio/output/
cat audio/output/your-audio.txt
```

**Option B: Manual Processing** (web UI)
- Open http://localhost:7860
- Upload audio file
- Get instant transcription

### 3. Monitor Status

```bash
# Check API status
curl http://localhost:8080/status

# View logs
docker-compose logs -f

# Open Swagger UI
open http://localhost:8080/swagger
```

### 4. Stop the System

```bash
./stop.sh
```

Or manually:
```bash
docker-compose down
```

## 📁 Project Structure

```
whisper/
├── app.py                  # Whisper Gradio service
├── Dockerfile              # Whisper container
├── docker-compose.yml      # Complete orchestration
├── start.sh               # Quick start script
├── stop.sh                # Stop script
├── audio/
│   ├── input/             # Drop audio files here
│   └── output/            # Transcriptions appear here
└── WhisperApi/            # .NET Monitor API
    ├── Program.cs         # Main API logic
    ├── Dockerfile         # API container
    └── README.md          # API documentation
```

## 🔧 Services

### Whisper Service (Port 7860)
- **Purpose**: AI transcription engine with web UI
- **URL**: http://localhost:7860
- **Model**: OpenAI Whisper Large
- **Features**: Manual upload, real-time transcription

### Monitor API (Port 8080)
- **Purpose**: Automatic folder monitoring
- **URL**: http://localhost:8080
- **Swagger**: http://localhost:8080/swagger
- **Features**: Auto-processing, status monitoring, REST API

## 🎯 Use Cases

### 1. Batch Processing
Drop multiple audio files and let the system process them automatically:
```bash
cp *.mp3 audio/input/
# All files processed automatically
```

### 2. Manual Transcription
Use the web UI for quick one-off transcriptions:
- Visit http://localhost:7860
- Upload file
- Get instant result

### 3. Integration
Use the REST API to integrate with other systems:
```bash
# Check status
curl http://localhost:8080/status

# Start monitoring
curl -X POST http://localhost:8080/start

# Stop monitoring
curl -X POST http://localhost:8080/stop
```

## 📝 Output Format

Transcriptions include detected language and English translation:

```
**Detected Language:** Spanish

**Transcription (English):**
Hello, this is a test transcription of Spanish audio translated to English...
```

## ⚙️ Configuration

### Environment Variables

Edit `docker-compose.yml` or use environment variables:

```yaml
environment:
  - AudioMonitor__InputFolder=/audio/input
  - AudioMonitor__OutputFolder=/audio/output
  - AudioMonitor__WhisperUrl=http://whisper:7860/
```

### Whisper Model

To use a different model size, edit `app.py`:
```python
model = whisper.load_model("large")  # Options: tiny, base, small, medium, large
```

### Custom Folders

Change folder locations in `docker-compose.yml`:
```yaml
volumes:
  - /your/custom/input:/audio/input
  - /your/custom/output:/audio/output
```

## 🔍 Monitoring & Debugging

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f whisper
docker-compose logs -f whisper-api
```

### Check Container Status
```bash
docker-compose ps
```

### Check API Status
```bash
curl http://localhost:8080/status | jq
```

### Manual Test
```bash
# Test Whisper directly
curl -X POST http://localhost:7860/api/predict \
  -F "data=@test-audio.mp3"
```

## 🐛 Troubleshooting

### Files Not Processing?
1. Check logs: `docker-compose logs -f whisper-api`
2. Verify file format is supported
3. Check folder permissions
4. Ensure Whisper service is running: `curl http://localhost:7860`

### Slow Processing?
- Whisper Large is CPU-intensive
- Consider using smaller model (edit `app.py`)
- For GPU: ensure NVIDIA Docker runtime is installed

### API Not Responding?
```bash
# Restart services
docker-compose restart

# Check if ports are available
lsof -i :7860
lsof -i :8080
```

## 📊 Performance

| Model | Speed | Accuracy | RAM |
|-------|-------|----------|-----|
| tiny  | Fast  | Lower    | ~1GB |
| base  | Fast  | Good     | ~1GB |
| small | Medium| Good     | ~2GB |
| medium| Slow  | Better   | ~5GB |
| large | Slowest| Best   | ~10GB |

## 🔐 Security Notes

- API runs on port 8080 (HTTP only)
- No authentication by default
- For production: add API keys, HTTPS, rate limiting
- Consider network isolation in docker-compose

## 🚀 Advanced Usage

### Process & Archive
Modify `Program.cs` to move processed files:
```csharp
var processedFolder = Path.Combine(InputFolder, "processed");
Directory.CreateDirectory(processedFolder);
File.Move(filePath, Path.Combine(processedFolder, fileName));
```

### Custom Notifications
Add webhook or email notifications in `ProcessAudioFileAsync`.

### Multiple Whisper Instances
Scale Whisper horizontally for parallel processing:
```yaml
whisper:
  deploy:
    replicas: 3
```

## 📚 API Documentation

Full API documentation available at:
- Swagger UI: http://localhost:8080/swagger
- API README: [WhisperApi/README.md](WhisperApi/README.md)

## 🛠️ Development

### Local Development (without Docker)

**Whisper Service:**
```bash
pip install -r requirements.txt
python app.py
```

**Monitor API:**
```bash
cd WhisperApi
dotnet run
```

### Requirements
- Docker & Docker Compose (for containerized setup)
- Or: Python 3.10+, .NET 8 SDK (for local development)

## 📄 License

MIT

## 🙏 Credits

- OpenAI Whisper: https://github.com/openai/whisper
- Gradio: https://gradio.app
- .NET: https://dotnet.microsoft.com

## 🤝 Contributing

Contributions welcome! Areas for improvement:
- [ ] Authentication & authorization
- [ ] Retry logic for failed transcriptions
- [ ] Progress tracking per file
- [ ] Support for other transcription services
- [ ] Batch processing optimization
- [ ] Web dashboard for monitoring

## 📞 Support

For issues or questions:
1. Check logs: `docker-compose logs -f`
2. Review troubleshooting section
3. Check Whisper documentation
4. Open an issue on GitHub
