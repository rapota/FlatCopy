using FlatCopy;
using FlatCopy.FileSystem;
using FlatCopy.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

Logger logger = new LoggerConfiguration()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] [{ThreadId}] ({Scope}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

IConfigurationRoot configuration = ProgramExtensions.BuildConfiguration(args);
IConfigurationSection optionsSection = configuration.GetSection("Options");

IServiceCollection services = new ServiceCollection();
services
    .Configure<CopyOptions>(optionsSection)
    .AddLogging(configure => configure.AddSerilog(logger, true))
    .AddSingleton<IFileSystemApi, FileSystemApi>()
    .AddSingleton<IFileCopyService, FileCopyService>()
    .AddSingleton<IDirectoryScannerService, DirectoryScannerService>()
    .AddSingleton<IFlatCopyService, FlatCopyService>()
    .AddSingleton<IDirectoryCopyService, DirectoryCopyService>()
    .AddSingleton<Application>();

using ServiceProvider provider = services.BuildServiceProvider(true);
Application? application = provider.GetService<Application>();
application?.Run();
