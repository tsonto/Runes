using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Utf32Net
{
	public sealed class RuneString : IEnumerable<Rune>, IEquatable<RuneString>, IComparable<RuneString>
	{
		private readonly Rune[] array;

		public static RuneString FromSequence(IEnumerable<Rune> runes)
		{
			return new(runes.ToArray(), raw: true);
		}

		public RuneString(ReadOnlySpan<Rune> runes)
		{
			array = runes.ToArray();
		}

		public RuneString(string s)
		{
			array = MakeRuneArrayFromString(s);
		}

		public override string ToString()
			=> string.Join("", array.Select(rune => rune.ToString()));

		internal static Rune[] MakeRuneArrayFromString(string s)
		{
			int pos = 0;
			var list = new List<Rune>();
			while (RuneStone.TryRead(s, ref pos, out Rune r))
				list.Add(r);
			return list.ToArray();
		}

		private RuneString(Rune[] array, bool raw)
		{
			if (raw)
				this.array = array;
			else
				this.array = array.ToArray();
		}

		public Rune this[Index index] => array[index];

		public RuneString this[Range range]
		{
			get
			{
				(int offset, int length) = range.GetOffsetAndLength(array.Length);
				return Substring(offset, length);
			}
		}

		public int IndexOf(Rune rune)
			=> IndexOf(rune, 0, array.Length);

		public int IndexOf(Rune rune, int start)
			=> IndexOf(rune, start, array.Length - start);

		public int IndexOf(Rune rune, int start, int length)
		{
			for (int i = start; i < start + length; ++i)
				if (array[i] == rune)
					return i;
			return -1;
		}

		public RuneString[] Split(Rune delimiter)
		{
			var parts = new List<RuneString>();
			int i = 0;
			while (true)
			{
				// Look for the next instance of the delimiter. If there isn't any, add
				// the remainder as the last piece and return.
				int j = IndexOf(delimiter, i);
				if (j == -1)
				{
					parts.Add(Substring(i));
					return parts.ToArray();
				}

				// Add the piece since after the last delimiter.
				parts.Add(Substring(i, j - i));
				i = j + 1;
			}
		}

		[SuppressMessage("Performance", "CA1825:Avoid zero-length array allocations", Justification = "Avoid overhead of type generation for Array.Empty<T>")]
		public static RuneString Empty { get; } = new RuneString(new Rune[0], raw: true);

		public RuneString TrimStart()
			=> FromSequence(array.SkipWhile(Rune.IsWhiteSpace));

		public RuneString TrimEnd()
		{
			for (int i = array.Length - 1; i >= 0; --i)
				if (!Rune.IsWhiteSpace(array[i]))
					return Substring(0, i + 1);
			return Empty;
		}

		public RuneString Trim()
			=> TrimStart().TrimEnd();

		public int Length => array.Length;

		public RuneString Substring(int start)
			=> Substring(start, array.Length - start);

		public RuneString Substring(int start, int length)
		{
			if (start == 0 && length == array.Length)
				return this;
			if (length == 0)
				return Empty;
			if (start == array.Length)
				return Empty;
			return new(array[start..(start + length)], raw: true);
		}

		public int CompareTo(RuneString? other)
		{
			// Null always sorts before non-null.
			if (other is null)
				return -1;

			for (int i = 0; i < array.Length && i < other.array.Length; ++i)
			{
				var cpComparison = array[i].CompareTo(other.array[i]);
				if (cpComparison != 0)
					return cpComparison;
			}

			// If all the elements match up to the length of the shorter string, the the shorter string sorts before the longer one.
			return array.Length.CompareTo(other.array.Length);
		}

		public bool Equals([NotNullWhen(true)] RuneString? other)
			=> other is not null
			&& array.SequenceEqual(other.array);

		public IEnumerator<Rune> GetEnumerator()
			=> (IEnumerator<Rune>)array.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> array.GetEnumerator();

		public static bool operator ==(RuneString? left, RuneString? right)
			=> left is null
			? right is null
			: left.Equals(right);

		public static bool operator !=(RuneString? left, RuneString? right)
			=> !(left == right);

		public static bool operator <(RuneString? left, RuneString? right)
			=> left is null
			? right is not null
			: left.CompareTo(right) < 0;

		public static bool operator <=(RuneString? left, RuneString? right)
			=> left is null
			|| left.CompareTo(right) <= 0;

		public static bool operator >(RuneString? left, RuneString? right)
			=> left is not null
			&& left.CompareTo(right) > 0;

		public static bool operator >=(RuneString? left, RuneString? right)
			=> left is null
			? right is null
			: left.CompareTo(right) >= 0;

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> ReferenceEquals(this, obj)
			|| (obj is RuneString s && Equals(s));

		public override int GetHashCode()
			=> array.GetHashCode();
	}
}