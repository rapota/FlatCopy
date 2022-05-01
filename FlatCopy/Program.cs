using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

namespace FlatCopy;

class Program
{
    static void Main(string[] args)
    {
        IConfigurationRoot configuration = BuildConfiguration(args);

        Logger logger = CreateLogger(configuration);

        CopyOptions copyOptions = configuration.GetSection("options").Get<CopyOptions>();

        ServiceCollection services = new ServiceCollection();
        services
            .AddSingleton(copyOptions)
            .AddLogging(configure => configure.AddSerilog(logger, true))
            .AddSingleton<FileService>()
            .AddSingleton<Application>();

        using ServiceProvider provider = services.BuildServiceProvider(true);
        Application? application = provider.GetService<Application>();
        application?.Run();
    }

    private static IConfigurationRoot BuildConfiguration(string[] args) =>
        new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddCommandLine(args)
            .Build();

    private static Logger CreateLogger(IConfiguration configuration)
    {
        LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
        loggerConfiguration.ReadFrom.Configuration(configuration);

        return loggerConfiguration.CreateLogger();
    }
}