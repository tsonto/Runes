using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Tsonto.System.Text
{
    /// <summary>
    /// Provides static methods for working with <see cref="Rune"/> s and <see cref="RuneString"/> s.
    /// </summary>
    public static class RuneStone
    {
        /// <summary>
        /// Gets a <see cref="Rune"/> that represents the Unicode replacement character (U+FFFD).
        /// </summary>
        public static readonly Rune ReplacementCharacter = new Rune(0xFFFD);

        /// <summary>
        /// Checks whether the given character is a modifier character, per Unicode category M.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if the character is a modifier character; false otherwise.</returns>
        public static bool IsModifer(char c)
            => IsModifer(char.GetUnicodeCategory(c));

        /// <summary>
        /// Checks whether the given <see cref="Rune"/> is a modifier character, per Unicode category M.
        /// </summary>
        /// <param name="rune">The rune to check.</param>
        /// <returns>True if the rune is a modifier character; false otherwise.</returns>
        public static bool IsModifer(Rune rune)
            => IsModifer(Rune.GetUnicodeCategory(rune));

        /// <summary>
        /// Checks whether the given Unicode category is a Modifier category (a subtype of category M).
        /// </summary>
        /// <param name="category">The category to check.</param>
        /// <returns>True if the category is a modifier category; false otherwise.</returns>
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

        /// <summary>
        /// Attempts to read a rune at the given position in a string, and advances the <paramref name="position"/>
        /// argument to just after that rune.
        /// </summary>
        /// <param name="s">The string to read from.</param>
        /// <param name="position">
        /// The position in the string to read from. 0 means to read from the beginning. This argument will
        /// automatically be advanced to the start of the next rune, or to just past the end of the string if there are
        /// no more runes.
        /// </param>
        /// <param name="r">
        /// If the call returns true, this is the rune read from the string; otherwise the value is character 0. If the
        /// rune at the specified location is invalid, the output is U+FFFD, the Unicode replacement character.
        /// </param>
        /// <returns>
        /// True if the method found a rune. This includes invalid input that returns the replacement character. False
        /// if <paramref name="position"/> is already at the end of the string.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position"/> is less than zero or greater that the string's length.
        /// </exception>
        public static bool TryRead(string s, ref int position, out Rune r)
            => TryRead(s, ref position, out r, RuneParseErrorHandling.UseReplacementCharacter);

        /// <summary>
        /// Attempts to read a rune at the given position in a string, and advances the <paramref name="position"/>
        /// argument to just after that rune.
        /// </summary>
        /// <param name="s">The string to read from.</param>
        /// <param name="position">
        /// The position in the string to read from. 0 means to read from the beginning. This argument will
        /// automatically be advanced to the start of the next rune, or to just past the end of the string if there are
        /// no more runes.
        /// </param>
        /// <param name="r">
        /// If the call returns true, this is the rune read from the string; otherwise the value is character 0. If the
        /// rune at the specified location is invalid and <paramref name="errorHandling"/> is <see
        /// cref="RuneParseErrorHandling.UseReplacementCharacter"/>, the output is U+FFFD, the Unicode replacement
        /// character. If the rune at the specified location is invalid and <paramref name="errorHandling"/> is <see
        /// cref="RuneParseErrorHandling.OmitCharacter"/>, the output is the next valid rune found.
        /// </param>
        /// <param name="errorHandling">
        /// Specifies what action the method should take if it encounters an invalid Unicode sequence.
        /// </param>
        /// <returns>
        /// True if the method found a rune. This includes invalid input that returns the replacement character or that
        /// skips to the next valid rune. False if <paramref name="position"/> is already at the end of the string.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="position"/> is less than zero or greater that the string's length.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The Unicode character or sequence at the given position is invalid, and <paramref name="errorHandling"/> is
        /// set to <see cref="RuneParseErrorHandling.ThrowException"/>.
        /// </exception>
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

        /// <summary>
        /// Reads the runes from a given string. If the string contains invalid Unicode characters/sequences, they will be replaced by
        /// the <see cref="ReplacementCharacter"/>.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <returns>A sequence of <see cref="Rune"/>s.</returns>
        public static IEnumerable<Rune> Read(string s)
            => Read(s, default);

        /// <summary>
        /// Reads the runes from a given string.
        /// the <see cref="ReplacementCharacter"/>.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="errorHandling">How to handle invalid Unicode characters/sequences in the string.</param>
        /// <returns>A sequence of <see cref="Rune"/>s.</returns>
        public static IEnumerable<Rune> Read(string s, RuneParseErrorHandling errorHandling)
        {
            int pos = 0;
            while (TryRead(s, ref pos, out Rune r, errorHandling))
                yield return r;
        }

    }
}
