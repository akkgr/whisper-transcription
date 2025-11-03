FROM python:3.10-slim

# Install system dependencies
RUN apt-get update && apt-get install -y \
    ffmpeg \
    git \
    && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

# Copy requirements
COPY requirements.txt .

# Install Python dependencies
RUN pip install --no-cache-dir --upgrade pip && \
    pip install --no-cache-dir torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cpu && \
    pip install --no-cache-dir -r requirements.txt

# Copy application
COPY app.py .

# Create volume directories
RUN mkdir -p /audio/input /audio/output

# Set environment variables
ENV INPUT_FOLDER=/audio/input
ENV OUTPUT_FOLDER=/audio/output
ENV WHISPER_MODEL=large
ENV PYTHONUNBUFFERED=1

# Run the application
CMD ["python", "-u", "app.py"]
