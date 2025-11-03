#!/usr/bin/env python3
import sys
import json
from gradio_client import Client, handle_file

def transcribe_audio(audio_file_path, gradio_url="http://localhost:7860"):
    try:
        client = Client(gradio_url)
        # Use handle_file to properly format the file input
        result = client.predict(
            audio_file=handle_file(audio_file_path),
            api_name="/predict"
        )
        print(result, end='')
        return 0
    except Exception as e:
        print(f"Error: {str(e)}", file=sys.stderr)
        return 1

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: transcribe.py <audio_file_path> [gradio_url]", file=sys.stderr)
        sys.exit(1)
    
    audio_path = sys.argv[1]
    gradio_url = sys.argv[2] if len(sys.argv) > 2 else "http://localhost:7860"
    
    sys.exit(transcribe_audio(audio_path, gradio_url))
