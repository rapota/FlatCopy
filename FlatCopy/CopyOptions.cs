namespace FlatCopy
{
    public class CopyOptions
    {
        public string TargetFolder { get; set; }

        public string SourceFolder { get; set; }

        public string SearchPattern { get; set; }

        public OverwriteOption Overwrite { get; set; }

        public bool IsParallel { get; set; }

        public bool CreateHardLinks { get; set; }
    }
}