using Xunit;

namespace RegParserDotNet.Tests
{
    public class REG_DWORD
    {
        public const string Export =
@"[HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\New World Computing\Heroes of Might and Magic® III\1.0]
""Main Game Y""=dword:0000000a";

        [Theory]
        [InlineData(Export)]
        public void PropertyIsParseable(string input)
        {
            var parser = new RegParser();

            var keys = parser.Parse(input);

            Assert.NotNull(keys);
            Assert.NotEmpty(keys);
            Assert.Equal(2, keys.Count());
            Assert.Equal(keys.First().Path, keys.Last().Path);

            var key = keys.Last();

            Assert.Equal(RegistryValueType.REG_DWORD, key.Type);
            Assert.Equal("Main Game Y", key.Property);
            Assert.Equal(0xA, (int)key.Value);
        }
    }
}
