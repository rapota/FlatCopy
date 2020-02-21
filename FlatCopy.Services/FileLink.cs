namespace FileHeap
{
	public class FileLink
	{
		public FileLink(string source, string target)
		{
			Source = source;
			Target = target;
		}

		public string Source { get; }

		public string Target { get; }
	}
}