using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Utf32Net
{
	public sealed class UpcharString : IEnumerable<Upchar>, IEquatable<UpcharString>, IComparable<UpcharString>
	{
		private readonly Upchar[] array;

		public UpcharString(string s)
		{
			array = MakeUpcharArrayFromString(s);
		}

		public static UpcharString FromSequence(IEnumerable<Upchar> sequence)
			=> new UpcharString(sequence.ToArray(), true);

		public static UpcharString FromSequence(IEnumerable<Rune> sequence)
			=> new UpcharString(sequence.ToArray(), true);


		public UpcharString(ReadOnlySpan<Upchar> g)
		{
			array = g.ToArray();
		}


		public UpcharString(RuneString r)
			: this(r.ToString())
		{ }

		internal UpcharString(Upchar[] array, bool raw)
		{
			if (raw)
				this.array = array;
			else
				this.array = array.ToArray();
		}

		private static Upchar[] MakeUpcharArrayFromString(string s)
		{
			int position = 0;
			var list = new List<Upchar>();
			while (Upchar.TryRead(s, ref position, out Upchar g))
				list.Add(g);
			return list.ToArray();
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> ReferenceEquals(this, obj)
			|| (obj is UpcharString s && Equals(s));

		public override int GetHashCode()
			=> array.GetHashCode();

		public IEnumerator<Upchar> GetEnumerator()
			=> ((IEnumerable<Upchar>)array).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> array.GetEnumerator();

		public bool Equals([NotNullWhen(true)] UpcharString? other)
			=> other is not null
			&& array.SequenceEqual(other.array);

		public override string ToString()
			=> string.Join("", array.Select(upchar => upchar.ToString()));

		public UpcharString Substring(int start)
			=> Substring(start, array.Length - start);

		public UpcharString Substring(int start, int length)
		{
			if (start == 0 && length == array.Length)
				return this;
			if (length == 0)
				return Empty;
			if (start == array.Length)
				return Empty;
			return new(array[start..(start + length)], raw: true);
		}

		public Upchar this[Index index] => array[index];

		public UpcharString this[Range range]
		{
			get
			{
				(int offset, int length) = range.GetOffsetAndLength(array.Length);
				return Substring(offset, length);
			}
		}

		[SuppressMessage("Performance", "CA1825:Avoid zero-length array allocations", Justification = "Avoid overhead of type generation for Array.Empty<T>")]
		public static UpcharString Empty { get; } = new(new Upchar[0], raw: true);

		public int CompareTo(UpcharString? other)
		{
			// Null always sorts before non-null.
			if (other is null)
				return -1;

			for (int i = 0; i < array.Length && i < other.array.Length; ++i)
			{
				var upcharComparison = array[i].CompareTo(other.array[i]);
				if (upcharComparison != 0)
					return upcharComparison;
			}

			// If all the elements match up to the length of the shorter string, the the shorter string sorts before the longer one.
			return array.Length.CompareTo(other.array.Length);
		}

		public static bool operator ==(UpcharString? left, UpcharString? right)
			=> left is null
			? right is null
			: left.Equals(right);

		public static bool operator !=(UpcharString? left, UpcharString? right)
			=> !(left == right);

		public static bool operator <(UpcharString? left, UpcharString? right)
			=> left is null
			? right is not null
			: left.CompareTo(right) < 0;

		public static bool operator <=(UpcharString? left, UpcharString? right)
			=> left is null
			|| left.CompareTo(right) <= 0;

		public static bool operator >(UpcharString? left, UpcharString? right)
			=> left is not null
			&& left.CompareTo(right) > 0;

		public static bool operator >=(UpcharString? left, UpcharString? right)
			=> left is null
			? right is null
			: left.CompareTo(right) >= 0;

		public int Length => array.Length;

		public static UpcharString operator +(UpcharString left, Upchar right)
		{
			var newArray = new Upchar[left.Length + 1];
			Buffer.BlockCopy(left.array, 0, newArray, 0, left.Length);
			newArray[left.Length] = right;
			return new(newArray, raw: true);
		}

		public static UpcharString operator +(Upchar left, UpcharString right)
		{
			var newArray = new Upchar[right.Length + 1];
			Buffer.BlockCopy(right.array, 0, newArray, 1, right.Length);
			newArray[0] = left;
			return new(newArray, raw: true);
		}

		public static UpcharString operator +(UpcharString left, UpcharString right)
		{
			var newArray = new Upchar[left.Length + right.Length];
			Buffer.BlockCopy(left.array, 0, newArray, 0, left.Length);
			Buffer.BlockCopy(right.array, 0, newArray, left.Length, right.Length);
			return new(newArray, raw: true);
		}
	}
}