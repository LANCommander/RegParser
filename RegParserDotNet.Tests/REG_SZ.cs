using Xunit;

namespace RegParserDotNet.Tests
{
    public class REG_SZ
    {
        public const string ExportWithQuotes =
@"[HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\New World Computing\Heroes of Might and Magic® III\1.0]
""Test \""Thing\""""=""With a value!\""Test\""""";

        [Theory]
        [InlineData(ExportWithQuotes)]
        public void PropertyIsParseable(string input)
        {
            var parser = new RegParser();

            var keys = parser.Parse(input);

            Assert.NotNull(keys);
            Assert.NotEmpty(keys);
            Assert.Equal(2, keys.Count());
            Assert.Equal(keys.First().Path, keys.Last().Path);

            var key = keys.Last();

            Assert.Equal(RegistryValueType.REG_SZ, key.Type);
            Assert.Equal("Test \"Thing\"", key.Property);
            Assert.Equal("With a value!\"Test\"", (string)key.Value);
        }
    }
}
