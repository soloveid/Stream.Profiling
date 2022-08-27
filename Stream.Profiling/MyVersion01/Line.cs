using System;

namespace Stream.Profiling.MyVersion01
{
    struct Line : IComparable<Line>
    {
        public string TotalLine;

        int numberLength;

        int stringStartIndex
        {
            get { return numberLength + 2; }
        }

        public void Update(string totalLine)
        {
            TotalLine = totalLine;
            numberLength = (byte)totalLine.IndexOf('.');
        }

        public int CompareTo(Line other)
        {
            var y = other;
            var yTotalLine = y.TotalLine;
            var yStringStartIndex = y.stringStartIndex;
            var yNumberLength = y.numberLength;

            var xStringStartIndex = stringStartIndex;

            var fastCompare = TotalLine[xStringStartIndex] - yTotalLine[yStringStartIndex];
            if (fastCompare != 0)
                return fastCompare;

            var compareResult = CompareTo(TotalLine.AsSpan(xStringStartIndex), yTotalLine.AsSpan(yStringStartIndex));
            if (compareResult != 0)
                return compareResult;

            var diff = numberLength - yNumberLength;
            if (diff != 0)
                return diff;

            return CompareTo(TotalLine.AsSpan(0, numberLength), yTotalLine.AsSpan(0, yNumberLength));
        }

        int CompareTo(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        {
            int diff;
            for (var i = 0; i < a.Length && i < b.Length; i++)
            {
                diff = a[i] - b[i];
                if (diff != 0)
                    return diff;
            }

            return a.Length - b.Length;
        }
    }
}
