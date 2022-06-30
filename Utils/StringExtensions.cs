using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatCommands.Utils
{
    public static class StringExtensions
    {
        public static IEnumerable<string> ParseArguments(this string line, char delimiter = ' ', params char[] textQualifier)
        {
            if (string.IsNullOrWhiteSpace(line))
                yield break;

            var lastQualifier = '\0';
            var inString = false;
            var token = new StringBuilder();

            for (var i = 0; i < line.Length; i++)
            {
                var currentChar = line[i];

                var prevChar = '\0';
                if (i > 0)
                    prevChar = line[i - 1];
                else
                    prevChar = '\0';

                var nextChar = '\0';
                if (i + 1 < line.Length)
                    nextChar = line[i + 1];
                else
                    nextChar = '\0';

                if (textQualifier.Contains(currentChar) && (prevChar == '\0' || prevChar == delimiter) && !inString)
                {
                    inString = true;
                    lastQualifier = currentChar;
                    continue;
                }

                if (currentChar == lastQualifier && (nextChar == '\0' || nextChar == delimiter) && inString)
                {
                    inString = false;
                    continue;
                }

                if (currentChar == delimiter && !inString)
                {
                    yield return token.ToString();
                    token = token.Remove(0, token.Length);
                    continue;
                }

                token = token.Append(currentChar);
            }

            yield return token.ToString();
        }

        public static IEnumerable<string> ParseArguments(this string line)
        {
            return line.ParseArguments(' ', '"', '\'');
        }
    }
}