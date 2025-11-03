using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<AudioFileMonitor>();
builder.Services.AddHostedService<AudioFileMonitor>(provider => provider.GetRequiredService<AudioFileMonitor>());

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints
app.MapGet("/status", (AudioFileMonitor monitor) =>
{
    return Results.Ok(new
    {
        IsRunning = monitor.IsRunning,
        ProcessedFiles = monitor.ProcessedFilesCount,
        InputFolder = monitor.InputFolder,
        OutputFolder = monitor.OutputFolder,
        WhisperUrl = monitor.WhisperUrl
    });
})
.WithName("GetStatus")
.WithOpenApi();

app.MapPost("/start", (AudioFileMonitor monitor) =>
{
    if (monitor.IsRunning)
        return Results.BadRequest("Monitor is already running");

    monitor.Start();
    return Results.Ok("Monitor started");
})
.WithName("StartMonitor")
.WithOpenApi();

app.MapPost("/stop", (AudioFileMonitor monitor) =>
{
    if (!monitor.IsRunning)
        return Results.BadRequest("Monitor is not running");

    monitor.Stop();
    return Results.Ok("Monitor stopped");
})
.WithName("StopMonitor")
.WithOpenApi();

app.Run();

public class AudioFileMonitor : BackgroundService
{
    private readonly ILogger<AudioFileMonitor> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private FileSystemWatcher? _watcher;
    private bool _isRunning;
    private int _processedFiles;

    public string InputFolder { get; }
    public string OutputFolder { get; }
    public string WhisperUrl { get; }
    public bool IsRunning => _isRunning;
    public int ProcessedFilesCount => _processedFiles;

    public AudioFileMonitor(
        ILogger<AudioFileMonitor> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;

        InputFolder = _configuration["AudioMonitor:InputFolder"] ?? "/audio/input";
        OutputFolder = _configuration["AudioMonitor:OutputFolder"] ?? "/audio/output";
        WhisperUrl = _configuration["AudioMonitor:WhisperUrl"] ?? "http://localhost:7860/";

        // Ensure directories exist
        Directory.CreateDirectory(InputFolder);
        Directory.CreateDirectory(OutputFolder);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audio File Monitor starting...");
        _logger.LogInformation("Input folder: {InputFolder}", InputFolder);
        _logger.LogInformation("Output folder: {OutputFolder}", OutputFolder);
        _logger.LogInformation("Whisper URL: {WhisperUrl}", WhisperUrl);

        Start();
        return Task.CompletedTask;
    }

    public void Start()
    {
        if (_isRunning) return;

        _watcher = new FileSystemWatcher(InputFolder)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*",
            EnableRaisingEvents = true
        };

        // Subscribe to events
        _watcher.Created += OnFileCreated;
        _watcher.Changed += OnFileChanged;

        _isRunning = true;
        _logger.LogInformation("File monitor started");

        // Process existing files in the folder
        ProcessExistingFiles();
    }

    public void Stop()
    {
        if (!_isRunning) return;

        if (_watcher != null)
        {
            _watcher.Created -= OnFileCreated;
            _watcher.Changed -= OnFileChanged;
            _watcher.Dispose();
            _watcher = null;
        }

        _isRunning = false;
        _logger.LogInformation("File monitor stopped");
    }

    private void ProcessExistingFiles()
    {
        try
        {
            var audioExtensions = new[] { ".mp3", ".wav", ".m4a", ".flac", ".ogg", ".mp4", ".webm" };
            var files = Directory.GetFiles(InputFolder)
                .Where(f => audioExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToList();

            _logger.LogInformation("Found {Count} existing audio files to process", files.Count);

            foreach (var file in files)
            {
                Task.Run(async () => await ProcessAudioFileAsync(file));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing existing files");
        }
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("New file detected: {FileName}", e.Name);
        Task.Run(async () => await ProcessAudioFileAsync(e.FullPath));
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Handle file changes if needed
        _logger.LogDebug("File changed: {FileName}", e.Name);
    }

    private async Task ProcessAudioFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        try
        {
            // Check if it's an audio file
            var audioExtensions = new[] { ".mp3", ".wav", ".m4a", ".flac", ".ogg", ".mp4", ".webm" };
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (!audioExtensions.Contains(extension))
            {
                _logger.LogDebug("Skipping non-audio file: {FileName}", fileName);
                return;
            }

            // Wait for file to be fully written
            await WaitForFileToBeReady(filePath);

            _logger.LogInformation("Processing audio file: {FileName}", fileName);

            // Send to Whisper API
            var transcription = await SendToWhisperAsync(filePath);

            if (!string.IsNullOrEmpty(transcription))
            {
                // Save transcription to output folder
                var outputFileName = Path.GetFileNameWithoutExtension(fileName) + ".txt";
                var outputPath = Path.Combine(OutputFolder, outputFileName);

                await File.WriteAllTextAsync(outputPath, transcription);

                _logger.LogInformation("Transcription saved: {OutputFileName}", outputFileName);

                Interlocked.Increment(ref _processedFiles);

                // Optionally, delete or move the processed audio file
                // File.Delete(filePath);
                // or
                // var processedFolder = Path.Combine(InputFolder, "processed");
                // Directory.CreateDirectory(processedFolder);
                // File.Move(filePath, Path.Combine(processedFolder, fileName));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file: {FileName}", fileName);

            // Save error to file
            var errorFileName = Path.GetFileNameWithoutExtension(fileName) + "_error.txt";
            var errorPath = Path.Combine(OutputFolder, errorFileName);
            await File.WriteAllTextAsync(errorPath, $"Error: {ex.Message}\n\n{ex.StackTrace}");
        }
    }

    private async Task WaitForFileToBeReady(string filePath, int maxRetries = 10)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return;
            }
            catch (IOException)
            {
                await Task.Delay(500);
            }
        }

        throw new IOException($"File {filePath} is not ready after {maxRetries} retries");
    }

    private async Task<string?> SendToWhisperAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Sending file to Whisper via Python client...");

            // Use the Python Gradio client for reliable communication
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "python3",
                Arguments = $"/app/transcribe.py \"{filePath}\" \"{WhisperUrl.TrimEnd('/')}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new System.Diagnostics.Process { StartInfo = startInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                _logger.LogInformation("Transcription completed successfully");
                return output.Trim();
            }
            else
            {
                _logger.LogError("Python client error: {Error}", error);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending file to Whisper API");
        }

        return null;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Stop();
        return base.StopAsync(cancellationToken);
    }
}
