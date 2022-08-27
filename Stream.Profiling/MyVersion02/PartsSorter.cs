using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stream.Profiling.MyVersion02
{
    class PartsSorter
    {
        readonly string fileName;

        readonly int filesCount;

        readonly int expectedPartLinesCount;

        public PartsSorter(string fileName, int filesCount, int expectedPartLinesCount)
        {
            this.fileName = fileName;
            this.filesCount = filesCount;
            this.expectedPartLinesCount = expectedPartLinesCount;
        }

        public List<string> Split()
        {
            var partFileNames = new List<string>();
            var writers = new List<StreamWriter>();

            foreach (var index in Enumerable.Range(0, filesCount))
            {
                var partFileName = $"my2_{index + 1}.txt";
                partFileNames.Add(partFileName);
                writers.Add(new StreamWriter(partFileName));
            }

            var tempLine = new Line();
            using (var reader = new StreamReader(fileName))
            {
                while(true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;

                    tempLine.Update(line);
                    var fileIndex = tempLine.GetLineHash() % filesCount;
                    writers[fileIndex].WriteLine(line);
                }
            }

            foreach (var writer in writers)
                writer.Dispose();

            return partFileNames;
        }

        public void SortParts(List<string> partFileNames)
        {
            var storage = new Storage(expectedPartLinesCount);
            foreach (var partFileName in partFileNames)
            {
                storage.Store(partFileName);
                Array.Sort(storage.Data, 0, storage.Count);
                using(var writer = new StreamWriter(partFileName, false))
                {
                    var arr = storage.Data;
                    var count = storage.Count;
                    for (var i = 0; i < count; i++)
                        writer.WriteLine(arr[i].TotalLine);
                }
            }
        }
    }
}