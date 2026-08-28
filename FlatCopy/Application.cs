using FlatCopy.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using FlatCopy.FileSystemServices;

namespace FlatCopy;

public sealed class Application(
    IOptions<CopySettings> _options,
    IFlatCopyService _flatCopyService,
    ILogger<Application> _logger)
{
    public void Run()
    {
        List<FlatCopyParams> flatCopyParamsList = _options.Value.BuildTasks();
        LogParameters(flatCopyParamsList);

        Stopwatch sw = Stopwatch.StartNew();
        List<string> copiedFiles = CopyFiles(flatCopyParamsList);
        sw.Stop();

        Stopwatch swd = Stopwatch.StartNew();
        long count = _flatCopyService.DeleteExtraFiles(copiedFiles, _options.Value.TargetFolder, _options.Value.SearchPattern);
        swd.Stop();

        _logger.LogInformation("Processed {count} files for {elapsed}", copiedFiles.Count, sw.Elapsed);
        _logger.LogInformation("Deleted {count} extra files for {elapsed}", count, swd.Elapsed);
    }

    private void LogParameters(List<FlatCopyParams> flatCopyParamsList)
    {
        if (!_logger.IsEnabled(LogLevel.Information))
        {
            return;
        }

        _logger.LogInformation("{count} source folders to copy.", flatCopyParamsList.Count);

        int i = 0;
        foreach (FlatCopyParams flatCopyParams in flatCopyParamsList)
        {
            i++;

            SearchParams searchParams = flatCopyParams.SearchParams;
            StringBuilder sb = new(searchParams.QueryParams.SearchPath);

            if (searchParams.SkipSubFolders.Length > 0)
            {
                sb.Append(" ");
                sb.AppendJoin(' ', searchParams.SkipSubFolders.Select(x => $"-'{x}'"));
            }

            if (searchParams.SubFoldersOnly.Length > 0)
            {
                sb.Append(" ");
                sb.AppendJoin(' ', searchParams.SubFoldersOnly.Select(x => $"+'{x}'"));
            }

            _logger.LogInformation("Source folder #{i}: {folders}", i, sb.ToString());
        }
    }

    private List<string> CopyFiles(IEnumerable<FlatCopyParams> flatCopyParamsList)
    {
        List<string> result = new(100000);
        foreach (FlatCopyParams flatCopyParams in flatCopyParamsList)
        {
            using IDisposable? scope = _logger.BeginScope(flatCopyParams.SearchParams.QueryParams.SearchPath);

            List<string> flatCopy = _flatCopyService.FlatCopy(flatCopyParams);
            _logger.LogInformation("Copied {count} files.", flatCopy.Count);

            result.AddRange(flatCopy);
        }

        return result;
    }
}