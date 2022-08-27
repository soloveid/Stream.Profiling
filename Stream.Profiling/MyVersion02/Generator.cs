using System;
using System.IO;
using System.Text;

namespace Stream.Profiling.MyVersion02
{
    class Generator
    {
        readonly Random numberRandom;

        readonly Random stringLegthRandom;

        readonly Random letterRandom;

        const int MAX_STRING_LENGTH = 100;

        readonly char[] charArray;

        int GetNextNumber()
        {
            return numberRandom.Next(0, 10000);
        }

        int GenerateNextString()
        {
            var length = stringLegthRandom.Next(20, MAX_STRING_LENGTH);

            for (var i = 0; i < charArray.Length && i < length; i++)
                charArray[i] = (char)letterRandom.Next(65, 90);

            return length;
        }

        public Generator()
        {
            charArray = new char[MAX_STRING_LENGTH];
            numberRandom = new Random(Environment.TickCount);
            stringLegthRandom = new Random(Environment.TickCount / 13);
            letterRandom = new Random(Environment.TickCount / 31);
        }

        public void Generate(string fileName, int totalLinesCount)
        {
            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8, 4 * 1024))
            {
                for (var i = 0; i < totalLinesCount; i++)
                {
                    writer.Write(GetNextNumber());
                    writer.Write(". ");

                    var stringLength = GenerateNextString();
                    writer.Write(charArray, 0, stringLength);
                    writer.WriteLine();
                }
            }
        }
    }
}
