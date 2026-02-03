// Тесты к заданию 1

namespace Tests
{
    public class StringCompressorTests
    {
        [Theory]
        [InlineData("aaabbcccdde", "a3b2c3d2e")]
        [InlineData("abc", "abc")]
        [InlineData("a", "a")]
        [InlineData("", "")]
        [InlineData("aaaaa", "a5")]
        [InlineData("aaabbbccc", "a3b3c3")]
        [InlineData("abbbccccd", "ab3c4d")]
        [InlineData("xyz", "xyz")]
        [InlineData("aaabba", "a3b2a")]
        public void Compress_ValidInput_ReturnsCompressedString(string input, string expected)
        {
            var result = StringCompression.Compress(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("a3b2c3d2e", "aaabbcccdde")]
        [InlineData("abc", "abc")]
        [InlineData("a", "a")]
        [InlineData("", "")]
        [InlineData("a5", "aaaaa")]
        [InlineData("a3b3c3", "aaabbbccc")]
        [InlineData("ab3c4d", "abbbccccd")]
        [InlineData("xyz", "xyz")]
        [InlineData("a3b2a", "aaabba")]
        public void Decompress_ValidInput_ReturnsOriginalString(string compressed, string expected)
        {
            var result = StringCompression.Decompress(compressed);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("aaabbcccdde")]
        [InlineData("abc")]
        [InlineData("a")]
        [InlineData("")]
        [InlineData("aaaaa")]
        [InlineData("aaabbbccc")]
        [InlineData("abbbccccd")]
        [InlineData("xyz")]
        [InlineData("aaabba")]
        public void CompressAndDecompress_RoundTrip_ReturnsOriginalString(string original)
        {
            var compressed = StringCompression.Compress(original);
            var decompressed = StringCompression.Decompress(compressed);

            Assert.Equal(original, decompressed);
        }

        [Fact]
        public void Compress_Null_ReturnsNull()
        {
            var result = StringCompression.Compress(null);

            Assert.Null(result);
        }

        [Fact]
        public void Decompress_Null_ReturnsNull()
        {
            var result = StringCompression.Decompress(null);

            Assert.Null(result);
        }
    }
}

