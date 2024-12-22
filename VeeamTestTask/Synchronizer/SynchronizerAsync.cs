using System.Security.Cryptography;
using Serilog;

namespace VeeamTestTask.Synchronizer;

public class SynchronizerAsync
{
    private int _changes;

    public async Task<int> SynchronizeDirectoriesAsync(string sourcePath, string targetPath, SynchronizerMode mode)
    {
        await RecursiveSynchronizeDirectoriesAsync(sourcePath, targetPath, mode);
        var syncChanges = _changes;
        _changes = 0;
        return syncChanges;
    }

    private async Task RecursiveSynchronizeDirectoriesAsync(string sourcePath, string targetPath, SynchronizerMode mode)
    {
        var tasks = new List<Task> { SynchronizeFilesAsync(sourcePath, targetPath, mode) };
        foreach (var directory in Directory.GetDirectories(sourcePath))
        {
            var directoryName = Path.GetFileName(directory);
            var targetDirectory = Path.Combine(targetPath, directoryName);

            if (!Directory.Exists(targetDirectory))
            {
                if (mode == SynchronizerMode.Create)
                    CreateDirectory(targetDirectory);
                else
                {
                    DeleteDirectory(directory);
                    continue;
                }
            }

            tasks.Add(RecursiveSynchronizeDirectoriesAsync(directory, targetDirectory, mode));
        }

        await Task.WhenAll(tasks);
    }

    private async Task SynchronizeFilesAsync(string sourcePath, string targetPath, SynchronizerMode mode)
    {
        var tasks = Directory.GetFiles(sourcePath).Select(file =>
        {
            return Task.Run(async () =>
            {
                var fileName = Path.GetFileName(file);
                var targetFile = Path.Combine(targetPath, fileName);
                var fileExists = File.Exists(targetFile);

                if (fileExists && await FilesAreIdenticalAsync(file, targetFile))
                    return;

                if (mode == SynchronizerMode.Create)
                    CopyFile(file, targetFile, fileExists);
                else
                    DeleteFile(file);
            });
        }).ToArray();

        await Task.WhenAll(tasks);
    }

    private void DeleteDirectory(string directory)
    {
        Directory.Delete(directory, true);
        Log.Information($"Deleted directory: {directory}");
        _changes++;
    }

    private void CreateDirectory(string directory)
    {
        Directory.CreateDirectory(directory);
        Log.Information($"Created directory: {directory}");
        _changes++;
    }

    private void DeleteFile(string file)
    {
        File.Delete(file);
        Log.Information($"Deleted file: {file}");
        _changes++;
    }

    private void CopyFile(string sourceFile, string targetFile, bool fileExists)
    {
        File.Copy(sourceFile, targetFile, true);
        Log.Information($"{(fileExists ? "Updated" : "Created")} file: {sourceFile}");
        _changes++;
    }

    /**
     * MD5 hashing from https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.md5?view=net-9.0
     */
    private static async Task<bool> FilesAreIdenticalAsync(string file1, string file2)
    {
        // TODO: Since this runs asynchronously, it is trying to use 100% CPU and memory. Find a way to improve this if not intended.
        var fileStream1Task = Task.Run(() => File.ReadAllBytes(file1));
        var fileStream2Task = Task.Run(() => File.ReadAllBytes(file2));

        await Task.WhenAll(fileStream1Task, fileStream2Task);

        var hash1Task = Task.Run(() => MD5.HashData(fileStream1Task.Result));
        var hash2Task = Task.Run(() => MD5.HashData(fileStream2Task.Result));

        await Task.WhenAll(hash1Task, hash2Task);

        return hash1Task.Result.SequenceEqual(hash2Task.Result);
    }
}