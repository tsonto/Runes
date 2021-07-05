using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Tsonto.Collections.Generic;

namespace Tsonto.System.Text
{
    /// <summary>
    /// An immutable sequence of <see cref="Rune"/> s.
    /// </summary>
    public sealed class RuneString : IEnumerable<Rune>, IEquatable<RuneString>, IComparable<RuneString>, IReadOnlyList<Rune>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuneString"/> class.
        /// </summary>
        /// <param name="runes">The runes that will make up the string.</param>
        public RuneString(IEnumerable<Rune> runes)
        {
            data = new(runes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuneString"/> class.
        /// </summary>
        /// <param name="runes">The runes that will make up the string.</param>
        public RuneString(in ReadOnlySpan<Rune> runes)
        {
            data = new(in runes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuneString"/> class.
        /// </summary>
        /// <param name="s">A string to parse for runes.</param>
        /// <remarks>
        /// If the string contains invalid Unicode characters/sequences, they will be replaced by the Unicode
        /// replacement character (U+FFFD).
        /// </remarks>
        public RuneString(string s)
            : this(s, RuneParseErrorHandling.UseReplacementCharacter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuneString"/> class.
        /// </summary>
        /// <param name="s">A string to parse for runes.</param>
        /// <param name="errorHandling">How to handle invalid Unicode characters/sequences.</param>
        public RuneString(string s, RuneParseErrorHandling errorHandling)
        {
            data = MakeRuneArrayFromString(s, errorHandling);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuneString"/> class.
        /// </summary>
        /// <param name="runes">The runes that will make up the string.</param>
        public RuneString(in ImmutableArraySegment<Rune> runes)
        {
            data = runes;
        }

        /// <summary>
        /// Gets the number of runes in the string.
        /// </summary>
        int IReadOnlyCollection<Rune>.Count => ((IReadOnlyCollection<Rune>)data).Count;

        /// <summary>
        /// Gets the number of runes in the string.
        /// </summary>
        public int Length
            => data.Length;

        /// <summary>
        /// Gets the rune at the given position in the rune string.
        /// </summary>
        /// <param name="index">The offset, in number of runes.</param>
        /// <returns>The rune at the given position.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// The indicated position is outside the bounds of the string.
        /// </exception>
        public Rune this[Index index]
            => data[index];

        /// <summary>
        /// Gets the rune at the given position in the rune string.
        /// </summary>
        /// <param name="index">The offset, in number of runes.</param>
        /// <returns>The rune at the given position.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// The indicated position is outside the bounds of the string.
        /// </exception>
        public Rune this[int index] => ((IReadOnlyList<Rune>)data)[index];

        /// <summary>
        /// Gets a portion of the rune string.
        /// </summary>
        /// <param name="range">The portion of the string to retrieve.</param>
        /// <returns>A new <see cref="RuneString"/> representing the indicated portion of the original.</returns>
        /// <remarks>This operation does not perform a copy—it is an O(1) operation.</remarks>
        /// <exception cref="IndexOutOfRangeException">
        /// The start or end position is outside the bounds of the string.
        /// </exception>
        public RuneString this[Range range]
            => new(data[range]);

        /// <summary>
        /// Gets the length, in 16-bit units, of the string if it were converted to UTF-16 format.
        /// </summary>
        public int Utf16UnitLength
            => data.Sum(rune => rune.Utf16SequenceLength);

        /// <summary>
        /// Gets the length, in 32-bit units, of the string if it were converted to UTF-32 format.
        /// </summary>
        public int Utf32UnitLength
            => Length;

        /// <summary>
        /// Gets the length, in 8-bit units, of the string if it were converted to UTF-8 format.
        /// </summary>
        public int Utf8UnitLength
            => data.Sum(rune => rune.Utf8SequenceLength);

        /// <summary>
        /// Gets a <see cref="RuneString"/> of length 0.
        /// </summary>
        public static readonly RuneString Empty = new RuneString(default(ImmutableArraySegment<Rune>));

        private static readonly Encoding Utf32BE = new UTF32Encoding(bigEndian: true, byteOrderMark: false);

        private readonly ImmutableArraySegment<Rune> data;

        /// <summary>
        /// Creates a new <see cref="RuneString"/> from the concatenation of multiple source strings.
        /// </summary>
        /// <param name="strings">The source strings.</param>
        /// <returns>A new <see cref="RuneString"/> of the runes from the source strings.</returns>
        /// <remarks>
        /// This operation has O(n) memory usage and O(s+n) time complexity, where n is the total length of all inputs
        /// and s is the number of inputs. Some special cases are O(1).
        /// </remarks>
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

        /// <summary>
        /// Combine the input strings sequentially, with a given delimiter between each source's content.
        /// </summary>
        /// <param name="delimiter">A <see cref="Rune"/> to place between each source.</param>
        /// <param name="strings">The sources to combine.</param>
        /// <returns>A new <see cref="RuneString"/> of the runes from the source strings.</returns>
        /// <remarks>
        /// This operation has O(n) memory usage and O(s+n) time complexity, where n is the total length of all inputs
        /// and s is the number of inputs. Some special cases are O(1).
        /// </remarks>
        public static RuneString Join(Rune delimiter, RuneString[] strings)
        {
            if (strings.Length == 0)
                return Empty;
            if (strings.Length == 1)
                return strings[0];

            var individualSegments = strings.Select(s => s.data).Cast<IReadOnlyList<Rune>>().ToArray();
            var combinedData = ImmutableArraySegment.Join(delimiter, individualSegments);
            return new RuneString(combinedData);
        }

        /// <summary>
        /// Combine the input strings sequentially, with a given delimiter between each source's content.
        /// </summary>
        /// <param name="delimiter">A <see cref="RuneString"/> to place between each source.</param>
        /// <param name="strings">The sources to combine.</param>
        /// <returns>A new <see cref="RuneString"/> of the runes from the source strings.</returns>
        /// <remarks>
        /// This operation has O(n + s*d) time and memory complexity, where n is the total length of all source inputs,
        /// s is the number of source inputs, and d is the length of the delimiter. Some special cases are O(1).
        /// </remarks>
        public static RuneString Join(RuneString delimiter, RuneString[] strings)
        {
            if (strings.Length == 0)
                return Empty;
            if (strings.Length == 1)
                return strings[0];

            var individualSegments = strings.Select(s => s.data).Cast<IReadOnlyList<Rune>>().ToArray();
            var combinedData = ImmutableArraySegment.Join(delimiter, individualSegments);
            return new RuneString(combinedData);
        }

        /// <summary>
        /// Determines whether two <see cref="RuneString"/> s are inequal. This method is not locale-aware.
        /// </summary>
        /// <param name="left">One of the strings.</param>
        /// <param name="right">The other string.</param>
        /// <returns>True if the inputs are inequal; false otherwise.</returns>
        public static bool operator !=(RuneString? left, RuneString? right)
            => !(left == right);

        /// <summary>
        /// Concatenates two <see cref="RuneString"/> s.
        /// </summary>
        /// <param name="a">The first input.</param>
        /// <param name="b">The second input.</param>
        /// <returns>A new <see cref="RuneString"/> made of the concatenated inputs.</returns>
        /// <remarks>Null inputs are treated as empty strings.</remarks>
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

        /// <summary>
        /// Determines whether the first <see cref="RuneString"/> sorts before the second. This method is not
        /// locale-aware.
        /// </summary>
        /// <param name="left">One of the strings.</param>
        /// <param name="right">The other string.</param>
        /// <returns>True if the first string sorts before the second; false otherwise.</returns>
        /// <remarks>Null sorts before non-null.</remarks>
        public static bool operator <(RuneString? left, RuneString? right)
            => left is null
            ? right is not null
            : left.CompareTo(right) < 0;

        /// <summary>
        /// Determines whether the first <see cref="RuneString"/> sorts at or before the second. This method is not
        /// locale-aware.
        /// </summary>
        /// <param name="left">One of the strings.</param>
        /// <param name="right">The other string.</param>
        /// <returns>True if the first string sorts at or before the second; false otherwise.</returns>
        /// <remarks>Null sorts before non-null.</remarks>
        public static bool operator <=(RuneString? left, RuneString? right)
            => left is null
            || left.CompareTo(right) <= 0;

        /// <summary>
        /// Determines whether two <see cref="RuneString"/> s are equal (have the same runes in the same sequence). This
        /// method is not locale-aware.
        /// </summary>
        /// <param name="left">One of the strings.</param>
        /// <param name="right">The other string.</param>
        /// <returns>True if the inputs are equal; false otherwise.</returns>
        public static bool operator ==(RuneString? left, RuneString? right)
            => left is null
            ? right is null
            : left.Equals(right);

        /// <summary>
        /// Determines whether the first <see cref="RuneString"/> sorts after the second. This method is not
        /// locale-aware.
        /// </summary>
        /// <param name="left">One of the strings.</param>
        /// <param name="right">The other string.</param>
        /// <returns>True if the first string sorts after the second; false otherwise.</returns>
        /// <remarks>Null sorts before non-null.</remarks>
        public static bool operator >(RuneString? left, RuneString? right)
            => left is not null
            && left.CompareTo(right) > 0;

        /// <summary>
        /// Determines whether the first <see cref="RuneString"/> sorts at or after the second. This method is not
        /// locale-aware.
        /// </summary>
        /// <param name="left">One of the strings.</param>
        /// <param name="right">The other string.</param>
        /// <returns>True if the first string sorts at or after the second; false otherwise.</returns>
        /// <remarks>Null sorts before non-null.</remarks>
        public static bool operator >=(RuneString? left, RuneString? right)
            => left is null
            ? right is null
            : left.CompareTo(right) >= 0;

        /// <summary>
        /// Calculates whether the given <see cref="RuneString"/> sorts before, after, or at the same place as this one.
        /// This method is not locale-aware.
        /// </summary>
        /// <param name="other">The string to compare against the current one.</param>
        /// <returns>
        /// 0 if the strings are equal; a negative value if the current string sorts before the given string; or a
        /// positive value if the current string sorts after the given string.
        /// </returns>
        /// <remarks>Null always sorts before non-null.</remarks>
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

        /// <summary>
        /// Checks whether the given rune is present in the string.
        /// </summary>
        /// <param name="rune">The rune to look for.</param>
        /// <returns>True if the rune is present in the string; false otherwise.</returns>
        public bool Contains(in Rune rune)
            => data.IndexOf(in rune, RuneEquals) != -1;

        /// <summary>
        /// Checks whether the given string is equal to the current string. This method is not locale-aware.
        /// </summary>
        /// <param name="other">The string to check against this one.</param>
        /// <returns>True if the strings are equal.</returns>
        public bool Equals([NotNullWhen(true)] RuneString? other)
            => other is not null
            && data.SequenceEquals(other.data);

        /// <summary>
        /// Checks whether the given object is a <see cref="RuneString"/> and equal to the current string. This method
        /// is not locale-aware.
        /// </summary>
        /// <param name="obj">The string to check against this one.</param>
        /// <returns>True if the strings are equal.</returns>
        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is RuneString s && Equals(s);

        /// <inheritdoc/>
        public IEnumerator<Rune> GetEnumerator()
            => data.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
            => data.GetEnumerator();

        /// <inheritdoc/>
        public override int GetHashCode()
            => data.GetHashCode();

        /// <summary>
        /// Finds the position of the first instance of the given rune in the string, or -1 if it is not present.
        /// </summary>
        /// <param name="rune">The rune to search for.</param>
        /// <returns>
        /// The position of the rune, as a number of runes from the beginning, or -1 to indicate that the rune is not
        /// present.
        /// </returns>
        public int IndexOf(in Rune rune)
            => data.IndexOf(in rune, RuneEquals);

        /// <summary>
        /// Finds the position of the first instance of the given rune in the string, or -1 if it is not present.
        /// </summary>
        /// <param name="rune">The rune to search for.</param>
        /// <param name="start">The position to start searching from, inclusive. 0 to search from the beginning.</param>
        /// <returns>
        /// The position of the rune, as a number of runes from the beginning of the string, or -1 to indicate that the
        /// rune is not present in the range.
        /// </returns>
        public int IndexOf(in Rune rune, int start)
            => data.IndexOf(in rune, start, RuneEquals);

        /// <summary>
        /// Finds the position of the first instance of the given rune in the string, or -1 if it is not present.
        /// </summary>
        /// <param name="rune">The rune to search for.</param>
        /// <param name="start">The position to start searching from, inclusive. 0 to search from the beginning.</param>
        /// <param name="length">The number of runes to search from the start position.</param>
        /// <returns>
        /// The position of the rune, as a number of runes from the beginning of the string, or -1 to indicate that the
        /// rune is not present in the range.
        /// </returns>
        public int IndexOf(in Rune rune, int start, int length)
            => data.IndexOf(in rune, start, length, RuneEquals);

        /// <summary>
        /// Finds the position of the first instance of the given substring in the string, or -1 if it is not present.
        /// </summary>
        /// <param name="substring">The substring to search for.</param>
        /// <param name="start">The position to start searching from, inclusive. 0 to search from the beginning.</param>
        /// <param name="length">The number of runes to search from the start position.</param>
        /// <returns>
        /// The start position of the substring, as a number of runes from the beginning of the string, or -1 to indicate that the
        /// substring is not present in the range.
        /// </returns>
        public int IndexOf(in RuneString substring, int start, int length)
            => data.IndexOf(substring.data, start, length, RuneEquals);

        /// <summary>
        /// Finds the position of the first instance of the given substring in the string, or -1 if it is not present.
        /// </summary>
        /// <param name="substring">The substring to search for.</param>
        /// <param name="start">The position to start searching from, inclusive. 0 to search from the beginning.</param>
        /// <returns>
        /// The start position of the substring, as a number of runes from the beginning of the string, or -1 to indicate that the
        /// substring is not present in the range.
        /// </returns>
        public int IndexOf(in RuneString substring, int start)
            => data.IndexOf(substring.data, start, data.Length, RuneEquals);

        /// <summary>
        /// Finds the position of the first instance of the given substring in the string, or -1 if it is not present.
        /// </summary>
        /// <param name="substring">The substring to search for.</param>
        /// <returns>
        /// The start position of the substring, as a number of runes from the beginning of the string, or -1 to indicate that the
        /// substring is not present in the range.
        /// </returns>
        public int IndexOf(in RuneString substring)
            => data.IndexOf(substring.data, 0, data.Length, RuneEquals);

        /// <summary>
        /// Finds the position of the first instance of any of the given runes in the string, or -1 if none are present.
        /// </summary>
        /// <param name="runes">The runes to search for.</param>
        /// <param name="start">The position to start searching from, inclusive. 0 to search from the beginning.</param>
        /// <param name="length">The number of runes to search from the start position.</param>
        /// <returns>
        /// The position of the first match, as a number of runes from the beginning of the string, or -1 to indicate that none
        /// of the runes are present in the range.
        /// </returns>
        public int IndexOfAny(Rune[] runes, int start, int length)
            => data.IndexOfAny(runes, start, length, RuneEquals);

        /// <summary>
        /// Finds the position of the first instance of any of the given runes in the string, or -1 if none are present.
        /// </summary>
        /// <param name="runes">The runes to search for.</param>
        /// <param name="start">The position to start searching from, inclusive. 0 to search from the beginning.</param>
        /// <returns>
        /// The position of the first match, as a number of runes from the beginning of the string, or -1 to indicate that none
        /// of the runes are present in the range.
        /// </returns>
        public int IndexOfAny(Rune[] runes, int start)
            => data.IndexOfAny(runes, start, data.Length, RuneEquals);

        /// <summary>
        /// Finds the position of the first instance of any of the given runes in the string, or -1 if none are present.
        /// </summary>
        /// <param name="runes">The runes to search for.</param>
        /// <returns>
        /// The position of the first match, as a number of runes from the beginning of the string, or -1 to indicate that none
        /// of the runes are present in the range.
        /// </returns>
        public int IndexOfAny(Rune[] runes)
            => data.IndexOfAny(runes, 0, data.Length, RuneEquals);

        /// <summary>
        /// Finds the position of the first instance of any of the given substrings in the string, or -1 if none are present.
        /// </summary>
        /// <param name="substrings">The substrings to search for.</param>
        /// <param name="start">The position to start searching from, inclusive. 0 to search from the beginning.</param>
        /// <param name="length">The number of runes to search from the start position.</param>
        /// <returns>
        /// The position of the start of the first match, as a number of runes from the beginning of the string, or -1 to indicate that none
        /// of the substrings are present in the range.
        /// </returns>
        public int IndexOfAny(RuneString[] substrings, int start, int length)
            => data.IndexOfAny(substrings.Select(s=>s.data).ToArray(), start, length, RuneEquals);

        /// <summary>
        /// Finds the position of the first instance of any of the given substrings in the string, or -1 if none are present.
        /// </summary>
        /// <param name="substrings">The substrings to search for.</param>
        /// <param name="start">The position to start searching from, inclusive. 0 to search from the beginning.</param>
        /// <returns>
        /// The position of the start of the first match, as a number of runes from the beginning of the string, or -1 to indicate that none
        /// of the substrings are present in the range.
        /// </returns>
        public int IndexOfAny(RuneString[] substrings, int start)
            => data.IndexOfAny(substrings.Select(s => s.data).ToArray(), start, data.Length, RuneEquals);

        /// <summary>
        /// Finds the position of the first instance of any of the given substrings in the string, or -1 if none are present.
        /// </summary>
        /// <param name="substrings">The substrings to search for.</param>
        /// <returns>
        /// The position of the start of the first match, as a number of runes from the beginning of the string, or -1 to indicate that none
        /// of the substrings are present in the range.
        /// </returns>
        public int IndexOfAny(RuneString[] substrings)
            => data.IndexOfAny(substrings.Select(s => s.data).ToArray(), 0, data.Length, RuneEquals);

        /// <summary>
        /// Returns a <see cref="RuneString"/> array that contains the substrings in this instance that are delimited by
        /// a specified <see cref="Rune"/>.
        /// </summary>
        /// <param name="delimiter">The <see cref="Rune"/> to split on.</param>
        /// <returns>An array of substrings.</returns>
        /// <remarks>The output will always have at least one element, which may be empty.</remarks>
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

        /// <summary>
        /// Gets a portion of the rune string starting from a specified position and going to the end.
        /// </summary>
        /// <param name="start">The offset from the front to start at.</param>
        /// <returns>A new <see cref="RuneString"/> representing the indicated portion of the original.</returns>
        /// <remarks>This operation does not perform a copy—it is an O(1) operation.</remarks>
        /// <exception cref="IndexOutOfRangeException">
        /// The start position is outside the bounds of the string.
        /// </exception>
        public RuneString Substring(int start)
            => new(data[start..]);

        /// <summary>
        /// Gets a portion of the rune string starting from a specified position and of the specified length.
        /// </summary>
        /// <param name="start">The offset from the front to start at.</param>
        /// <param name="length">How many runes to retrieve.</param>
        /// <returns>A new <see cref="RuneString"/> representing the indicated portion of the original.</returns>
        /// <remarks>This operation does not perform a copy—it is an O(1) operation.</remarks>
        /// <exception cref="IndexOutOfRangeException">
        /// The start or end position is outside the bounds of the string.
        /// </exception>
        public RuneString Substring(int start, int length)
            => new(data[start..(start + length)]);

        /// <summary>
        /// Converts the runes to an array of characters.
        /// </summary>
        /// <returns>The character array.</returns>
        /// <remarks>
        /// Because some runes convert to two characters, the length of the output may exceed the string's length. Also,
        /// this ouput may not match the original <see cref="string"/> if it had any invalid Unicode
        /// characters/sequences.
        /// </remarks>
        public char[] ToChars()
            => ToString().ToCharArray();

        /// <summary>
        /// Converts the runes to a <see cref="string"/>.
        /// </summary>
        /// <returns>The equivalent <see cref="string"/>.</returns>
        /// <remarks>
        /// Because some runes convert to two characters, the length of the output may exceed the string's length. Also,
        /// this ouput may not match the original <see cref="string"/> if it had any invalid Unicode
        /// characters/sequences.
        /// </remarks>
        public override string ToString()
            => string.Join("", data.Select(rune => rune.ToString()));

        /// <summary>
        /// Encodes the rune string in big-endian UTF-16 format.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        public byte[] ToUtf16BEBytes()
            => Encoding.BigEndianUnicode.GetBytes(ToString());

        /// <summary>
        /// Encodes the rune string in little-endian UTF-16 format.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        public byte[] ToUtf16LEBytes()
            => Encoding.Unicode.GetBytes(ToString());

        /// <summary>
        /// Encodes the rune string as a sequence of UTF-16 units.
        /// </summary>
        /// <returns>An array of 16-bit units.</returns>
        public ushort[] ToUtf16Units()
            => ToString().Select(c => (ushort)c).ToArray();

        /// <summary>
        /// Encodes the rune string in big-endian UTF-32 format.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        public byte[] ToUtf32BEBytes()
            => Utf32BE.GetBytes(ToString());

        /// <summary>
        /// Encodes the rune string in little-endian UTF-32 format.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        public byte[] ToUtf32LEBytes()
            => Encoding.UTF32.GetBytes(ToString());

        /// <summary>
        /// Encodes the rune string as a sequence of UTF-32 units.
        /// </summary>
        /// <returns>An array of 32-bit units.</returns>
        public uint[] ToUtf32Units()
            => data.Select(rune => (uint)rune.Value).ToArray();

        /// <summary>
        /// Encodes the rune string in UTF-8 format.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        /// <remarks>
        /// The output can be considered either a sequence of encoded bytes or as a sequence of 8-bit units--the
        /// concepts are identical for UTF-8. UTF-8 does not little-endian and big-endian varieties.
        /// </remarks>
        public byte[] ToUtf8()
            => Encoding.UTF8.GetBytes(ToString());

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
    }
}
