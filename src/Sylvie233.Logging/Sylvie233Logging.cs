using System.IO;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Sylvie233.Logging;

/// <summary>Serilog 统一配置入口：控制台 + 按天滚动文件，控制台与文件可按需关闭。</summary>
public static class Sylvie233Logging
{
    /// <summary>按配置构建一个日志记录器（不修改静态 Log.Logger）。</summary>
    public static Serilog.ILogger CreateLogger(LoggingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        LoggingEnvironmentOverrides.Apply(options);
        return new LoggerConfiguration().Apply(options).CreateLogger();
    }

    /// <summary>把配置应用到已有的 LoggerConfiguration（供宿主 UseSerilog 使用）。</summary>
    public static LoggerConfiguration Apply(this LoggerConfiguration configuration, LoggingOptions options)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(options);
        LoggingEnvironmentOverrides.Apply(options);

        configuration
            .MinimumLevel.Is(options.MinimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", options.ApplicationName);

        if (options.EnrichEnvironment)
        {
            configuration.Enrich.WithMachineName().Enrich.WithEnvironmentName();
        }

        if (options.EnrichThread)
        {
            configuration.Enrich.WithThreadId();
        }

        if (options.EnableConsole)
        {
            configuration.WriteTo.Console(outputTemplate: LoggingOptions.ConsoleTemplate);
        }

        if (options.EnableFile)
        {
            var directory = LogDirectoryResolver.Resolve(options);
            var path = Path.Combine(directory, $"{options.FileNamePrefix}-.log");

            configuration.WriteTo.Async(
                sink => sink.File(
                    path,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: options.RetainedFileCountLimit,
                    fileSizeLimitBytes: options.FileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    outputTemplate: LoggingOptions.FileTemplate),
                bufferSize: 8192,
                blockWhenFull: false);
        }

        return configuration;
    }

    /// <summary>宿主（ASP.NET Core / 通用 Host）接入：替换 ILoggerFactory 并接管请求日志。</summary>
    public static IHostBuilder UseSylvie233Logging(this IHostBuilder hostBuilder, LoggingOptions options)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);
        ArgumentNullException.ThrowIfNull(options);

        return hostBuilder.UseSerilog(
            (_, _, configuration) => configuration.Apply(options),
            preserveStaticLogger: false);
    }
}
