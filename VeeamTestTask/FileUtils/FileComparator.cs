using System.Security.Cryptography;

namespace VeeamTestTask.FileUtils;

public static class FileComparator
{
    public static bool LastWriteTime(string file1, string file2)
    {
        var fileInfo1 = new FileInfo(file1);
        var fileInfo2 = new FileInfo(file2);
        return fileInfo1.LastWriteTimeUtc != fileInfo2.LastWriteTimeUtc;
    }

    /**
     * MD5 hashing from https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.md5?view=net-9.0
     */
    public static async Task<bool> Md5Async(string file1, string file2)
    {
        var fileStream1Task = Task.Run(() => File.ReadAllBytes(file1));
        var fileStream2Task = Task.Run(() => File.ReadAllBytes(file2));

        await Task.WhenAll(fileStream1Task, fileStream2Task);

        var hash1Task = Task.Run(() => MD5.HashData(fileStream1Task.Result));
        var hash2Task = Task.Run(() => MD5.HashData(fileStream2Task.Result));

        await Task.WhenAll(hash1Task, hash2Task);

        return hash1Task.Result.SequenceEqual(hash2Task.Result);
    }

    /**
     * MD5 hashing from https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.md5?view=net-9.0
     */
    public static bool Md5(string file1, string file2)
    {
        var hash1 = MD5.HashData(File.ReadAllBytes(file1));
        var hash2 = MD5.HashData(File.ReadAllBytes(file2));
        return hash1.SequenceEqual(hash2);
    }
}