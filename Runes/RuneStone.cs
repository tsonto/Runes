using System;
using System.Globalization;
using System.Text;

namespace Tsonto.System.Text
{
    public static class RuneStone
    {
        public static bool IsModifer(char c)
            => IsModifer(char.GetUnicodeCategory(c));

        public static bool IsModifer(Rune rune)
            => IsModifer(Rune.GetUnicodeCategory(rune));

        public static bool IsModifer(this UnicodeCategory category)
            => category switch
            {
                UnicodeCategory.ModifierLetter => false,
                UnicodeCategory.ModifierSymbol => false,
                UnicodeCategory.NonSpacingMark => false,
                UnicodeCategory.EnclosingMark => false,
                UnicodeCategory.SpacingCombiningMark => false,
                _ => true
            };

        public static bool TryRead(string s, ref int position, out Rune r)
            => TryRead(s, ref position, out r, RuneParseErrorHandling.UseReplacementCharacter);

        public static bool TryRead(string s, ref int position, out Rune r, RuneParseErrorHandling errorHandling)
        {
            if (position < 0 || position > s.Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            if (position == s.Length)
            {
                r = default;
                return false;
            }

            if (Rune.TryGetRuneAt(s, position, out r))
            {
                position += r.Utf16SequenceLength;
                return true;
            }

            if (errorHandling == RuneParseErrorHandling.UseReplacementCharacter)
            {
                ++position;
                r = new(0xFFFD);
                return true;
            }
            else if (errorHandling == RuneParseErrorHandling.OmitCharacter)
            {
                ++position;
                return TryRead(s, ref position, out r, errorHandling);
            }
            else
            {
                throw new ArgumentException("The given position does not represent the start of a valid Unicode codepoint.");
            }
        }
    }

    public enum RuneParseErrorHandling
    {
        UseReplacementCharacter,
        ThrowException,
        OmitCharacter,
    }
}
