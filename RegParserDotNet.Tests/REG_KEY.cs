using Xunit;

namespace RegParserDotNet.Tests
{
    public class REG_KEY
    {
        [Theory]
        [InlineData("[HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\New World Computing]")]
        [InlineData("[HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\New World Computing\\Heroes of Might and Magic® III\\1.0\\[Test] 123$83(8&3^4^]]")]
        public void KeyIsParseable(string input)
        {
            var parser = new RegParser();

            var keys = parser.Parse(input);

            Assert.NotNull(keys);
            Assert.NotEmpty(keys);
            Assert.Single(keys);

            var key = keys.First();

            Assert.Equal(key.Path.Length, input.Length - 2);
            Assert.True(String.IsNullOrWhiteSpace(key.Property));
            Assert.Null(key.Value);
        }
    }
}