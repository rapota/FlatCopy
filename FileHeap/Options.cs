using CommandLine;
using CommandLine.Text;

namespace FileHeap
{
	public class Options
	{
		[Option('o', "output", Required = true, HelpText = "Target folder.")]
		public string TargetFolder { get; set; }

		[Option('i', "input", Required = true, HelpText = "Source folder.")]
		public string SourceFolder { get; set; }

		[Option('p', "pattern", DefaultValue = "*.*", HelpText = "Search pattern.")]
		public string SearchPattern { get; set; }

		[Option('a', "async", DefaultValue = false, HelpText = "Asynchronous processing.")]
		public bool IsAsync { get; set; }

		[Option('m', "multi", DefaultValue = false, HelpText = "parallel processing.")]
		public bool IsParalell { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}