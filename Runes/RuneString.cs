using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Tsonto.Collections.Generic;

namespace Tsonto.System.Text
{
    public sealed class RuneString : IEnumerable<Rune>, IEquatable<RuneString>, IComparable<RuneString>, IReadOnlyList<Rune>
    {
        public RuneString(IEnumerable<Rune> runes)
        {
            data = new(runes);
        }

        public RuneString(in ReadOnlySpan<Rune> runes)
        {
            data = new(in runes);
        }

        public RuneString(string s)
            : this(s, RuneParseErrorHandling.UseReplacementCharacter)
        {
        }

        public RuneString(string s, RuneParseErrorHandling errorHandling)
        {
            data = MakeRuneArrayFromString(s, errorHandling);
        }

        public RuneString(in ImmutableArraySegment<Rune> runes)
        {
            data = runes;
        }

        public int Length
            => data.Length;

        public Rune this[Index index]
            => data[index];

        public RuneString this[Range range]
            => new(data[range]);

        public int Utf16UnitLength
            => data.Sum(rune => rune.Utf16SequenceLength);

        public int Utf32UnitLength
            => Length;

        public int Utf8UnitLength
            => data.Sum(rune => rune.Utf8SequenceLength);

        public int Count => ((IReadOnlyCollection<Rune>)data).Count;

        public Rune this[int index] => ((IReadOnlyList<Rune>)data)[index];

        public static readonly RuneString Empty = new RuneString(default(ImmutableArraySegment<Rune>));

        private static readonly Encoding Utf32BE = new UTF32Encoding(bigEndian: true, byteOrderMark: false);

        private readonly ImmutableArraySegment<Rune> data;

        public static bool operator !=(RuneString? left, RuneString? right)
            => !(left == right);

        public static RuneString operator +(RuneString? a, RuneString? b)
        {
            if (a is null && b is null)
                return Empty;
            if (a is null)
                return b!;
            if (b is null)
                return a;
            return new(a.data.Append(b.data));
        }

        public static bool operator <(RuneString? left, RuneString? right)
            => left is null
            ? right is not null
            : left.CompareTo(right) < 0;

        public static bool operator <=(RuneString? left, RuneString? right)
            => left is null
            || left.CompareTo(right) <= 0;

        public static bool operator ==(RuneString? left, RuneString? right)
            => left is null
            ? right is null
            : left.Equals(right);

        public static bool operator >(RuneString? left, RuneString? right)
            => left is not null
            && left.CompareTo(right) > 0;

        public static bool operator >=(RuneString? left, RuneString? right)
            => left is null
            ? right is null
            : left.CompareTo(right) >= 0;

        public int CompareTo(RuneString? other)
        {
            // Null always sorts before non-null.
            if (other is null)
                return -1;

            // Check for reference equality. It's cheap, and could save us a lot of work.
            if (ReferenceEquals(other, this))
                return 0;

            for (int i = 0; i < data.Length && i < other.data.Length; ++i)
            {
                var cpComparison = data[i].CompareTo(other.data[i]);
                if (cpComparison != 0)
                    return cpComparison;
            }

            // If all the elements match up to the length of the shorter string, then the shorter string sorts before
            // the longer one.
            return data.Length.CompareTo(other.data.Length);
        }

        public bool Contains(in Rune rune)
            => data.IndexOf(in rune, RuneEquals) != -1;

        public bool Equals([NotNullWhen(true)] RuneString? other)
            => other is not null
            && data.SequenceEquals(other.data);

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is RuneString s && Equals(s);

        public IEnumerator<Rune> GetEnumerator()
            => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => data.GetEnumerator();

        public override int GetHashCode()
            => data.GetHashCode();

        public int IndexOf(in Rune rune)
            => data.IndexOf(in rune, RuneEquals);

        public int IndexOf(in Rune rune, int start)
            => data.IndexOf(in rune, start, RuneEquals);

        public int IndexOf(in Rune rune, int start, int length)
            => data.IndexOf(in rune, start, length, RuneEquals);

        public RuneString[] Split(in Rune delimiter)
        {
            var parts = new List<RuneString>();
            int i = 0;
            while (true)
            {
                // Look for the next instance of the delimiter. If there isn't any, add the remainder as the last piece
                // and return.
                int j = IndexOf(in delimiter, i);
                if (j == -1)
                {
                    parts.Add(this[i..]);
                    return parts.ToArray();
                }

                // Add the piece since after the last delimiter.
                parts.Add(this[i..j]);
                i = j + 1;
            }
        }

        public RuneString Substring(int start)
            => new(data[start..]);

        public RuneString Substring(int start, int length)
            => new(data[start..(start + length)]);

        public char[] ToChars()
            => ToString().ToCharArray();

        public override string ToString()
            => string.Join("", data.Select(rune => rune.ToString()));

        public byte[] ToUtf16BEBytes()
            => Encoding.BigEndianUnicode.GetBytes(ToString());

        public byte[] ToUtf16LEBytes()
            => Encoding.Unicode.GetBytes(ToString());

        public int[] ToUtf16Units()
            => ToString().Select(c => (int)c).ToArray();

        public byte[] ToUtf32BEBytes()
            => Utf32BE.GetBytes(ToString());

        public byte[] ToUtf32LEBytes()
            => Encoding.UTF32.GetBytes(ToString());

        public int[] ToUtf32Units()
            => data.Select(rune => rune.Value).ToArray();

        public byte[] ToUtf8Bytes()
            => Encoding.UTF8.GetBytes(ToString());

        public int[] ToUtf8Units()
            => ToUtf8Bytes().Select(b => (int)b).ToArray();

        private static ImmutableArraySegment<Rune> MakeRuneArrayFromString(string s, RuneParseErrorHandling errorHandling)
        {
            int pos = 0;
            var list = new List<Rune>();
            while (RuneStone.TryRead(s, ref pos, out Rune r, errorHandling))
                list.Add(r);
            return new(list);
        }

        private static bool RuneEquals(in Rune a, in Rune b)
            => a.Value == b.Value;

        public static RuneString Concat(params RuneString[] strings)
        {
            if (strings.Length == 0)
                return Empty;
            if (strings.Length == 1)
                return strings[0];

            var individualSegments = strings.Select(s => s.data).Cast<IReadOnlyList<Rune>>().ToArray();
            var combinedData = ImmutableArraySegment.Concat(individualSegments);
            return new RuneString(combinedData);
        }

        public static RuneString Join(Rune delimiter, params RuneString[] strings)
        {
            if (strings.Length == 0)
                return Empty;
            if (strings.Length == 1)
                return strings[0];

            var individualSegments = strings.Select(s => s.data).Cast<IReadOnlyList<Rune>>().ToArray();
            var combinedData = ImmutableArraySegment.Join(delimiter, individualSegments);
            return new RuneString(combinedData);
        }

        public static RuneString Join(RuneString delimiter, params RuneString[] strings)
        {
            if (strings.Length == 0)
                return Empty;
            if (strings.Length == 1)
                return strings[0];

            var individualSegments = strings.Select(s => s.data).Cast<IReadOnlyList<Rune>>().ToArray();
            var combinedData = ImmutableArraySegment.Join(delimiter, individualSegments);
            return new RuneString(combinedData);
        }
    }
}
