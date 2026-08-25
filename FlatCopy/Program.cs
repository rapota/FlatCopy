using FlatCopy;
using FlatCopy.FileSystemServices;
using FlatCopy.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

Logger logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Scope} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

IConfigurationRoot configuration = ProgramExtensions.BuildConfiguration(args);
IConfigurationSection optionsSection = configuration.GetSection("Settings");

IServiceCollection services = new ServiceCollection();
services
    .Configure<CopySettings>(optionsSection)
    .AddLogging(configure => configure.AddSerilog(logger, true))
    .AddFileSystemServices()
    .AddSingleton<Application>();

using ServiceProvider provider = services.BuildServiceProvider(true);
Application application = provider.GetRequiredService<Application>();
application.Run();
