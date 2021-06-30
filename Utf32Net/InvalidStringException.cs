using System;
using System.Runtime.Serialization;

namespace Utf32Net
{
	[Serializable]
	internal class InvalidStringException : Exception
	{
		public InvalidStringException()
		{
		}

		public InvalidStringException(string message) : base(message)
		{
		}

		public InvalidStringException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidStringException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}