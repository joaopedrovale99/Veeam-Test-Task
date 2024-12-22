using Serilog;
using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;

namespace VeeamTestTask;

public class Configuration
{
    public string SourcePath { get; }
    public string ReplicaPath { get; }
    public int SyncIntervalSeconds { get; }
    public string LogFilePath { get; }

    private Configuration(string sourcePath, string replicaPath, int syncIntervalSeconds, string logFilePath)
    {
        SourcePath = sourcePath;
        ReplicaPath = replicaPath;
        SyncIntervalSeconds = syncIntervalSeconds;
        LogFilePath = logFilePath;
    }

    public static Configuration Load(string[] args)
    {
        if (args.Length < 4)
        {
            throw new ArgumentException("Usage: FolderSynchronizer <sourcePath> <replicaPath> <syncIntervalSeconds> <logFilePath>");
        }

        var sourcePath = args[0];
        var replicaPath = args[1];
        if (!Directory.Exists(sourcePath))
        {
            throw new DirectoryNotFoundException($"Source folder does not exist: {sourcePath}");
        }

        var syncIntervalSeconds = int.Parse(args[2]);
        var logFilePath = args[3];

        ConfigureLogger(logFilePath);

        return new Configuration(sourcePath, replicaPath, syncIntervalSeconds, logFilePath);
    }

    private static void ConfigureLogger(string logFilePath)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(logFilePath)
            .CreateLogger();
    }
}