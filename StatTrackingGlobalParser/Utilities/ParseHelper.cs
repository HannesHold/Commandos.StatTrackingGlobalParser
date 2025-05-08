using System.IO;
using System.Text;

namespace StatTrackingGlobalParser.Utilities;

public static class ParseHelper
{
    public static IEnumerable<(int First, int Second)> GetAreaOfInterest(byte[]? source, string startPattern, string endPattern)
    {
        if (source is null || source.Length == 0) { throw new InvalidDataException($"Content is empty."); }

        var startIndexes = new List<int>();
        var endIndexes = new List<int>();

        foreach (int startIndex in ParseHelper.PatternAt(source, Encoding.UTF8.GetBytes(startPattern)))
        {
            startIndexes.Add(startIndex);
        }

        foreach (int endIndex in ParseHelper.PatternAt(source, Encoding.UTF8.GetBytes(endPattern)))
        {
            endIndexes.Add(endIndex);
        }

        return startIndexes.Zip(endIndexes);
    }

    public static IEnumerable<int> PatternAt(byte[] source, byte[] pattern)
    {
        for (int i = 0; i < source.Length; i++)
        {
            if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
            {
                yield return i;
            }
        }
    }

    public static string ParseNameFromStartIndex(byte[] source, int startIndex, bool direction)
    {
        var firstStringCharacterFound = false;
        var currentIndex = direction ? startIndex : startIndex - 1;
        var characters = new List<byte>();

        while (true)
        {
            // Skip some non-printable character values
            if (!firstStringCharacterFound && source[currentIndex] == 0x00 || 
                !firstStringCharacterFound && source[currentIndex] == 0x0D ||
                !firstStringCharacterFound && source[currentIndex] == 0x0F)
            {
                if (direction)
                {
                    currentIndex++;
                }
                else
                {
                    currentIndex--;
                }
               
                continue;
            }

            // Skip some non-printable character values
            if (firstStringCharacterFound && source[currentIndex] == 0x00 || 
                firstStringCharacterFound && source[currentIndex] == 0x0D ||
                firstStringCharacterFound && source[currentIndex] == 0x0F)
            {
                break;
            }

            characters.Add(source[currentIndex]);
            firstStringCharacterFound = true;

            if (direction)
            {
                currentIndex++;
            }
            else
            {
                currentIndex--;
            }
        }

        if (!direction) { characters.Reverse(); }
        return Encoding.UTF8.GetString([.. characters]);
    }
}