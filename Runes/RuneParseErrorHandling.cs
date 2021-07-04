using System;

namespace Tsonto.System.Text
{
    /// <summary>
    /// Specifies how to handle invalid Unicode characters/sequences.
    /// </summary>
    public enum RuneParseErrorHandling
    {
        /// <summary>
        /// Emit the Unicode replacement character (U+FFFD).
        /// </summary>
        UseReplacementCharacter,

        /// <summary>
        /// Throw an <see cref="ArgumentException"/>.
        /// </summary>
        ThrowException,

        /// <summary>
        /// Advance to the next position and retry.
        /// </summary>
        OmitCharacter,
    }
}
