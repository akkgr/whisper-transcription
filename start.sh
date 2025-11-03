#!/bin/bash

echo "🚀 Starting Whisper Transcription System..."
echo ""

# Build and start services
echo "📦 Building Docker containers..."
docker-compose up -d --build

echo ""
echo "⏳ Waiting for services to start..."
sleep 5

# Check status
echo ""
echo "📊 Checking service status..."
docker-compose ps

echo ""
echo "🔍 Checking API status..."
curl -s http://localhost:8080/status | jq '.' || echo "API not ready yet, wait a moment..."

echo ""
echo "✅ System is running!"
echo ""
echo "📁 Folders:"
echo "   Input:  ./audio/input  (drop audio files here)"
echo "   Output: ./audio/output (transcriptions will appear here)"
echo ""
echo "🌐 URLs:"
echo "   Whisper UI:  http://localhost:7860"
echo "   API Swagger: http://localhost:8080/swagger"
echo "   API Status:  http://localhost:8080/status"
echo ""
echo "📝 Usage:"
echo "   1. Drop audio files into ./audio/input/"
echo "   2. Transcriptions automatically appear in ./audio/output/"
echo "   3. Check status: curl http://localhost:8080/status"
echo ""
echo "📋 View logs:"
echo "   docker-compose logs -f"
echo ""
