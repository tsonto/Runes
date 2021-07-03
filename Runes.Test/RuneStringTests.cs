using FluentAssertions;
using Xunit;

namespace Tsonto.System.Text.Test
{
    public class RuneStringTests
    {
        [Theory]
        [InlineData("", "", 0)]
        [InlineData("a", "a", 0)]
        [InlineData("a", "b", -1)]
        [InlineData("b", "a", 1)]
        [InlineData("aa", "aaa", -1)]
        [InlineData("aaa", "aa", 1)]
        [InlineData("\xD83D\xDC7D", "\xD83D\xDC7D", 0)]
        [InlineData("\xD83D\xDC7D", "\xFFFD\xFFFD", 1)]
        public void CompareTo(string a, string b, int expected)
        {
            var ra = new RuneString(a);
            var rb = new RuneString(b);
            var actual = ra.CompareTo(rb);
            if (expected == 0)
                actual.Should().Be(0);
            else if (expected == 1)
                actual.Should().BeGreaterThan(0);
            else
                actual.Should().BeLessThan(0);
        }

        [Theory]
        [InlineData("")]
        [InlineData("abcd")]
        [InlineData("\xD83D\xDC7D")]
        [InlineData("a\xD83D\xDC7Db")]
        [InlineData("a\x0303\x0306")]
        [InlineData("a\x0306\x0303")]
        public void Constructor_FromString_RoundTrips(string sample)
        {
            var actual = new RuneString(sample).ToString();
            actual.Should().Be(sample);
        }

        [Fact]
        public void Length()
        {
            new RuneString("a\xD83D\xDC7Db").Length.Should().Be(3);
        }

        [Fact]
        public void IndexerFromStart()
        {
            new RuneString("a\xD83D\xDC7Dbc")[1].Value.Should().Be(0x1F47D);
        }

        [Fact]
        public void IndexerFromEnd()
        {
            new RuneString("a\xD83D\xDC7Dbc")[^3].Value.Should().Be(0x1F47D);
        }

        [Fact] 
        public void Concat()
        {
            var a = new RuneString("abc");
            var b = new RuneString("");
            var c = new RuneString("de");
            var actual = RuneString.Concat(a, b, c);
            actual.ToString().Should().Be("abcde");
        }
    }
}
