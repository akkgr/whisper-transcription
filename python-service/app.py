import os
import time
import logging
from pathlib import Path
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler
import whisper
from whisper.tokenizer import LANGUAGES

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Configuration
INPUT_FOLDER = os.getenv('INPUT_FOLDER', '/audio/input')
OUTPUT_FOLDER = os.getenv('OUTPUT_FOLDER', '/audio/output')
MODEL_SIZE = os.getenv('WHISPER_MODEL', 'large')
PROCESSED_FOLDER = os.path.join(INPUT_FOLDER, 'processed')

# Supported audio extensions
AUDIO_EXTENSIONS = {'.mp3', '.wav', '.m4a', '.flac', '.ogg', '.mp4', '.webm', '.avi', '.mkv'}

# Global Whisper model
model = None
processed_files = 0


def load_model():
    """Load the Whisper model"""
    global model
    logger.info(f"Loading Whisper model: {MODEL_SIZE}")
    model = whisper.load_model(MODEL_SIZE)
    logger.info("Model loaded successfully")


def is_audio_file(filepath):
    """Check if file is a supported audio format"""
    return Path(filepath).suffix.lower() in AUDIO_EXTENSIONS


def wait_for_file_complete(filepath, timeout=30):
    """Wait for file to be completely written"""
    size = -1
    for _ in range(timeout):
        try:
            new_size = os.path.getsize(filepath)
            if new_size == size:
                return True
            size = new_size
            time.sleep(1)
        except (OSError, FileNotFoundError):
            time.sleep(1)
    return False


def transcribe_audio(audio_path):
    """Transcribe audio file using Whisper"""
    global processed_files
    
    filename = os.path.basename(audio_path)
    logger.info(f"Processing: {filename}")
    
    try:
        # Wait for file to be completely written
        if not wait_for_file_complete(audio_path):
            logger.warning(f"File may not be complete: {filename}")
        
        # Transcribe with translation to English
        logger.info(f"Transcribing: {filename}")
        result = model.transcribe(audio_path, task="translate", verbose=False)
        
        # Get detected language
        detected_language_code = result.get('language', 'unknown')
        detected_language_name = LANGUAGES.get(detected_language_code, detected_language_code).title()
        transcription = result['text'].strip()
        
        # Format output
        output_text = f"""**Detected Language:** {detected_language_name}

**Transcription (English):**
{transcription}
"""
        
        # Save transcription
        output_filename = Path(filename).stem + '.txt'
        output_path = os.path.join(OUTPUT_FOLDER, output_filename)
        
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write(output_text)
        
        logger.info(f"Transcription saved: {output_filename}")
        logger.info(f"Detected language: {detected_language_name} ({detected_language_code})")
        
        processed_files += 1
        
        # Move processed file to processed folder
        try:
            os.makedirs(PROCESSED_FOLDER, exist_ok=True)
            processed_path = os.path.join(PROCESSED_FOLDER, filename)
            os.rename(audio_path, processed_path)
            logger.info(f"Moved to processed: {filename}")
        except Exception as e:
            logger.warning(f"Could not move file to processed: {e}")
        
        return True
        
    except Exception as e:
        logger.error(f"Error processing {filename}: {str(e)}", exc_info=True)
        
        # Save error to file
        error_filename = Path(filename).stem + '_error.txt'
        error_path = os.path.join(OUTPUT_FOLDER, error_filename)
        
        with open(error_path, 'w', encoding='utf-8') as f:
            f.write(f"Error processing {filename}:\n\n{str(e)}")
        
        return False


class AudioFileHandler(FileSystemEventHandler):
    """Handler for audio file events"""
    
    def on_created(self, event):
        """Called when a file is created"""
        if event.is_directory:
            return
        
        filepath = event.src_path
        
        if is_audio_file(filepath):
            logger.info(f"New file detected: {os.path.basename(filepath)}")
            # Small delay to ensure file is ready
            time.sleep(1)
            transcribe_audio(filepath)
    
    def on_modified(self, event):
        """Called when a file is modified (useful for large files being copied)"""
        # We handle this in wait_for_file_complete
        pass


def process_existing_files():
    """Process any existing files in the input folder"""
    logger.info(f"Checking for existing files in: {INPUT_FOLDER}")
    
    files = []
    for file in os.listdir(INPUT_FOLDER):
        filepath = os.path.join(INPUT_FOLDER, file)
        if os.path.isfile(filepath) and is_audio_file(filepath):
            files.append(filepath)
    
    if files:
        logger.info(f"Found {len(files)} existing audio file(s) to process")
        for filepath in files:
            transcribe_audio(filepath)
    else:
        logger.info("No existing audio files found")


def main():
    """Main function"""
    logger.info("=" * 60)
    logger.info("Whisper Audio Transcription Service")
    logger.info("=" * 60)
    logger.info(f"Input folder: {INPUT_FOLDER}")
    logger.info(f"Output folder: {OUTPUT_FOLDER}")
    logger.info(f"Model: {MODEL_SIZE}")
    logger.info(f"Supported formats: {', '.join(sorted(AUDIO_EXTENSIONS))}")
    
    # Create folders if they don't exist
    os.makedirs(INPUT_FOLDER, exist_ok=True)
    os.makedirs(OUTPUT_FOLDER, exist_ok=True)
    os.makedirs(PROCESSED_FOLDER, exist_ok=True)
    
    # Load Whisper model
    load_model()
    
    # Process existing files
    process_existing_files()
    
    # Setup file system monitoring
    logger.info("Starting file system monitor...")
    event_handler = AudioFileHandler()
    observer = Observer()
    observer.schedule(event_handler, INPUT_FOLDER, recursive=False)
    observer.start()
    
    logger.info("✓ Service is running and monitoring for new files")
    logger.info(f"✓ Processed {processed_files} file(s) so far")
    logger.info("Press Ctrl+C to stop")
    
    try:
        while True:
            time.sleep(10)
            # Periodic status update
            if processed_files > 0:
                logger.debug(f"Total processed: {processed_files} file(s)")
    except KeyboardInterrupt:
        logger.info("Stopping service...")
        observer.stop()
    
    observer.join()
    logger.info("Service stopped")


if __name__ == "__main__":
    main()
