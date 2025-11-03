# 🐍 Python Whisper Transcription Service

A pure Python service that automatically monitors a folder for audio files, transcribes them using OpenAI Whisper, detects the source language, and translates to English.

## ✨ Features

- 🔍 **Automatic folder monitoring** using `watchdog`
- 🌐 **Language detection** - automatically detects source language
- 📝 **Translation** - translates all audio to English
- 🎵 **Multi-format support** - MP3, WAV, M4A, FLAC, OGG, MP4, WebM, etc.
- 📊 **Detailed logging** - see exactly what's happening
- 🔄 **Auto-processing** - processes existing files on startup
- 📁 **Organized output** - moves processed files to subfolder
- 🐳 **Docker ready** - fully containerized

## 🚀 Quick Start

### Using Docker (Recommended)

```bash
# Start the service
docker-compose up -d

# Watch logs
docker-compose logs -f

# Check status
docker-compose ps
```

### Local Development

```bash
cd python-service

# Create virtual environment
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# Run
python app.py
```

## 📁 Folder Structure

```
audio/
├── input/           # Drop audio files here
│   └── processed/   # Processed files moved here
└── output/          # Transcriptions appear here
```

## 🎯 Usage

### 1. Drop an audio file

```bash
cp my-audio.mp3 audio/input/
```

### 2. Watch it process

```bash
docker-compose logs -f whisper-service
```

Output:
```
whisper-service | New file detected: my-audio.mp3
whisper-service | Processing: my-audio.mp3
whisper-service | Transcribing: my-audio.mp3
whisper-service | Detected language: Spanish (es)
whisper-service | Transcription saved: my-audio.txt
whisper-service | Moved to processed: my-audio.mp3
```

### 3. Read the transcription

```bash
cat audio/output/my-audio.txt
```

Output:
```
**Detected Language:** Spanish

**Transcription (English):**
Hello, this is a test transcription translated to English...
```

## ⚙️ Configuration

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `INPUT_FOLDER` | `/audio/input` | Folder to monitor for audio files |
| `OUTPUT_FOLDER` | `/audio/output` | Folder for transcription results |
| `WHISPER_MODEL` | `large` | Whisper model size (`tiny`, `base`, `small`, `medium`, `large`) |
| `PYTHONUNBUFFERED` | `1` | Enable real-time logging |

### Change Model Size

Edit `docker-compose.yml`:

```yaml
environment:
  - WHISPER_MODEL=medium  # Faster but less accurate
```

Models:
- `tiny` - Fastest, least accurate (~1GB RAM)
- `base` - Fast, decent (~1GB RAM)
- `small` - Good balance (~2GB RAM)
- `medium` - Better quality (~5GB RAM)
- `large` - Best quality, slowest (~10GB RAM)

## 🎵 Supported Audio Formats

- MP3 (`.mp3`)
- WAV (`.wav`)
- M4A (`.m4a`)
- FLAC (`.flac`)
- OGG (`.ogg`)
- MP4 (`.mp4`)
- WebM (`.webm`)
- AVI (`.avi`)
- MKV (`.mkv`)

## 📊 How It Works

1. **Startup**: Loads Whisper model into memory
2. **Scan**: Processes any existing files in input folder
3. **Monitor**: Watches for new files using `watchdog`
4. **Detect**: New file appears → waits for complete write
5. **Transcribe**: Sends to Whisper for processing
6. **Detect Language**: Identifies source language
7. **Translate**: Translates to English
8. **Save**: Writes formatted output to text file
9. **Archive**: Moves processed file to `processed/` subfolder

## 🔧 Advanced Usage

### Process Specific File Types

Edit `app.py`:

```python
AUDIO_EXTENSIONS = {'.mp3', '.wav'}  # Only MP3 and WAV
```

### Change Output Format

Edit the `transcribe_audio` function:

```python
output_text = f"{detected_language_name}: {transcription}"
```

### Keep Files in Input Folder

Comment out the move operation in `transcribe_audio`:

```python
# os.rename(audio_path, processed_path)
```

### Add Timestamps

```python
result = model.transcribe(audio_path, task="translate", verbose=False, word_timestamps=True)
```

## 📝 Logging

The service provides detailed logging:

- **INFO**: Normal operations (file detected, processed, saved)
- **WARNING**: Non-critical issues (file not complete, can't move)
- **ERROR**: Processing failures (transcription errors)
- **DEBUG**: Detailed debugging info

View logs:
```bash
docker-compose logs -f whisper-service
```

## 🐛 Troubleshooting

### Files not processing?

Check logs:
```bash
docker-compose logs whisper-service | grep ERROR
```

### Service crashed?

Restart:
```bash
docker-compose restart whisper-service
```

### Out of memory?

Use smaller model:
```yaml
environment:
  - WHISPER_MODEL=medium  # or small, base, tiny
```

### Slow processing?

- Use smaller model
- Check CPU usage: `docker stats`
- Ensure audio files aren't too large
- Consider GPU acceleration

## 🚀 Performance

| Model | Speed (1 min audio) | RAM | Accuracy |
|-------|---------------------|-----|----------|
| tiny | ~5 sec | ~1GB | Good |
| base | ~10 sec | ~1GB | Better |
| small | ~20 sec | ~2GB | Very Good |
| medium | ~40 sec | ~5GB | Excellent |
| large | ~60 sec | ~10GB | Best |

*Times are approximate on CPU*

## 🔐 Production Considerations

### Security
- Run as non-root user
- Limit folder permissions
- Scan uploaded files for malware
- Add rate limiting

### Scaling
- Use message queue (RabbitMQ, Redis)
- Run multiple worker containers
- Use GPU acceleration
- Implement load balancing

### Monitoring
- Add health check endpoint
- Export metrics (Prometheus)
- Set up alerts
- Track processing time

## 📦 Dependencies

- `openai-whisper` - Whisper transcription
- `watchdog` - File system monitoring
- `torch` - PyTorch backend
- `ffmpeg` - Audio processing

## 🤝 Contributing

Ideas for improvements:
- Add REST API endpoints
- Support batch processing
- Add progress tracking
- Email/webhook notifications
- Speaker diarization
- Custom vocabulary

## 📄 License

MIT

## 🙏 Credits

- OpenAI Whisper: https://github.com/openai/whisper
- Watchdog: https://github.com/gorakhargosh/watchdog
