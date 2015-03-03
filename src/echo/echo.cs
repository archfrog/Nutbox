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

using System.Collections.Generic;	// List<T>
using Org.Nutbox.Options;			// ListValue()

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.echo")]
[assembly: AssemblyDescription("Displays the specified string while optionally parsing escape characters")]
[assembly: AssemblyConfiguration("SHIP")]
[assembly: AssemblyCompany("Mikael Lyngvig")]
[assembly: AssemblyProduct("Nutbox")]
[assembly: AssemblyCopyright("Copyleft (-) 2009-2015 Mikael Lyngvig")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyName("")]
[assembly: System.CLSCompliant(true)]

namespace Org.Nutbox.Echo
{
    class Setup: Org.Nutbox.Setup
    {
		private BooleanValue mEscapes = new BooleanValue(false);
		public bool Escapes
		{
			get { return mEscapes.Value; }
		}

		private BooleanValue mLineless = new BooleanValue(false);
		public bool Lineless
		{
			get { return mLineless.Value; }
		}

		private ListValue mWords = new ListValue();
		public List<string> Words
		{
			get { return mWords.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("e", mEscapes),
				new FalseOption("E", mEscapes),
				new TrueOption("n", mLineless),
				new ListParameter(1, "word", mWords, Option.eMode.Optional)
			};
			base.Add(options);
		}
    }

    class Program: Org.Nutbox.Program
    {
		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"echo",							// Program
			"v1.00",						// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights,	// Rights
			Org.Nutbox.Copyright.Support,	// Support
            Org.Nutbox.Copyright.Website,   // Website
			Org.Nutbox.Echo.Help.Text,		// Help
			Org.Nutbox.Copyright.Lower,		// Lower
			Org.Nutbox.Copyright.Upper		// Upper
		);

		public Program():
			base(_info)
		{
		}

		// note: FxCop does not like List<string> to be used in public methods!
		private static string Join(List<string> values, string separator)
		{
			if (values.Count == 0)
				return "";
			string result = values[0];
			for (int i = 1; i < values.Count; i += 1)
			{
				result += separator;
				result += values[i];
			}
			return result;
		}

        public override void Main(Org.Nutbox.Setup nutbox_setup)
        {
			Setup setup = (Setup) nutbox_setup;

			// convert the parameter(s) into an easy-to-use format
			string text = Join(setup.Words, " ");

			// if extension
			if (!setup.Escapes)
			{
				// output the string
				System.Console.Write(text);
				if (!setup.Lineless)
					System.Console.WriteLine();
				return;
			}

			// setup.Escapes (-e) is enable, process char by char
			bool escape = false;
			foreach (char ch in text)
			{
				if (!escape)
				{
					if (ch == '\\')
						escape = true;
					else
						System.Console.Write(ch);
					continue;
				}
				escape = false;

				// process escape sequence
				char output = ch;	// dummy assignment to make csc quiet
				switch (ch)
				{
					case '\\': output = '\\'; break;

					case 'a':
						System.Console.Beep();	// Windows: does not work?
						continue;

					case 'b': output = '\b'; break;

					// note: GNU 'echo' aborts completely without even printing
					// note: a newline character (perhaps a bug, perhaps not).
					case 'c': return;

					case 'f': output = '\f'; break;
					case 'n': output = '\n'; break;
					case 'r': output = '\r'; break;
					case 't': output = '\t'; break;
					case 'v': output = '\v'; break;

					case 'x':
						throw new Org.Nutbox.InternalError("Unsupported feature: \\x");

					case '0': goto case '9';
					case '1': goto case '9';
					case '2': goto case '9';
					case '3': goto case '9';
					case '4': goto case '9';
					case '5': goto case '9';
					case '6': goto case '9';
					case '7': goto case '9';
					case '8': goto case '9';
					case '9':
    					throw new Org.Nutbox.InternalError("Unsupported feature: Octal escape");
				}
				System.Console.Write(output);
			}

			if (!setup.Lineless)
				System.Console.WriteLine();
		}

		public static int Main(string[] args)
		{
			Setup setup     = new Setup();
			Program program = new Program();

			// let Org.Nutbox.Program.Main() handle exceptions, etc.
			return program.Main(setup, args);
		}
    }
}
