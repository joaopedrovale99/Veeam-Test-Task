using Serilog;

namespace VeeamTestTask.Synchronizer;

public abstract class BaseSynchronizer
{
    private int _changes;

    public async Task<int> SynchronizeDirectories(string sourcePath, string targetPath, SynchronizerMode mode)
    {
        await RecursiveSynchronizeDirectories(sourcePath, targetPath, mode);
        var syncChanges = _changes;
        _changes = 0;
        return syncChanges;
    }

    protected abstract Task RecursiveSynchronizeDirectories(string sourcePath, string targetPath, SynchronizerMode mode);

    protected void DeleteDirectory(string directory)
    {
        Directory.Delete(directory, true);
        Log.Information($"Deleted directory: {directory}");
        _changes++;
    }

    protected void CreateDirectory(string directory)
    {
        Directory.CreateDirectory(directory);
        Log.Information($"Created directory: {directory}");
        _changes++;
    }

    protected void DeleteFile(string file)
    {
        File.Delete(file);
        Log.Information($"Deleted file: {file}");
        _changes++;
    }

    protected void CopyFile(string sourceFile, string targetFile, bool fileExists)
    {
        File.Copy(sourceFile, targetFile, true);
        Log.Information($"{(fileExists ? "Updated" : "Created")} file: {sourceFile}");
        _changes++;
    }
}