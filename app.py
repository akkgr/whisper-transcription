import whisper
import gradio as gr
import numpy as np
import torch
import os

# Check if model exists in cache, if not download it
def ensure_model_downloaded():
    cache_dir = "/root/.cache/whisper"
    # Check if any model files exist in cache
    if os.path.exists(cache_dir) and os.listdir(cache_dir):
        print("Model found in cache, loading...")
    else:
        print("Model not found in cache, downloading...")
        # This will download to the cache directory
        whisper.load_model("large")
        print("Model downloaded and cached!")

# Ensure model is downloaded
ensure_model_downloaded()

# Load model with GPU support if available
device = "cuda" if torch.cuda.is_available() else "cpu"
print(f"Using device: {device}")
model = whisper.load_model("large", device=device)

def transcribe(audio_file):
    # Debug: Print the type and value of audio input
    print(f"Audio file input type: {type(audio_file)}")
    print(f"Audio file input: {audio_file}")
    
    if audio_file is None:
        return "No audio file uploaded."
    
    # Get the file path
    file_path = audio_file.name if hasattr(audio_file, 'name') else audio_file
    print(f"Processing file: {file_path}")
    
    try:
        # Use Whisper to transcribe and translate to English
        result = model.transcribe(file_path, language="en")
        return result['text']
    except Exception as e:
        return f"Error processing audio: {str(e)}"

interface = gr.Interface(fn=transcribe, inputs=gr.File(file_types=["audio"]), outputs=gr.Textbox(lines=10, max_lines=20, show_copy_button=True))
interface.launch(server_name="0.0.0.0", share=True)