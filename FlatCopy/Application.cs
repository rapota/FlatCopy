using FlatCopy.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace FlatCopy;

public sealed class Application(
    IOptions<CopyOptions> _options,
    IFlatCopyService _flatCopyService,
    ILogger<Application> _logger)
{
    public void Run()
    {
        List<FlatCopyParams> flatCopyParamsList = BuildTasks(_options.Value);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("{count} source folders to copy.", flatCopyParamsList.Count);
            for (int i = 0; i < flatCopyParamsList.Count; i++)
            {
                _logger.LogInformation("Source folder #{i}: {folders}", i + 1, flatCopyParamsList[i].SearchParams.SourceFolder);
            }
        }

        Stopwatch sw = Stopwatch.StartNew();
        List<string> copiedFiles = CopyFiles(flatCopyParamsList);
        sw.Stop();

        Stopwatch swd = Stopwatch.StartNew();
        long count = _flatCopyService.DeleteExtraFiles(copiedFiles, _options.Value.TargetFolder, _options.Value.SearchPattern);
        swd.Stop();

        _logger.LogInformation("Processed {count} files for {elapsed}", copiedFiles.Count, sw.Elapsed);
        _logger.LogInformation("Deleted {count} extra files for {elapsed}", count, swd.Elapsed);
    }

    private static List<FlatCopyParams> BuildTasks(CopyOptions copyOptions)
    {
        List<FlatCopyParams> result = new();
        foreach (KeyValuePair<string, CopySource> copySource in copyOptions.Sources)
        {
            FlatCopyParams flatCopyParams = FlatCopyParamsHelper.ToFlatCopyParams(copySource.Key, copyOptions, copySource.Value);
            result.Add(flatCopyParams);
        }

        CopyParams copyParams = new(copyOptions.CreateHardLinks, copyOptions.Overwrite);
        foreach (string sourceFolder in copyOptions.SourceFolders)
        {
            SearchParams searchParams = new(sourceFolder, copyOptions.SearchPattern, copyOptions.SkipExtensions, [], []);

            string path = Path.TrimEndingDirectorySeparator(sourceFolder);
            string fileName = Path.GetFileName(path);
            FlatCopyParams flatCopyParams = new(fileName, copyParams, searchParams, copyOptions.TargetFolder);

            result.Add(flatCopyParams);
        }

        return result;
    }

    private List<string> CopyFiles(IEnumerable<FlatCopyParams> flatCopyParamsList)
    {
        List<string> result = new(100000);
        foreach (FlatCopyParams flatCopyParams in flatCopyParamsList)
        {
            using IDisposable? scope = _logger.BeginScope(flatCopyParams.SearchParams.SourceFolder);

            List<string> flatCopy = _flatCopyService.FlatCopy(flatCopyParams);
            _logger.LogInformation("Copied {count} files.", flatCopy.Count);

            result.AddRange(flatCopy);
        }

        return result;
    }
}