using System;
using System.Diagnostics;

namespace Stream.Profiling
{
    class Program
    {
        static void Main(string[] args)
        {
            var totalLinesCount = 2_000_000;
            var partLinesCount = 200_000;
            var allLinesFileName = $"L{totalLinesCount}.txt";
            var sortedFileName = "result.txt";

            var sw = Stopwatch.StartNew();
            new Generator(allLinesFileName).Generate(totalLinesCount);
            var sorter = new Sorter();
            var partFiles = sorter.SplitFile(allLinesFileName, partLinesCount);
            sorter.SortResult(partFiles, sortedFileName);
            sw.Stop();
            Console.WriteLine($"Execution took: {sw.Elapsed}");
        }
    }
}
