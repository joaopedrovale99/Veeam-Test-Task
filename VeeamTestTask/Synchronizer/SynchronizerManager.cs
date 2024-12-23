using System.Diagnostics;
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
            await Synchronize();
            while (true)
            {
                await _syncTimer.WaitForNextTickAsync();
                await Synchronize();
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("Synchronization stopped.");
        }
    }

    private async Task Synchronize()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var changes = await Synchronize(_config.AsyncFlag);
            stopwatch.Stop();
            Log.Information($"Folder synchronization completed. {changes} changes. Elapsed time: {stopwatch.Elapsed}");
        }
        catch (Exception)
        {
            Log.Error("Error during synchronization.");
        }
    }

    private async Task<int> Synchronize(bool asyncFlag)
    {
        BaseSynchronizer synchronizer = asyncFlag ? new SynchronizerAsync() : new Synchronizer();

        var changes = 0;

        changes += await synchronizer.SynchronizeDirectories(_config.ReplicaPath, _config.SourcePath, SynchronizerMode.Delete);
        changes += await synchronizer.SynchronizeDirectories(_config.SourcePath, _config.ReplicaPath, SynchronizerMode.Create);

        return changes;
    }
}