using System;
using System.Diagnostics;
using System.IO;
using Stream.Profiling.Common;
using Stream.Profiling.MyVersion01;
using Stream.Profiling.PodkolzzzinVersion;

namespace Stream.Profiling
{
    class Program
    {
        static void Main(string[] args)
        {
            var totalLinesCount = 2_000_000;
            var partLinesCount = 200_000;
            var allLinesFileName = $"L{totalLinesCount}.txt";
            var sortedResultFilePodkolzin = "resultPodkolzin.txt";
            var sortedResultFileMyVersion01 = "resultMyVersion01.txt";

            foreach (var file in Directory.EnumerateFiles(".", "*.txt"))
                File.Delete(file);

            var generateWatch = Stopwatch.StartNew();
            new Generator(allLinesFileName).Generate(totalLinesCount);
            Console.WriteLine($"Generate elapsed: {generateWatch.ElapsedMilliseconds:N} ms");
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Podkolzin version:");
            GC.Collect(2, GCCollectionMode.Forced);
            GC.Collect(2, GCCollectionMode.Forced);
            var files1 = RunPodkolzinVersion(allLinesFileName, partLinesCount, sortedResultFilePodkolzin);

            Console.WriteLine();
            Console.WriteLine("My version 01:");
            GC.Collect(2, GCCollectionMode.Forced);
            GC.Collect(2, GCCollectionMode.Forced);
            var files2 = RunMyVersion01(allLinesFileName, partLinesCount, sortedResultFileMyVersion01);
            GC.Collect(2, GCCollectionMode.Forced);
            GC.Collect(2, GCCollectionMode.Forced);

            CompareResults(new SortResult(files1, sortedResultFilePodkolzin), new SortResult(files2, sortedResultFileMyVersion01));
        }

        static string[] RunPodkolzinVersion(string fileName, int batchLinesCount, string resultFileName)
        {
            var totalStopWatch = Stopwatch.StartNew();

            var sorter = new Sorter();
            var sortWatch = Stopwatch.StartNew();
            var files = sorter.SplitFile(fileName, batchLinesCount);
            Console.WriteLine($"Sorting elapsed: {sortWatch.ElapsedMilliseconds:N} ms");

            var mergeWatch = Stopwatch.StartNew();
            sorter.SortResult(files, resultFileName);
            Console.WriteLine($"Merging elapsed: {mergeWatch.ElapsedMilliseconds:N} ms");

            Console.WriteLine($"Total elapsed: {totalStopWatch.ElapsedMilliseconds:N} ms");

            return files;
        }

        static string[] RunMyVersion01(string fileName, int batchLinesCount, string resultFileName)
        {
            var totalStopWatch = Stopwatch.StartNew();

            var stopWatch = Stopwatch.StartNew();
            var files = new PartsSorter(fileName, batchLinesCount).Sort();
            Console.WriteLine($"Parts sorting: elapsed {stopWatch.ElapsedMilliseconds:N} ms");

            stopWatch.Restart();
            new PartsMerger(resultFileName, files).Merge();
            Console.WriteLine($"Parts merging: elapsed {stopWatch.ElapsedMilliseconds:N} ms");

            Console.WriteLine($"Total elapsed {totalStopWatch.ElapsedMilliseconds:N} ms");

            Console.WriteLine();

            return files.ToArray();
        }

        static void CompareResults(SortResult etalonResult, SortResult result1)
        {
            if (etalonResult.SortedBatchFiles.Length != result1.SortedBatchFiles.Length)
            {
                Console.WriteLine("Количество батчей отличается");
                throw new InvalidProgramException();
            }

            for (var i = 0; i < etalonResult.SortedBatchFiles.Length; i++)
            {
                var file1Context = File.ReadAllText(etalonResult.SortedBatchFiles[i]);
                var file2Context = File.ReadAllText(result1.SortedBatchFiles[i]);

                if (file1Context != file2Context)
                {
                    Console.WriteLine($"Содержимое батчей {etalonResult.SortedBatchFiles[i]} и {result1.SortedBatchFiles[i]} различно");
                    throw new InvalidProgramException();
                }
            }

            var file1Lines = File.ReadAllLines(etalonResult.SortedResultFile);
            var file2Lines = File.ReadAllLines(result1.SortedResultFile);

            if (file1Lines.Length != file2Lines.Length)
            {
                Console.WriteLine($"Количество строк в результирующих файлах {etalonResult.SortedResultFile} и {result1.SortedResultFile} различно");
                throw new InvalidProgramException();
            }

            for (var i = 0; i < file1Lines.Length; i++)
            {
                if (file1Lines[i] != file2Lines[i])
                {
                    Console.WriteLine($"Несовпадают строки {i + 1} в результирующих файлах {etalonResult.SortedResultFile} и {result1.SortedResultFile}");
                    Console.WriteLine(file1Lines[i]);
                    Console.WriteLine(" VS ");
                    Console.WriteLine(file2Lines[i]);
                    throw new InvalidProgramException();
                }
            }
        }
    }
}
