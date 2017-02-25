using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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
				Run(options);
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

		private static void Run(Options options)
		{
			string mode;
			if (options.IsAsync)
			{
				mode = "async";
			}
			else if (options.IsParalell)
			{
				mode = "parallel";
			}
			else
			{
				mode = "normal";
			}

			logger.Info("Creating hard links fo folder '{0}' at '{1}' with {2} mode.", options.SourceFolder, options.TargetFolder, mode);

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
				long count;
				var sw = Stopwatch.StartNew();
				if (options.IsAsync)
				{
					count = RunAsync(options);
				}
				else if (options.IsParalell)
				{
					count = RunParallel(options);
				}
				else
				{
					count = RunNormal(options);
				}

				sw.Stop();

				long seconds = sw.ElapsedMilliseconds / 1000;
				long ips = seconds > 0
					? count / seconds
					: count;

				logger.Info(
					"Created {0} hard links in {1} ms. {2} IPS.",
					count,
					sw.ElapsedMilliseconds,
					ips);
			}
			catch (Win32Exception ex)
			{
				logger.Error(ex, "Failed to create hard links.");
			}
			catch (IOException ex)
			{
				logger.Error(ex, "Invalid source path.");
			}
			catch (AggregateException ex)
			{
				logger.Error(ex, "Failed to create hard links.");
			}
		}

		private static long RunNormal(Options options)
		{
			long result = 0;
			foreach (FileLink fileLink in GetFileLinks(options.SourceFolder, options.TargetFolder, options.SearchPattern))
			{
				CreateHardLink(fileLink);
				result++;
			}

			return result;
		}

		private static long RunAsync(Options options)
		{
			using (Task<long> task = CreateLinksAsync(options))
			{
				return task.Result;
			}
		}

		private static long RunParallel(Options options)
		{
			long result = 0;

			GetFileLinks(options.SourceFolder, options.TargetFolder, options.SearchPattern)
				.AsParallel()
				.ForAll(fileLink =>
				{
					CreateHardLink(fileLink);
					Interlocked.Increment(ref result);
				});

			return result;
		}

		/// <summary>
		/// Creates all links.
		/// </summary>
		/// <param name="options"></param>
		/// <returns>Count of created hard links.</returns>
		/// <exception cref="Win32Exception">In case of create hard link problem.</exception>
		/// <exception cref="IOException">Thrown in case of invalid source path.</exception>
		private static async Task<long> CreateLinksAsync(Options options)
		{
			long result = 0;
			foreach (FileLink fileLink in GetFileLinks(options.SourceFolder, options.TargetFolder, options.SearchPattern))
			{
				await CreateHardLinkAsync(fileLink);
				result++;
			}

			return result;
		}

		private static IEnumerable<FileLink> GetFileLinks(string sourceFolder, string targetFolder, string pattern)
		{
			foreach (string sourceFile in Directory.EnumerateFiles(sourceFolder, pattern, SearchOption.AllDirectories))
			{
				string relativeName = sourceFile.Remove(0, sourceFolder.Length);

				string normilizedName = relativeName
					.TrimStart(Path.DirectorySeparatorChar)
					.Replace(Path.DirectorySeparatorChar, '_');

				string targetFile = Path.Combine(targetFolder, normilizedName);
				if (File.Exists(targetFile))
				{
					continue;
				}

				yield return new FileLink(sourceFile, targetFile);
			}
		}

		private static void CreateHardLink(FileLink fileLink)
		{
			logger.Trace("Processing file: {0}.", fileLink.Source);
			FileManagementFunctions.CreateHardLink(fileLink.Target, fileLink.Source);
		}

		private static Task CreateHardLinkAsync(FileLink fileLink)
		{
			logger.Trace("Processing file: {0}.", fileLink.Source);
			return FileManagementFunctions.CreateHardLinkAsync(fileLink.Target, fileLink.Source);
		}
	}
}
