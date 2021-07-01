using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Utf32Net
{
	public static class RuneStone
	{
		public static bool TryRead(string s, ref int position, out Rune r)
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

			position = s.Length;
			r = default;
			return false;
		}

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

        public static Rune ZeroWidthNonJoiner { get; } = new( 0x200C);
        public static Rune ZeroWidthJoiner { get; }=new( 0x200D);
        public static Rune CombiningGraphemeJoiner { get; } = new(0x034F);

        internal static SortedSet<Rune> Joiners = new(new[] { ZeroWidthNonJoiner, ZeroWidthJoiner, CombiningGraphemeJoiner });
    }
}
