using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Tsonto.System.Text.Test
{
	public class RuneStoneTests
	{
		[Theory]
        [InlineData("")]
        [InlineData("a", 0x61)]
        [InlineData("abcd", 0x61, 0x62, 0x63, 0x64)]
        [InlineData("\xD83D\xDC7D", 0x1F47D)]
        [InlineData("a\xD83D\xDC7Db", 0x61, 0x1F47D, 0x62)]
        [InlineData("a\x0303\x0306", 0x61, 0x0303, 0x0306)]
        [InlineData("a\x0306\x0303", 0x61, 0x0306, 0x0303)]
        public void TryRead_Success(string input, params int[] expected)
		{
            int i = 0;
            var actual = new List<int>();
            while (RuneStone.TryRead(input, ref i, out Rune r, RuneParseErrorHandling.ThrowException))
                actual.Add(r.Value);
            actual.Should().BeEquivalentTo(expected);
		}

        [Fact]
        public void TryRead_UnpairedSurrogate_UseReplacementCharacter_ProducesReplacementCharacter()
        {
            string s = "" + (char)0xD83D;
            int i = 0;
            var actual = new List<int>();
            while (RuneStone.TryRead(s, ref i, out Rune r, RuneParseErrorHandling.UseReplacementCharacter))
                actual.Add(r.Value);
            actual.Should().BeEquivalentTo(0xFFFD);
        }

        [Fact]
        public void TryRead_UnpairedSurrogateAmidOtherText_UseReplacementCharacter_ProducesReplacementCharacterAndContinues()
        {
            string s = "a" + (char)0xD83D + "b";
            int i = 0;
            var actual = new List<int>();
            while (RuneStone.TryRead(s, ref i, out Rune r, RuneParseErrorHandling.UseReplacementCharacter))
                actual.Add(r.Value);
            actual.Should().BeEquivalentTo(0x61, 0xFFFD, 0x62);
        }

        [Fact]
        public void TryRead_UnpairedSurrogate_Throw_Throws()
        {
            string s = "" + (char)0xD83D;
            int i = 0;
            Assert.Throws<ArgumentException>(
                () => RuneStone.TryRead(s, ref i, out Rune r, RuneParseErrorHandling.ThrowException));
        }

        [Fact]
        public void TryRead_UnpairedSurrogate_Skip_ReturnsEmpty()
        {
            string s = "" + (char)0xD83D;
            int i = 0;
            var actual = new List<int>();
            while (RuneStone.TryRead(s, ref i, out Rune r, RuneParseErrorHandling.OmitCharacter))
                actual.Add(r.Value);
            actual.Should().BeEmpty();
        }

        [Fact]
        public void TryRead_AdjacentUnpairedSurrogates_Skip_ReturnsEmpty()
        {
            string s = "" + (char)0xD83D + (char)0xD83D + (char)0xD83D;
            int i = 0;
            var actual = new List<int>();
            while (RuneStone.TryRead(s, ref i, out Rune r, RuneParseErrorHandling.OmitCharacter))
                actual.Add(r.Value);
            actual.Should().BeEmpty();
        }

        [Fact]
        public void TryRead_UnpairedSurrogateAmidOtherText_Skip_ReturnsTheOtherText()
        {
            string s = "a" + (char)0xD83D + "b";
            int i = 0;
            var actual = new List<int>();
            while (RuneStone.TryRead(s, ref i, out Rune r, RuneParseErrorHandling.OmitCharacter))
                actual.Add(r.Value);
            actual.Should().BeEquivalentTo(0x61, 0x62);
        }
    }
}
