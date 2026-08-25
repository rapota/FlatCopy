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
        List<FlatCopyParams> flatCopyParamsList = BuildTasks(_options.Value);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("{count} source folders to copy.", flatCopyParamsList.Count);

            int i = 0;
            foreach (FlatCopyParams flatCopyParams in flatCopyParamsList)
            {
                i++;

                SearchParams searchParams = flatCopyParams.SearchParams;
                StringBuilder sb = new(searchParams.SourceFolder);

                if (searchParams.SkipSubFolders.Length > 0)
                {
                    sb.Append(" ");
                    sb.AppendJoin(' ', searchParams.SkipSubFolders.Select(x => "-" + x));
                }

                if (searchParams.SubFoldersOnly.Length > 0)
                {
                    sb.Append(" ");
                    sb.AppendJoin(' ', searchParams.SubFoldersOnly.Select(x => "+" + x));
                }

                _logger.LogInformation("Source folder #{i}: {folders}", i, sb.ToString());
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

    private static List<FlatCopyParams> BuildTasks(CopySettings copySettings)
    {
        List<FlatCopyParams> result = new();
        foreach (KeyValuePair<string, SourceSettings> copySource in copySettings.Sources)
        {
            FlatCopyParams flatCopyParams = FlatCopyParamsHelper.ToFlatCopyParams(copySource.Key, copySettings, copySource.Value);
            result.Add(flatCopyParams);
        }

        CopyParams copyParams = new(copySettings.CreateHardLinks, FlatCopyParamsHelper.ToOverwriteParams(copySettings.Overwrite));
        foreach (string sourceFolder in copySettings.SourceFolders)
        {
            SearchParams searchParams = new(sourceFolder, copySettings.SearchPattern, copySettings.SkipExtensions, [], []);

            string path = Path.TrimEndingDirectorySeparator(sourceFolder);
            string fileName = Path.GetFileName(path);
            FlatCopyParams flatCopyParams = new(fileName, copyParams, searchParams, copySettings.TargetFolder);

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