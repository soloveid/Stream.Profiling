using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Stream.Profiling.PodkolzzzinVersion
{
    struct Line : IComparable<Line>
    {
        private int pos;

        private string line;

        public Line(string line)
        {
            pos = line.IndexOf(".");
            Number = int.Parse(line.AsSpan(0, pos));
            this.line = line;
        }

        public string Build() => line;

        public int Number { get; set; }

        public ReadOnlySpan<char> Word => line.AsSpan(pos + 2);

        public int CompareTo(Line other)
        {
            int result = Word.CompareTo(other.Word, StringComparison.Ordinal);
            if (result != 0)
                return result;

            return Number.CompareTo(other.Number);
        }
    }


    class Sorter
    {
        class LineState
        {
            public StreamReader Reader { get; set; }

            public Line Line { get; set; }
        }

        public void SortResult(string[] files, string resultFileName)
        {
            var readers = files.Select(x => new StreamReader(x)).ToArray();
            try
            {
                var lines = readers.Select(x => new LineState
                {
                    Line = new Line(x.ReadLine()),
                    Reader = x
                }).OrderBy(x => x.Line).ToList();

                using var writer = new StreamWriter(resultFileName);
                while (lines.Count > 0)
                {
                    var current = lines[0];
                    writer.WriteLine(current.Line.Build());

                    if (current.Reader.EndOfStream)
                    {
                        lines.Remove(current);
                        continue;
                    }

                    current.Line = new Line(current.Reader.ReadLine());
                    Reorder(lines);
                }
            }
            finally
            {
                foreach (var r in readers)
                    r.Dispose();
            }
        }

        void Reorder(List<LineState> lines)
        {
            if (lines.Count == 1)
                return;

            int i = 0;
            while (lines[i].Line.CompareTo(lines[i + 1].Line) > 0)
            {
                var t = lines[i];
                lines[i] = lines[i + 1];
                lines[i + 1] = t;
                i++;
                if (i + 1 == lines.Count)
                    return;
            }
        }

        public string[] SplitFile(string fileName, int partLinesCount)
        {
            var list = new List<string>();
            int partNumber = 0;
            Line[] lines = new Line[partLinesCount];
            int i = 0;

            using var reader = new StreamReader(fileName);
            for (string line = reader.ReadLine();; line = reader.ReadLine())
            {
                lines[i] = new Line(line);
                i++;
                if (i == partLinesCount)
                {
                    partNumber++;
                    var partFileName = partNumber + ".txt";
                    list.Add(partFileName);
                    Array.Sort(lines);
                    File.WriteAllLines(partFileName, lines.Select(x => x.Build()));
                    i = 0;
                }

                if (reader.EndOfStream)
                    break;
            }

            if (i != 0)
            {
                Array.Resize(ref lines, i);
                var partFileName = partNumber + ".txt";
                list.Add(partFileName);
                File.WriteAllLines(partFileName, lines.Select(x => x.Build()));
            }

            return list.ToArray();
        }
    }
}
