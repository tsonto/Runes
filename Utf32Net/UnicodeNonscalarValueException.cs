using System;
using System.Runtime.Serialization;

namespace Utf32Net
{
	[Serializable]
	internal class UnicodeNonscalarValueException : Exception
	{
		public UnicodeNonscalarValueException()
		{
		}

		public UnicodeNonscalarValueException(string message) : base(message)
		{
		}

		public UnicodeNonscalarValueException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected UnicodeNonscalarValueException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}