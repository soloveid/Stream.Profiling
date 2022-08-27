using System;
using System.Collections.Generic;
using System.IO;

namespace Stream.Profiling.MyVersion01
{
    class PartsSorter
    {
        readonly string fileName;

        readonly int partLinesCount;

        readonly Line[] batchLines;

        public PartsSorter(string fileName, int partLinesCount)
        {
            this.fileName = fileName;
            this.partLinesCount = partLinesCount;
            batchLines = new Line[partLinesCount];
            for (var i = 0; i < batchLines.Length; i++)
                batchLines[i] = new Line();
        }

        int GetBatch(StreamReader reader)
        {
            var batchIndex = 0;
            for (; batchIndex < partLinesCount; batchIndex++)
            {
                var line = reader.ReadLine();
                if (line != null)
                {
                    batchLines[batchIndex].Update(line);
                    continue;
                }

                break;
            }

            return batchIndex;
        }

        public List<string> Sort()
        {
            var partFileNames = new List<string>();

            using (var reader = new StreamReader(fileName))
            {
                for (var batchIndex = 0;; batchIndex++)
                {
                    var batchSize = GetBatch(reader);
                    if (batchSize == 0)
                        break;

                    Array.Sort(batchLines, 0, batchSize);

                    var partFileName = $"my1_{batchIndex + 1}.txt";
                    using (var writer = new StreamWriter(partFileName))
                    {
                        for (var i = 0; i < batchLines.Length && i < batchSize; i++)
                            writer.WriteLine(batchLines[i].TotalLine);
                    }

                    partFileNames.Add(partFileName);
                }
            }

            return partFileNames;
        }
    }
}