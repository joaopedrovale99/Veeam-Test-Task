using System.Security.Cryptography;
using Serilog;

namespace VeeamTestTask.Synchronizer;

public class Synchronizer
{
    private int _changes;

    public int SynchronizeDirectories(string sourcePath, string targetPath, SynchronizerMode mode)
    {
        RecursiveSynchronizeDirectories(sourcePath, targetPath, mode);
        var syncChanges = _changes;
        _changes = 0;
        return syncChanges;
    }

    private void RecursiveSynchronizeDirectories(string sourcePath, string targetPath, SynchronizerMode mode)
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
    }

    private void SynchronizeFiles(string sourcePath, string targetPath, SynchronizerMode mode)
    {
        foreach (var file in Directory.GetFiles(sourcePath))
        {
            var fileName = Path.GetFileName(file);
            var targetFile = Path.Combine(targetPath, fileName);
            var fileExists = File.Exists(targetFile);

            if (fileExists && FilesAreIdentical(file, targetFile)) continue;

            if (mode == SynchronizerMode.Create)
                CopyFile(file, targetFile, fileExists);
            else
                DeleteFile(file);
        }
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
     * Maybe a TryHashData() would be better
     */
    private static bool FilesAreIdentical(string file1, string file2)
    {
        var hash1 = MD5.HashData(File.ReadAllBytes(file1));
        var hash2 = MD5.HashData(File.ReadAllBytes(file2));
        return hash1.SequenceEqual(hash2);
    }
}