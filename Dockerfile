FROM python:3.10-slim
RUN apt-get update && apt-get install -y ffmpeg git && rm -rf /var/lib/apt/lists/*
RUN pip install --upgrade pip
# Install PyTorch (CPU version - will auto-detect GPU if available)
RUN pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cpu
RUN pip install git+https://github.com/openai/whisper.git gradio numpy
COPY app.py /app/app.py
WORKDIR /app
CMD ["python", "app.py"]