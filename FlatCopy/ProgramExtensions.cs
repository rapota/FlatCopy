using Microsoft.Extensions.Configuration;

namespace FlatCopy;

internal static class ProgramExtensions
{
    public static IConfigurationRoot BuildConfiguration(string[] args) =>
        new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddCommandLine(args)
            .Build();
}