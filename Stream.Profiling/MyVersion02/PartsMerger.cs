using System;
using System.Collections.Generic;
using System.IO;

namespace Stream.Profiling.MyVersion02
{
    class PartsMerger
    {
        class FilePart : IComparable<FilePart>
        {
            public StreamReader Stream { get; }

            public Line Line;

            public string TotalLine
            {
                get { return Line.TotalLine; }
            }

            public FilePart(StreamReader stream, Line line)
            {
                Stream = stream;
                Line = line;
            }

            public int CompareTo(FilePart other)
            {
                return Line.CompareTo(other.Line);
            }

            public void Update(string line)
            {
                Line.Update(line);
            }
        }

        readonly string resultFileName;

        readonly List<string> partFileNames;

        public PartsMerger(string resultFileName, List<string> partFileNames)
        {
            this.resultFileName = resultFileName;
            this.partFileNames = partFileNames;
        }

        public void Merge()
        {
            var partFiles = new FilePart[partFileNames.Count];
            for (var i = 0; i < partFiles.Length; i++)
            {
                var streamReader = new StreamReader(partFileNames[i]);
                var line = new Line();
                line.Update(streamReader.ReadLine());
                partFiles[i] = new FilePart(streamReader, line);
            }

            Array.Sort(partFiles, 0, partFiles.Length);

            using (var streamWriter = new StreamWriter(resultFileName))
            {
                var firstPartIndex = 0;
                while (firstPartIndex < partFiles.Length)
                {
                    var firstPartFile = partFiles[firstPartIndex];
                    streamWriter.WriteLine(firstPartFile.Line.TotalLine);

                    var readedLine = firstPartFile.Stream.ReadLine();
                    if (readedLine != null)
                    {
                        partFiles[firstPartIndex].Line.Update(readedLine);
                        for (var i = firstPartIndex; i < partFiles.Length - 1; i++)
                        {
                            if (partFiles[i].Line.CompareTo(partFiles[i + 1].Line) > 0)
                            {
                                var temp = partFiles[i];
                                partFiles[i] = partFiles[i + 1];
                                partFiles[i + 1] = temp;
                                continue;
                            }
                            break;
                        }
                    }
                    else
                        firstPartIndex++;
                }
            }

            for (var i = 0; i < partFiles.Length; i++)
                partFiles[i].Stream.Dispose();
        }
    }
}