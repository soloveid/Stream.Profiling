namespace Stream.Profiling.Common
{
    class SortResult
    {
        public string[] SortedBatchFiles { get; }
        public string SortedResultFile { get; }

        public SortResult(string[] sortedBatchFiles, string sortedResultFile)
        {
            SortedBatchFiles = sortedBatchFiles;
            SortedResultFile = sortedResultFile;
        }
    }
}