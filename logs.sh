#!/bin/bash

echo "📊 Monitoring Whisper Transcription System"
echo ""
echo "Press Ctrl+C to stop watching logs"
echo ""

docker-compose logs -f
