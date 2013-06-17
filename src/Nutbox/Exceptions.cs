#region license
// Copyleft (-) 2009-2012 Mikael Lyngvig (mikael@lyngvig.org).  Donated to the Public Domain.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following 
// conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the disclaimer below.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following 
//       disclaimer in the documentation and/or other materials provided with the distribution.
//     * Neither the name of Mikael Lyngvig nor the names of its contributors may be used to endorse or promote products derived 
//       from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
// SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

namespace Org.Nutbox
{
	using System.Runtime.Serialization;

	/// ShowHelpError:
	/// Thrown only if the user requests help through the use of an option.
	/// It is NOT really an error - the program exits with error code 0!
	[System.Serializable]
	public class ShowHelpError: System.Exception
	{
		public ShowHelpError()
		{
		}
	}

	[System.Serializable]
	public class InternalError: System.Exception
	{
		public InternalError()
		{
		}

		public InternalError(string message) :
			base(message)
		{
		}

		public InternalError(string message, System.Exception inner) :
			base(message, inner)
		{
		}

		protected InternalError(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
		}
	}

	// FxCop v1.36 whines on Nutbox using System.Exception, so we define a
	// wrapper class, which is Nutbox specific.
	// note: The most natural seems to derive from System.Exception; even MSDN
	// note: says so.
	[System.Serializable]
	public class Exception: System.Exception
	{
		// Exception():
		// Required by FxCop v1.36.
		public Exception()
		{
		}

		public Exception(string message) :
			base(message)
		{
		}

		// Exception():
		// Required by FxCop v1.36.
		public Exception(string message, System.Exception inner) :
			base(message, inner)
		{
		}

		// Exception():
		// Required by FxCop v1.36.
		protected Exception(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
		}
	}

	[System.Serializable]
	public class TextFileException: Exception
	{
		private string _name = "";
		public string Name
		{
			get { return _name; }
		}

		private int _line = 0;
		public int Line
		{
			get { return _line; }
		}

		private int _char = 0;
		public int Char
		{
			get { return _char; }
		}

		// Exception():
		// Required by FxCop v1.36.
		public TextFileException()
		{
		}

		// Exception(string, int, int, string):
		// This is the constructor that Nutbox uses.
		public TextFileException(string name, int line, int ch, string message) :
			base(message)
		{
			_name = name;
			_line = line;
			_char = ch;
		}

		// Exception(string, System.Exception):
		// Required by FxCop v1.36.
		public TextFileException(string message, System.Exception inner) :
			base(message, inner)
		{
		}

		// Exception(SerializationsInfo, StreamingContext):
		// Required by FxCop v1.36.
		protected TextFileException(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
		}
	}

	[System.Serializable]
	public class ExitWithExitCode: Exception
	{
		private int mCode = 0;
		public int Code
		{
			get { return mCode; }
		}

		public ExitWithExitCode()
		{
		}

		public ExitWithExitCode(int code)
		{
			mCode = code;
		}

		public ExitWithExitCode(string message, System.Exception inner) :
			base(message, inner)
		{
			throw new Nutbox.InternalError("This is indeed odd.");
		}

		protected ExitWithExitCode(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
		}
	}
}
