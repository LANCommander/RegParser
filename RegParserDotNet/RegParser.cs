using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RegParserDotNet
{
    public class RegParser
    {
        public RegParser() { }

        public IEnumerable<RegistryEntry> Parse(string contents)
        {
            var keys = new List<RegistryEntry>();

            string pathPattern = @"^\[(?<Path>.+)\]$";

            using (var reader = new StringReader(contents))
            {
                string line;
                string path = "";
                string pathContents = "";

                do
                {
                    line = reader.ReadLine();

                    if (line != null)
                    {
                        var pathMatch = Regex.Match(line, pathPattern);

                        if (pathMatch.Success)
                        {
                            path = pathMatch.Groups["Path"].Value;

                            keys.Add(new RegistryEntry(path));

                            keys.AddRange(ParsePathContents(pathContents, path));

                            pathContents = "";

                            continue;
                        }
                    }
                    else
                        keys.AddRange(ParsePathContents(pathContents, path));

                    pathContents += line + "\n";
                }
                while (line != null);                
            }

            return keys;
        }

        private IEnumerable<RegistryEntry> ParsePathContents(string pathContents, string path)
        {
            string propertyNamePattern = @"(?>(?<DefaultProperty>@)=|(?<!\\)""(?<PropertyName>.+)(?<!\\)""=)";
            string dwordValuePattern = @"dword:(?<DwordValue>[0-9a-fA-F]+)$";
            string stringValuePattern = @"(?!=\\)""(?<StringValue>.*)(?!=\\)""\s*$";
            string binaryValuePattern = @"hex:(?<BinaryValue>(?:[0-9a-fA-F]{2},?)+\\(?:\n\s*(?:[0-9a-fA-F]{2},?)+\\?)*)";

            var fullPattern = $"^{propertyNamePattern}(?>{dwordValuePattern}|{stringValuePattern}|{binaryValuePattern})";

            var keys = new List<RegistryEntry>();

            var matches = Regex.Matches(pathContents, fullPattern, RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                keys.Add(GetEntryFromMatch(match, path));
            }

            return keys;
        }

        private RegistryEntry GetEntryFromMatch(Match match, string path)
        {
            var entry = new RegistryEntry(path)
            {
                Type = GetTypeFromMatch(match),
                Property = Regex.Unescape(match.Groups["PropertyName"].Value)
            };

            if (match.Groups["DefaultProperty"].Success)
                entry.Property = match.Groups["DefaultProperty"].Value;

            switch (entry.Type)
            {
                case RegistryValueType.REG_DWORD:
                    entry.Value = Int32.Parse(match.Groups["DwordValue"].Value, NumberStyles.HexNumber);
                    break;

                case RegistryValueType.REG_BINARY:
                    entry.Value = HexToBytes(match.Groups["BinaryValue"].Value);
                    break;

                case RegistryValueType.REG_SZ:
                    entry.Value = Regex.Unescape(match.Groups["StringValue"].Value);
                    break;
            }

            return entry;
        }

        private RegistryValueType GetTypeFromMatch(Match match)
        {

            var groupNames = new string[] {
                "DwordValue",
                "StringValue",
                "BinaryValue"
            };

            foreach (var groupName in groupNames)
            {
                if (match.Groups[groupName].Success)
                {
                    switch (groupName)
                    {
                        case "DwordValue":
                            return RegistryValueType.REG_DWORD;

                        case "StringValue":
                            return RegistryValueType.REG_SZ;

                        case "BinaryValue":
                            return RegistryValueType.REG_BINARY;
                    }
                }
            }

            return RegistryValueType.REG_NONE;
        }

        private readonly static Dictionary<char, byte> HexMap = new Dictionary<char, byte>()
        {
            { 'a', 0xA },{ 'b', 0xB },{ 'c', 0xC },{ 'd', 0xD },
            { 'e', 0xE },{ 'f', 0xF },{ 'A', 0xA },{ 'B', 0xB },
            { 'C', 0xC },{ 'D', 0xD },{ 'E', 0xE },{ 'F', 0xF },
            { '0', 0x0 },{ '1', 0x1 },{ '2', 0x2 },{ '3', 0x3 },
            { '4', 0x4 },{ '5', 0x5 },{ '6', 0x6 },{ '7', 0x7 },
            { '8', 0x8 },{ '9', 0x9 }
        };

        private byte[] HexToBytes(string hex)
        {
            if (String.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Input can't be empty");

            // Sanitize
            hex = hex.Replace("0x", "").Replace(",", "").Replace("\\", "").Replace("\n", "").Replace(" ", "");

            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must be an even number of characters");

            var bytes = new byte[hex.Length / 2];

            char left;
            char right;

            try
            {
                int byteIndex = 0;

                for (int i = 0; i < hex.Length; i += 2, byteIndex++)
                {
                    left = hex[i];
                    right = hex[i + 1];

                    bytes[byteIndex] = (byte)((HexMap[left] << 4) | HexMap[right]);
                }

                return bytes;
            }
            catch (KeyNotFoundException)
            {
                throw new FormatException("Invalid hex string");
            }
        }
    }
}
