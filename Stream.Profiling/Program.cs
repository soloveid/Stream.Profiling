using System;
using System.Diagnostics;
using System.IO;
using Stream.Profiling.MyVersion01;
using Stream.Profiling.PodkolzzzinVersion;

namespace Stream.Profiling
{
    class Program
    {
        static void Main(string[] args)
        {
            var partLinesCount = 200_000;
            var filesCount = 10;
            var totalLinesCount = filesCount * partLinesCount;
            var allLinesFileName = $"L{totalLinesCount}.txt";
            var sortedResultFilePodkolzin = "resultPodkolzin.txt";
            var sortedResultFileMyVersion01 = "resultMyVersion01.txt";
            var sortedResultFileMyVersion02 = "resultMyVersion02.txt";

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

            Console.WriteLine();
            Console.WriteLine("My version 02:");
            GC.Collect(2, GCCollectionMode.Forced);
            GC.Collect(2, GCCollectionMode.Forced);
            var files3 = RunMyVersion02(allLinesFileName, filesCount, partLinesCount, sortedResultFileMyVersion02);
            GC.Collect(2, GCCollectionMode.Forced);
            GC.Collect(2, GCCollectionMode.Forced);

            CompareResultFiles(sortedResultFilePodkolzin, sortedResultFileMyVersion01);
            CompareBatchResults(files1, files2);

            CompareResultFiles(sortedResultFilePodkolzin, sortedResultFileMyVersion02);
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

        static string[] RunMyVersion02(string fileName, int filesCount, int expectedPartLinesCount, string resultFileName)
        {
            var totalStopWatch = Stopwatch.StartNew();

            var stopWatch = Stopwatch.StartNew();
            var partsSorter = new MyVersion02.PartsSorter(fileName, filesCount, expectedPartLinesCount);
            var files = partsSorter.Split();
            partsSorter.SortParts(files);
            Console.WriteLine($"Parts sorting: elapsed {stopWatch.ElapsedMilliseconds:N} ms");

            stopWatch.Restart();
            new MyVersion02.PartsMerger(resultFileName, files).Merge();
            Console.WriteLine($"Parts merging: elapsed {stopWatch.ElapsedMilliseconds:N} ms");

            Console.WriteLine($"Total elapsed {totalStopWatch.ElapsedMilliseconds:N} ms");

            Console.WriteLine();

            return files.ToArray();
        }

        static void CompareBatchResults(string[] etalonBatches, string[] batches)
        {
            if (etalonBatches.Length != batches.Length)
            {
                Console.WriteLine("Количество батчей отличается");
                throw new InvalidProgramException();
            }

            for (var i = 0; i < etalonBatches.Length; i++)
            {
                var file1Context = File.ReadAllText(etalonBatches[i]);
                var file2Context = File.ReadAllText(batches[i]);

                if (file1Context != file2Context)
                {
                    Console.WriteLine($"Содержимое батчей {etalonBatches[i]} и {batches[i]} различно");
                    throw new InvalidProgramException();
                }
            }
        }

        static void CompareResultFiles(string etalonResult, string result)
        {
            var file1Lines = File.ReadAllLines(etalonResult);
            var file2Lines = File.ReadAllLines(result);

            if (file1Lines.Length != file2Lines.Length)
            {
                Console.WriteLine($"Количество строк в результирующих файлах {etalonResult} и {result} различно");
                throw new InvalidProgramException();
            }

            for (var i = 0; i < file1Lines.Length; i++)
            {
                if (file1Lines[i] != file2Lines[i])
                {
                    Console.WriteLine($"Несовпадают строки {i + 1} в результирующих файлах {etalonResult} и {result}");
                    Console.WriteLine(file1Lines[i]);
                    Console.WriteLine(" VS ");
                    Console.WriteLine(file2Lines[i]);
                    throw new InvalidProgramException();
                }
            }
        }
    }
}
