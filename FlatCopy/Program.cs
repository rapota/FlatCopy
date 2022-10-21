using FlatCopy;
using FlatCopy.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

IConfigurationRoot configuration = ProgramExtensions.BuildConfiguration(args);
Logger logger = ProgramExtensions.CreateLogger(configuration);

ServiceCollection services = new ServiceCollection();
services
    .Configure<CopyOptions>(configuration.GetSection("Options"))
    .AddLogging(configure => configure.AddSerilog(logger, true))
    .AddSingleton<FileService>()
    .AddSingleton<Application>();

using ServiceProvider provider = services.BuildServiceProvider(true);
Application? application = provider.GetService<Application>();
application?.Run();
