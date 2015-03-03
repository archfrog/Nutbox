#region license
// Copyleft (-) 2009-2015 Mikael Lyngvig (mikael@lyngvig.org).  Donated to the Public Domain.
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

//****************************************************************************
// Notice:
// The code in here is really, really pathetic.  But I haven't yet figured out
// a good way, in C#, to do GNU getopt() style processing of the input.
// Contributions are welcome!
//
// The current implementation is rather crude and has several issues:
//
//     1. The implementation does not handle "rm -fr foo bar"; each option has
//        to a separate option.
//     2. The implementation does not allow code reuse in the option parser;
//        to facilitate this, ParseLongOption() and ParseShortOption() should
//        be merged into one.  In effect, ParseLongOption() should be named
//        ParseOption() and should be called both for long and short options,
//        but this would mean that constructs such as "rm --f -r" were valid.
//****************************************************************************

namespace Org.Nutbox
{
	// ParseResult:
	// Used by the ParseXxx() methods to indicate the result of the operation.
	// If anything but Success is returned, a corresponding exception is thrown.
	// ParseResult is used to ensure a consistent and predictable result by all
	// commands.  Its purpose is NOT to reduce the size of the executables!
	public enum ParseResult
	{
		Extranous,			// an extranous option parameter was specified
		Ignore,				// the option should be treated as a parameter
		Invalid,			// an invalid option parameter was specified
		Missing,  			// the option parameter is missing
		Success,			// the option was parsed successfully
		Unknown   			// an unknown option
	};

	public class Setup : Nutbox.Options.Parser
	{
		private bool _logo = false;
		public bool Logo
		{
			get { return _logo; }
		}

		public Setup()
		{
		}

		public override void Parse(string arg)
		{
			switch (arg.ToUpperInvariant())
			{
				case "/?"    : goto case "--HELP";
				case "-?"    : goto case "--HELP";
				case "--HELP":
					throw new Nutbox.ShowHelpError();
			}

			base.Parse(arg);
		}
	}
}
