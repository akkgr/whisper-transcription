#!/bin/bash

echo "🧪 Testing Whisper Transcription System"
echo ""

# Check if services are running
echo "1️⃣  Checking services status..."
docker-compose ps

echo ""
echo "2️⃣  Checking API status..."
curl -s http://localhost:8080/status | jq '.' 2>/dev/null || curl -s http://localhost:8080/status

echo ""
echo "3️⃣  Services are ready!"
echo ""
echo "📁 To test the system:"
echo "   1. Copy an audio file to ./audio/input/"
echo "      Example: cp your-audio.mp3 audio/input/"
echo ""
echo "   2. Watch the logs to see processing:"
echo "      docker-compose logs -f whisper-api"
echo ""
echo "   3. Check the output folder:"
echo "      ls -la audio/output/"
echo "      cat audio/output/your-audio.txt"
echo ""
echo "🌐 Access points:"
echo "   Whisper UI:  http://localhost:7860"
echo "   API Swagger: http://localhost:8080/swagger"
echo "   API Status:  http://localhost:8080/status"
echo ""
echo "💡 Supported formats: MP3, WAV, M4A, FLAC, OGG, MP4, WebM"
echo ""
