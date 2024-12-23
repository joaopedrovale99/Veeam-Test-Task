using Serilog;
using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;

namespace VeeamTestTask;

public class Configuration
{
    public string SourcePath { get; }
    public string ReplicaPath { get; }
    public int SyncIntervalSeconds { get; }
    public bool AsyncFlag { get; }
    public string LogFilePath { get; }

    private Configuration(string sourcePath, string replicaPath, int syncIntervalSeconds, bool asyncFlag, string logFilePath)
    {
        SourcePath = sourcePath;
        ReplicaPath = replicaPath;
        SyncIntervalSeconds = syncIntervalSeconds;
        AsyncFlag = asyncFlag;
        LogFilePath = logFilePath;
    }

    public static Configuration Load(string[] args)
    {
        if (args.Length < 5)
        {
            throw new ArgumentException("Usage: dotnet run <sourcePath> <replicaPath> <syncIntervalSeconds> <asyncFlag> <logFilePath>");
        }

        var sourcePath = args[0];
        var replicaPath = args[1];
        if (!Directory.Exists(sourcePath))
        {
            throw new DirectoryNotFoundException($"Source folder does not exist: {sourcePath}");
        }

        var syncIntervalSeconds = int.Parse(args[2]);
        var asyncFlag = bool.Parse(args[3]);
        var logFilePath = args[4];

        ConfigureLogger(logFilePath);

        return new Configuration(sourcePath, replicaPath, syncIntervalSeconds, asyncFlag, logFilePath);
    }

    private static void ConfigureLogger(string logFilePath)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(logFilePath)
            .CreateLogger();
    }
}