using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            string keyPattern = @"^""?(?<Name>.+?|@)""?=""?(?<Value>.+?)""?$";

            using (var reader = new StringReader(contents))
            {
                string line;
                string path = "";

                while ((line = reader.ReadLine()) != null)
                {
                    var pathMatch = Regex.Match(line, pathPattern);

                    if (pathMatch.Success)
                    {
                        path = pathMatch.Groups["Path"].Value;

                        keys.Add(new RegistryEntry(path));

                        continue;
                    }

                    var keyMatch = Regex.Match(line, keyPattern);

                    if (keyMatch.Success)
                    {
                        var name = keyMatch.Groups["Name"].Value;
                        var value = keyMatch.Groups["Value"].Value;

                        // Handle keys that may span multiple lines. Popular with REG_BINARY
                        while (line.EndsWith("\\"))
                        {
                            line = reader.ReadLine();

                            value += line.Trim();
                        }

                        keys.Add(GetKey(path, name, value));
                    }
                }
            }

            return keys;
        }

        private RegistryEntry GetKey(string path, string property, string value)
        {
            property = Regex.Unescape(property); // Remove character escaping

            if (value.StartsWith("dword:"))
                return new RegistryEntry(path, property, RegistryValueType.REG_DWORD, Int32.Parse(value.Replace("dword:", ""), NumberStyles.HexNumber));

            if (value.StartsWith("hex:"))
                return new RegistryEntry(path, property, RegistryValueType.REG_BINARY, HexToBytes(value.Replace("hex:", "")));

            if (value.StartsWith("hex(2):"))
                return new RegistryEntry(path, property, RegistryValueType.REG_EXPAND_SZ, HexToBytes(value.Replace("hex(2):", "")));

            return new RegistryEntry(path, property, RegistryValueType.REG_SZ, Regex.Unescape(value)); // Remove character escaping

            // throw new NotImplementedException("This key type is not supported");
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
            hex = hex.Replace("0x", "").Replace(",", "").Replace("\\", "");

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
