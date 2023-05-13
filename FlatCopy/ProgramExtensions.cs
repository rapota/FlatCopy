using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace FlatCopy;

internal static class ProgramExtensions
{
    public static IConfigurationRoot BuildConfiguration(string[] args) =>
        new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddCommandLine(args)
            .Build();

    public static Logger CreateLogger(IConfiguration configuration)
    {
        LoggerConfiguration loggerConfiguration = new();
        loggerConfiguration.ReadFrom.Configuration(configuration);

        return loggerConfiguration.CreateLogger();
    }
}