using CommandLine;

namespace FileHeap
{
	public class Options
	{
		[Option('o', "output", Required = true, HelpText = "Target folder.")]
		public string TargetFolder { get; set; }

		[Option('i', "input", Required = true, HelpText = "Source folder.")]
		public string SourceFolder { get; set; }

		[Option('p', "pattern", Default = "*.*", HelpText = "Search pattern.")]
		public string SearchPattern { get; set; }

		[Option('a', "async", Default = false, HelpText = "Asynchronous processing.")]
		public bool IsAsync { get; set; }

		[Option('m', "multi", Default = false, HelpText = "parallel processing.")]
		public bool IsParalell { get; set; }
	}
}