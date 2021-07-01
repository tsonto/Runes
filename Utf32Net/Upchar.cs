using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Utf32Net
{
    /// <summary> Represents a "user-perceived" character&mdash;what non-technical users would consider a single
    /// character of text. This concept is sometimes called a grapheme cluster or simply a grapheme. Ligatures
    /// formed of multiple base letters typically count as multiple upchars even if they are drawn as a single 
    /// symbol, since the reader still understands them as multiple letters.</summary>
    /// <remarks>
    /// <para>
    /// Multiletter ligature handling varies substantially from case to case. For such ligatures where Unicode
    /// provides a precomposed codepoint, such U+FB03 Latin Small Ligature FFI (ﬃ), that codepoint and any subsequent
    /// modifier characters are usually treated as a single upchar. Multiletter ligatures that do not have a
    /// Unicode precomposed codepoint, such as the Devanagari "mkbsh" conjunct consonant (म्क्ब्श), are usually
    /// treated as multiple upchars even though some fonts may render them as a single glyph. Ultimately the
    /// decision is up to the implementation of Microsoft's <see cref="StringInfo"/> class.
    /// </para>
    /// <para>
    /// The <a href="https://en.wikipedia.org/wiki/Zero-width_joiner">ZWJ character</a> (zero-width joiner), 
    /// which is mainly used as an instruction to drawn the letters on either side of it as a combined form, is 
    /// treated as a boundary between upchars, in which case it's included at the end of the preceding upchar. 
    /// The same is true of the <a href="https://en.wikipedia.org/wiki/Zero-width_non-joiner">ZWNJ character</a>
    /// (zero-width non-joiner).
    /// </para>
    /// <para>
    /// A major exception to that, norm, however, is in compound emoji, which may consist of multiple base emoji
    /// combined with a ZWJ. These are typically parsed as a single upchar, although fonts that don't support
    /// the combination may display them as multiple characters. <strong>Important:</strong> Unicode does not
    /// support aribtrary compound emoji&mdash;there is a list of allowed sequences, and any sequence not encode
    /// as a single upchar (and in comforming fonts will not combine as expected). For example, the "Women Holding 
    /// Hands: Medium-Dark Skin Tone, Light Skin Tone" emoji (👩🏾‍🤝‍👩🏻) defined in Emoji 12.0 is made of 7 codepoints across
    /// three ZWJ-delimited groups, and is treated as a single upchar.
    /// </para>
    /// </remarks>
	public readonly struct Upchar : IEquatable<Upchar>, IComparable<Upchar>
    {
        private Upchar(string textElement)
        {
            // Note: Upchar runes are always in Unicode normalization form C.
            array = RuneString.MakeRuneArrayFromString(textElement.Normalize());

            if (RuneStone.IsModifer(array[0]))
                throw new ArgumentException("The string does not begin with a base character.", nameof(textElement));
        }

        private Upchar(Rune[] runes)
        {
            array = runes;

            if (RuneStone.IsModifer(array[0]))
                throw new ArgumentException("The string does not begin with a base character.", nameof(runes));
        }

        /// <summary>
        /// Gets the foundational rune of the upchar.
        /// </summary>
        /// <remarks>
        /// This is the first codepoint of the upchar. It will never be a modifier. For some upchars&mdash;
        /// notably compound emoji&mdash;there may be multiple base runes combined with a the 
        /// <a href="https://en.wikipedia.org/wiki/Zero-width_joiner">ZWJ character</a>, in which case this
        /// property only gives the first base character.
        /// </remarks>
        public Rune BaseRune => array[0];

        //public Rune[] SubsequentRunes => array[1..];

        //public Rune? FinalJoiner
        //    => RuneStone.FinalJoiners.Contains(array[^1])
        //    ? array[^1] 
        //    : null;

        public bool IsCompound => array.Intersect(RuneStone.Joiners).Any();

        public Upchar[] GetComponents() => array.SplitInclusive(RuneStone.Joiners).Select(runes => new Upchar(runes)).ToArray();

        public Rune[] Runes
            => array.ToArray();

        internal int Utf16SequenceLength => array.Sum(r => r.Utf16SequenceLength);

        private readonly Rune[] array;

        public static UnicodeCategory GetUnicodeCategory(Upchar upchar)
                => Rune.GetUnicodeCategory(upchar.BaseRune);

        public static bool IsWhitespace(Upchar upchar)
            => Rune.IsWhiteSpace(upchar.BaseRune);

        public static bool operator !=(Upchar left, Upchar right) => !(left == right);

        public static UpcharString operator +(Upchar left, Upchar right)
            => new UpcharString(new[] { left, right }, raw: true);

        public static bool operator <(Upchar left, Upchar right) => left.CompareTo(right) < 0;

        public static bool operator <=(Upchar left, Upchar right) => left.CompareTo(right) <= 0;

        public static bool operator ==(Upchar left, Upchar right) => left.Equals(right);

        public static bool operator >(Upchar left, Upchar right) => left.CompareTo(right) > 0;

        public static bool operator >=(Upchar left, Upchar right) => left.CompareTo(right) >= 0;

        public static bool TryRead(ReadOnlySpan<Rune> s, ref int, out Upchar c)
        {
        }

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

        public int CompareTo(Upchar other)
        {
            for (int i = 0; i < array.Length && i < other.array.Length; ++i)
            {
                var runeComparison = array[i].CompareTo(other.array[i]);
                if (runeComparison != 0)
                    return runeComparison;
            }

            // If all the elements match up to the length of the shorter string, the the shorter string sorts before the
            // longer one.
            return array.Length.CompareTo(other.array.Length);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
                    => obj is Upchar g
            && Equals(g);

        public bool Equals(Upchar other)
            => array.SequenceEqual(other.array);

        public override int GetHashCode()
                    => array.GetHashCode();
    }
}
