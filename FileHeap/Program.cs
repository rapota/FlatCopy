using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using NLog;

namespace FileHeap
{
    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;

            Options options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                string help = options.GetUsage();
                logger.Error(help);
            }
            else
            {
                logger.Info("Creating hard links fo folder: {0}", options.SourceFolder);
                using (Task task = Run(options))
                {
                    task.Wait();
                }
            }

            logger.Debug("Application stoped.");
        }

        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                logger.Fatal(ex, "Domain unhandled exception.");
            }
            else
            {
                logger.Fatal("Domain unhandled error: {0}", e.ExceptionObject);
            }

            LogManager.Flush();
        }

        static void TaskSchedulerUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            logger.Error(e.Exception, "Task unhandled exception.");
            LogManager.Flush();
        }

        private static async Task Run(Options options)
        {
            if (!Directory.Exists(options.TargetFolder))
            {
                try
                {
                    Directory.CreateDirectory(options.TargetFolder);
                }
                catch (IOException ex)
                {
                    logger.Error(ex, "Failed to create target folder.");
                    return;
                }
            }

            try
            {
                ulong createdCount = await CreateLinksAsync(options);
                logger.Info("Created {0} hard links.", createdCount);
            }
            catch (Win32Exception ex)
            {
                logger.Error(ex, "Failed to create hard links.");
            }
            catch (IOException ex)
            {
                logger.Error(ex, "Invalid source path.");
            }
        }

        /// <summary>
        /// Creates all links.
        /// </summary>
        /// <param name="options"></param>
        /// <returns>Count of created hard links.</returns>
        /// <exception cref="Win32Exception">In case of create hard link problem.</exception>
        /// <exception cref="IOException">Thrown in case of invalid source path.</exception>
        private static async Task<ulong> CreateLinksAsync(Options options)
        {
            ulong result = 0;
            foreach (string sourceFile in Directory.EnumerateFiles(options.SourceFolder, options.SearchPattern, SearchOption.AllDirectories))
            {
                logger.Trace("Processing file: {0}.", sourceFile);
                string relativeName = sourceFile.Remove(0, options.SourceFolder.Length);

                string normilizedName = relativeName
                    .TrimStart(Path.DirectorySeparatorChar)
                    .Replace(Path.DirectorySeparatorChar, '_');

                string targetFile = Path.Combine(options.TargetFolder, normilizedName);
                if (File.Exists(targetFile))
                {
                    continue;
                }

                await FileManagementFunctions.CreateHardLinkAsync(targetFile, sourceFile);
                result++;
            }

            return result;
        }
    }
}
