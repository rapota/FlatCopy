using FlatCopy;
using FlatCopy.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] [{ThreadId}] ({Scope}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

IConfigurationRoot configuration = ProgramExtensions.BuildConfiguration(args);
IConfigurationSection optionsSection = configuration.GetSection("Options");

IServiceCollection services = new ServiceCollection();
services
    .Configure<CopyOptions>(optionsSection)
    .AddLogging(configure => configure.AddSerilog(Log.Logger, true))
    .AddSingleton<FileService>()
    .AddSingleton<Application>();

using ServiceProvider provider = services.BuildServiceProvider(true);
Application? application = provider.GetService<Application>();
application?.Run();
