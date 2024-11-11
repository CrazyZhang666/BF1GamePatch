namespace BF1GamePatch.Helper;

public static class FileHelper
{
    private static readonly MD5 md5 = MD5.Create();

    /// <summary>
    /// 创建文件夹
    /// </summary>
    public static void CreateDirectory(string dirPath)
    {
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
    }

    /// <summary>
    /// 异步删除文件夹及其子文件夹
    /// </summary>
    public static Task DeleteDirectoryAsync(string dirPath)
    {
        return Task.Run(() =>
        {
            if (Directory.Exists(dirPath))
                Directory.Delete(dirPath, true);
        });
    }

    /// <summary>
    /// 异步删除文件
    /// </summary>
    public static Task DeleteFileAsync(string filePath)
    {
        return Task.Run(() =>
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        });
    }

    /// <summary>
    /// 异步复制文件
    /// </summary>
    public static Task CopyFileAsync(string oldPath, string newPath)
    {
        return Task.Run(() =>
        {
            if (!File.Exists(oldPath))
                return;

            var newDir = Path.GetDirectoryName(newPath);
            CreateDirectory(newDir);

            File.Copy(oldPath, newPath, true);
        });
    }

    /// <summary>
    /// 获取嵌入资源流（自动添加前缀）
    /// </summary>
    public static Stream GetEmbeddedResourceStream(string resPath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream($"BF1GamePatch.Assets.Files.{resPath}");
    }

    /// <summary>
    /// 获取嵌入资源文本内容
    /// </summary>
    public static string GetEmbeddedResourceText(string resPath)
    {
        var stream = GetEmbeddedResourceStream(resPath);
        if (stream is null)
            return string.Empty;

        return new StreamReader(stream, Encoding.UTF8).ReadToEnd();
    }

    /// <summary>
    /// 获取嵌入资源文本全部行
    /// </summary>
    public static List<string> GetEmbeddedResourceTextAllLine(string resPath)
    {
        var stream = GetEmbeddedResourceStream(resPath);
        if (stream is null)
            return null;

        var allLines = new List<string>();
        var reader = new StreamReader(stream);

        while (reader.Peek() >= 0)
        {
            allLines.Add(reader.ReadLine());
        }
        reader.Close();

        return allLines;
    }

    /// <summary>
    /// 获取文件MD5值
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<string> GetFileMD5(string filePath)
    {
        if (!File.Exists(filePath))
            return string.Empty;

        using var fileStream = File.OpenRead(filePath);
        var fileMD5 = await md5.ComputeHashAsync(fileStream);

        return BitConverter.ToString(fileMD5).Replace("-", "");
    }
}
