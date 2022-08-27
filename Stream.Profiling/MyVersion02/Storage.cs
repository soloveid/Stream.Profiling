using System;
using System.IO;

namespace Stream.Profiling.MyVersion02
{
    class Storage
    {
        Line[] data;

        public Storage(int expectedRowsCount)
        {
            data = new Line[expectedRowsCount];
            for (var i = 0; i < data.Length; i++)
                data[i] = new Line();
        }

        public Line[] Data
        {
            get { return data; }
        }

        public int Count { get; private set; }

        public void Store(string fileName)
        {
            Count = 0;
            using (var reader = new StreamReader(fileName))
            {
                var i = 0;
                while (true)
                {
                    for (; i < data.Length; i++)
                    {
                        var line = reader.ReadLine();
                        if (line == null)
                            break;

                        data[i].Update(line);
                    }

                    var addedCount = i;
                    if (addedCount != data.Length || reader.EndOfStream)
                        break;

                    Array.Resize(ref data, (int) (1.2 * data.Length));
                }

                Count = i;
            }
        }
    }
}
