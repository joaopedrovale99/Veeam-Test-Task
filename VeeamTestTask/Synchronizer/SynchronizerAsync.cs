using System.Security.Cryptography;
using Serilog;
using VeeamTestTask.FileUtils;

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

                if (fileExists && await FileComparator.Md5Async(file, targetFile))
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
}