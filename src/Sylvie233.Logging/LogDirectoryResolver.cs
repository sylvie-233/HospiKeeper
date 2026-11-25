using System.IO;

namespace Sylvie233.Logging;

/// <summary>日志目录解析：环境变量 → 显式配置 → 仓库根目录 → 程序目录。</summary>
internal static class LogDirectoryResolver
{
    public const string DirectoryEnvironmentVariable = "SYLVIE233_LOG_DIRECTORY";

    public static string Resolve(LoggingOptions options)
    {
        var fromEnvironment = Environment.GetEnvironmentVariable(DirectoryEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return EnsureDirectory(Path.GetFullPath(fromEnvironment.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(options.LogDirectory))
        {
            var configured = options.LogDirectory.Trim();
            var fullPath = Path.IsPathRooted(configured)
                ? Path.GetFullPath(configured)
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configured));
            return EnsureDirectory(fullPath);
        }

        var root = FindSolutionRoot(AppContext.BaseDirectory) ?? AppContext.BaseDirectory;
        return EnsureDirectory(Path.Combine(root, "logs"));
    }

    private static string EnsureDirectory(string directory)
    {
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string? FindSolutionRoot(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git"))
                || Directory.GetFiles(directory.FullName, "*.slnx").Length > 0
                || Directory.GetFiles(directory.FullName, "*.sln").Length > 0)
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
