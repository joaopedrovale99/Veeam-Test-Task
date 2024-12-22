﻿using BenchmarkDotNet.Attributes;
using VeeamTestTask;
using VeeamTestTask.Synchronizer;

namespace Benchmarking;

public class FolderSynchronizationBenchmark()
{
    private readonly Configuration _config = Configuration.Load([
        @"C:\RK Keyboard Software",
        @"C:\Replica",
        "5",
        "true",
        @"C:\Log\SyncLog.txt"
    ]);

    [Benchmark]
    public async Task SynchronizeDirectoriesAsync()
    {
        var synchronizer = new SynchronizerAsync();
        await synchronizer.SynchronizeDirectoriesAsync(_config.ReplicaPath, _config.SourcePath, SynchronizerMode.Delete);
        await synchronizer.SynchronizeDirectoriesAsync(_config.SourcePath, _config.ReplicaPath, SynchronizerMode.Create);
    }

    [Benchmark]
    public void SynchronizeDirectoriesNonAsync()
    {
        var synchronizer = new Synchronizer();
        synchronizer.SynchronizeDirectories(_config.ReplicaPath, _config.SourcePath, SynchronizerMode.Delete);
        synchronizer.SynchronizeDirectories(_config.SourcePath, _config.ReplicaPath, SynchronizerMode.Create);
    }
}