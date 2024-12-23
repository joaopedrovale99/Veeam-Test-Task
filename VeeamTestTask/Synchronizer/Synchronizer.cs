using VeeamTestTask.FileUtils;

namespace VeeamTestTask.Synchronizer;

public class Synchronizer : BaseSynchronizer
{
    protected override Task RecursiveSynchronizeDirectories(string sourcePath, string targetPath, SynchronizerMode mode)
    {
        SynchronizeFiles(sourcePath, targetPath, mode);
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

            RecursiveSynchronizeDirectories(directory, targetDirectory, mode);
        }

        return Task.CompletedTask;
    }

    private void SynchronizeFiles(string sourcePath, string targetPath, SynchronizerMode mode)
    {
        foreach (var file in Directory.GetFiles(sourcePath))
        {
            var fileName = Path.GetFileName(file);
            var targetFile = Path.Combine(targetPath, fileName);
            var fileExists = File.Exists(targetFile);

            if (fileExists && FileComparator.Md5(file, targetFile)) continue;

            if (mode == SynchronizerMode.Create)
                CopyFile(file, targetFile, fileExists);
            else
                DeleteFile(file);
        }
    }
}