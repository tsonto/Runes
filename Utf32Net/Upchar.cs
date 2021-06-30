using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Utf32Net
{
	public readonly struct Upchar : IEquatable<Upchar>, IComparable<Upchar>
	{
		private readonly Rune[] array;

		private Upchar(string textElement)
		{
			// Note: Upchar runes are always in Unicode normalization form C.
			array = RuneString.MakeRuneArrayFromString(textElement.Normalize());

			if (RuneStone.IsModifer(array[0]))
				throw new ArgumentException("The string does not begin with a base character.", nameof(textElement));
		}

		public static bool TryRead(ReadOnlySpan<Rune> s, ref int, out Upchar c)
		{

		}

		public Rune[] Runes 
			=> array.ToArray();

		public static bool TryRead(string s, ref int position, out Upchar c)
		{
			if (position < 0 || position > s.Length)
				throw new ArgumentOutOfRangeException(nameof(position));

			if (position == s.Length)
			{
				c = default;
				return false;
			}

			string te = StringInfo.GetNextTextElement(s, position);
			if (te.Length == 0)
			{
				position = s.Length;
				c = default;
				return false;
			}

			position += te.Length;
			c = new Upchar(te);
			return true;
		}

		public Upchar AppendModifiers(string modifiers)
		{
			if (modifiers is null)
				throw new ArgumentNullException(nameof(modifiers));
			if (modifiers.Length == 0)
				return this;

			if (!modifiers.All(RuneStone.IsModifer))
				throw new ArgumentException("The string contained one or more non-modifier characters.", nameof(modifiers));

			var s = (ToString() + modifiers).Normalize();
			int position = 0;
			if (!TryRead(s, ref position, out Upchar upchar))
				throw new InvalidStringException();
			if (position != s.Length)
				throw new ArgumentException("The string contained multiple base characters.", nameof(modifiers));

			return upchar;
		}

		public static bool IsWhitespace(Upchar upchar)
			=> Rune.IsWhiteSpace(upchar.BaseRune);

		public static UnicodeCategory GetUnicodeCategory(Upchar upchar)
				=> Rune.GetUnicodeCategory(upchar.BaseRune);

		public Rune BaseRune => array[0];

		public Rune[] Modifiers => array[1..];

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Upchar g
			&& Equals(g);

		public override int GetHashCode()
			=> array.GetHashCode();

		public int CompareTo(Upchar other)
		{
			for (int i = 0; i < array.Length && i < other.array.Length; ++i)
			{
				var runeComparison = array[i].CompareTo(other.array[i]);
				if (runeComparison != 0)
					return runeComparison;
			}

			// If all the elements match up to the length of the shorter string, the the shorter string sorts before the longer one.
			return array.Length.CompareTo(other.array.Length);
		}

		public bool Equals(Upchar other)
			=> array.SequenceEqual(other.array);

		public static bool operator ==(Upchar left, Upchar right) => left.Equals(right);

		public static bool operator !=(Upchar left, Upchar right) => !(left == right);

		public static bool operator <(Upchar left, Upchar right) => left.CompareTo(right) < 0;

		public static bool operator <=(Upchar left, Upchar right) => left.CompareTo(right) <= 0;

		public static bool operator >(Upchar left, Upchar right) => left.CompareTo(right) > 0;

		public static bool operator >=(Upchar left, Upchar right) => left.CompareTo(right) >= 0;

		public static UpcharString operator +(Upchar left, Upchar right)
			=> new UpcharString(new[] { left, right }, raw: true);

		internal int Utf16SequenceLength => array.Sum(r => r.Utf16SequenceLength);
	}
}