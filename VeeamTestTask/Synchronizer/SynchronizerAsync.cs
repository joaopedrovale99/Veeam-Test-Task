using Serilog;
using VeeamTestTask.FileUtils;

namespace VeeamTestTask.Synchronizer;

public class SynchronizerAsync : BaseSynchronizer
{
    protected override async Task RecursiveSynchronizeDirectories(string sourcePath, string targetPath, SynchronizerMode mode)
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

            tasks.Add(RecursiveSynchronizeDirectories(directory, targetDirectory, mode));
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
}