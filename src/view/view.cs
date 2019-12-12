#region license
// Copyleft (-) 2009-2020 Mikael Egevig (mikael@egevig.org).  Donated to the Public Domain.
//
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following
// conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the disclaimer below.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following
//       disclaimer in the documentation and/or other materials provided with the distribution.
//     * Neither the name of Mikael Egevig nor the names of its contributors may be used to endorse or promote products derived
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
// I wanted to make this tool so that it could display standard input (sort of
// like a more contemporary version of 'less'), but .NET v2.0 does NOT support
// reading keys when standard input has been redirected.  This is one of those
// rather typical cases where .NET oozes of Visual Basic thinking.  If anyone
// succeeds in making a more/less clone, in C# for .NET 2.0, please let me
// know	how this feat was accomplished.
//****************************************************************************

using System.Collections.Generic;	// List<T>
using Org.Egevig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.view")]
[assembly: AssemblyDescription("Views the specified file(s) interactively.")]
[assembly: AssemblyConfiguration("SHIP")]
[assembly: AssemblyCompany("Mikael Egevig")]
[assembly: AssemblyProduct("Nutbox")]
[assembly: AssemblyCopyright("Copyleft (-) 2009-2020 Mikael Egevig")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.2.0")]
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyName("")]
[assembly: System.CLSCompliant(true)]

namespace Org.Egevig.Nutbox.View
{
    class Setup: Org.Egevig.Nutbox.Setup
    {
		private ListValue mWildcards = new ListValue();
		public string[] Wildcards
		{
			get { return mWildcards.Value.ToArray(); }
		}

		public Setup()
		{
			Option[] options =
			{
				new ListParameter(1, "wildcard", mWildcards, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
	}

    class Program: Org.Egevig.Nutbox.Program
    {
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"view",							// Program
			"v1.00",						// Version
			Org.Egevig.Nutbox.Copyright.Company,	// Company
			Org.Egevig.Nutbox.Copyright.Rights,	// Rights
			Org.Egevig.Nutbox.Copyright.Support,	// Support
            Org.Egevig.Nutbox.Copyright.Website,   // Website
			Org.Egevig.Nutbox.View.Help.Text,		// Help
			Org.Egevig.Nutbox.Copyright.Lower,		// Lower
			Org.Egevig.Nutbox.Copyright.Upper		// Upper
		);

		public Program():
			base(_info)
		{
		}

		public static void WriteFullLine(string value, int offsetX)
		{
			int width = System.Console.WindowWidth;

			while (value.Length < width + offsetX)
				value += ' ';
			value = value.Substring(offsetX, width);
			System.Console.Write(value);
		}

		public static string ExpandTabs(string value, int tabsize)
		{
			string result = "";

			if (tabsize <= 0)
				throw new Org.Egevig.Nutbox.InternalError("Invalid tabsize specified");

			int index = 0;
			foreach (char ch in value)
			{
				if (ch != '\t')
				{
					index  += 1;
					result += ch;
					continue;
				}

				int step = tabsize - (index % tabsize);
				result += new System.String(' ', step);
				index += step;
			}

			return result;
		}

		private static void DisplayLines(List<string> lines, int offsetX, int offsetY, int count)
		{
			// display as many lines as possible
			for (; count > 0; count -= 1)
			{
				if (offsetY == lines.Count)
					break;
				string line = lines[offsetY++];

				// expand tabs
				line = ExpandTabs(line, 4);

				WriteFullLine(line, offsetX);
			}

			// fill out with blank lines
			for (; count > 0; count -= 1)
			{
				WriteFullLine("", offsetX);
			}
		}

		private static int min(int a, int b)
		{
			return (a < b) ? a : b;
		}

		private static int max(int a, int b)
		{
			return (a > b) ? a : b;
		}

		public bool ExecuteView(System.IO.StreamReader reader)
		{
			bool result = false;			// true => exit the display loop

			// iterate over each line in the input
			List<string> lines = new List<string>();
			for (;;)
			{
				// read a single line at a time
				string line = reader.ReadLine();
				if (line == null)
					break;

				// buffer input for now
				lines.Add(line);
			}
			reader.Close();

			bool DisableCtrlC = System.Console.TreatControlCAsInput;
			System.Console.TreatControlCAsInput = true;
			bool CursorVisible = System.Console.CursorVisible;
			System.Console.CursorVisible = false;
			try
			{
				// display the buffered input
				int x = 1;				// use 1-based index for simpler logic
				int y = 1;				// use 1-based index for simpler logic
				bool done = false;
				int height = System.Console.WindowHeight;
				do
				{
					System.Console.SetCursorPosition(0, 0);
					DisplayLines(lines, x - 1, y - 1, height - 1);
					WriteFullLine("Commands: Q=quit, PgUp=Previous screen, PgDn=Next screen", 0);
					// note: must move cursor to (0, 0) or everything goes amok...
					System.Console.SetCursorPosition(0, 0);

					// read and process the user's command
					System.ConsoleKeyInfo key = System.Console.ReadKey(true);
					switch (key.Key)
					{
						case System.ConsoleKey.Enter:
							goto case System.ConsoleKey.DownArrow;

						case System.ConsoleKey.DownArrow:
							y = min(lines.Count, y + 1);
							break;

						case System.ConsoleKey.Backspace:
							goto case System.ConsoleKey.UpArrow;

						case System.ConsoleKey.UpArrow:
							y = max(1, y - 1);
							break;

						case System.ConsoleKey.LeftArrow:
							x = max(1, x - 1);
							break;

						case System.ConsoleKey.RightArrow:
							x += 1;
							break;

						case System.ConsoleKey.B:
							goto case System.ConsoleKey.PageUp;

						case System.ConsoleKey.PageUp:
							y = max(1, y - System.Console.WindowHeight - 1);
							break;

						case System.ConsoleKey.Spacebar:
							goto case System.ConsoleKey.PageDown;

						case System.ConsoleKey.PageDown:
							y = min(lines.Count, y + System.Console.WindowHeight - 1);
							break;

						case System.ConsoleKey.N:
							result = false;
							done = true;
							break;

						case System.ConsoleKey.Q:
							result = true;
							done = true;
							break;
					}
				} while (!done);

				System.Console.Clear();
			}
			finally
			{
				System.Console.CursorVisible = CursorVisible;
				System.Console.TreatControlCAsInput = DisableCtrlC;
			}
			return result;
		}

        public override void Main(Org.Egevig.Nutbox.Setup nutbox_setup)
        {
			Setup setup = (Setup) nutbox_setup;

#if STDIN
			// handle the simple case of input being the standard input device
			if (setup.Wildcards.Count == 0)
			{
				ExecuteView(new System.IO.StreamReader(System.Console.OpenStandardInput()));
				return;
			}
#endif

			// handle the case of multiple input files
			// ... expand wildcards
			string[] files = Org.Egevig.Nutbox.Platform.File.Find(setup.Wildcards, false);

			// view each file in turn
			foreach (string file in files)
			{
				System.IO.StreamReader reader = new System.IO.StreamReader(
					System.IO.File.Open(file, System.IO.FileMode.Open),
					true
				);
				bool exit = ExecuteView(reader);
				reader.Close();

				if (exit)
					break;
			}
		}

		public static int Main(string[] args)
		{
			Setup setup     = new Setup();
			Program program = new Program();

			// let Org.Egevig.Nutbox.Program.Main() handle exceptions, etc.
			return program.Main(setup, args);
		}
    }
}
