using Serilog;

namespace VeeamTestTask.Synchronizer;

public class SynchronizerManager
{
    private readonly Configuration _config;
    private PeriodicTimer? _syncTimer;

    public SynchronizerManager(Configuration config)
    {
        _config = config;

        if (Directory.Exists(_config.ReplicaPath)) return;

        Directory.CreateDirectory(_config.ReplicaPath);
        Log.Information($"Created replica folder: {_config.ReplicaPath}");
    }

    public async Task Start()
    {
        Log.Information("Synchronization started.");
        _syncTimer = new PeriodicTimer(TimeSpan.FromSeconds(_config.SyncIntervalSeconds));

        try
        {
            Synchronize();
            while (true)
            {
                await _syncTimer.WaitForNextTickAsync();
                Synchronize();
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("Synchronization stopped.");
        }
    }

    private void Synchronize()
    {
        try
        {
            var synchronizer = new Synchronizer();
            var changes = synchronizer.SynchronizeDirectories(_config.SourcePath, _config.ReplicaPath, SynchronizerMode.Create);
            changes += synchronizer.SynchronizeDirectories(_config.ReplicaPath, _config.SourcePath, SynchronizerMode.Delete);
            Log.Information($"Folder synchronization completed. {changes} changes.");
        }
        catch (Exception)
        {
            Log.Error("Error during synchronization.");
        }
    }
}