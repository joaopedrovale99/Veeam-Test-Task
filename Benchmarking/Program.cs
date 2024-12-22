using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Benchmarking;

var benchmarkRunnerConfig = new ManualConfig()
    .WithOptions(ConfigOptions.DisableOptimizationsValidator)
    .AddLogger(BenchmarkDotNet.Loggers.ConsoleLogger.Default);
BenchmarkRunner.Run<FolderSynchronizationBenchmark>(benchmarkRunnerConfig);