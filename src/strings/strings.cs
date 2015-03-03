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

using System;
using System.Collections.Generic;
using Org.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.strings")]
[assembly: AssemblyDescription("Prints the ASCII or Unicode strings embedded in a binary file")]
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

namespace Org.Nutbox.Strings
{
    class Setup: Org.Nutbox.Setup
    {
		private BooleanValue mOffset = new BooleanValue(false);
		public bool Offset				// true => print file offsets
		{
			get { return mOffset.Value; }
		}

		private BooleanValue mUnicode = new BooleanValue(false);
		public bool Unicode				// true => parse as UTF-16 input
		{
			get { return mUnicode.Value; }
		}

		private IntegerValue mWidth = new IntegerValue(3);
		public int Width				// minimum length of a match
		{
			get { return mWidth.Value; }
		}

		private ListValue mWildcards = new ListValue();
		public string[] Wildcards
		{
			get { return mWildcards.Value.ToArray(); }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("offset", mOffset),
				new FalseOption("nooffset", mOffset),
				new TrueOption("unicode", mUnicode),
				new FalseOption("nounicode", mUnicode),
				new IntegerOption("width", mWidth),
				new IntegerConstantOption("nowidth", mWidth, 3),
				new ListParameter(1, "wildcard", mWildcards, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
    }

    // Program:
    // The program class that contains all the actual program code.
    class Program: Org.Nutbox.Program
    {
		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"strings",						// Program
			"v1.00",						// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights,	// Rights
			Org.Nutbox.Copyright.Support,	// Support
			Org.Nutbox.Strings.Help.Text,	// Help
			Org.Nutbox.Copyright.Lower,		// Lower
			Org.Nutbox.Copyright.Upper		// Upper
		);

		public Program(): 
			base(_info)
		{
		}

		/// <summary>
		/// Sort of macro that handles the -Offset option.</summary>
		/// <param name="name">The name of the source file to be printed.</param>
		/// <param name="offset">The offset value to be printed.</param>
		/// <param name="text">The text to be printed (the found match).</param>
		/// <param name="print_offset">If true, the offset is printed too.</param>
		/// <returns></returns>
		private static void Write(string name, long offset, string text, bool print_offset)
		{
			System.Console.Write("{0}", name);
			if (print_offset)
				System.Console.Write("({0})", offset);
			System.Console.WriteLine(": {0}", text.TrimEnd());
		}

        public override void Main(Org.Nutbox.Setup nutbox_setup)
        {
			Setup setup = (Setup) nutbox_setup;

			// validate parameters (ought to be done above, but, alas, no).
			if (setup.Width < 1)
				throw new Org.Nutbox.Exception("Invalid width specified: " + setup.Width.ToString());

			// expand wildcards into actual file names
			string[] files = Org.Nutbox.Platform.File.Find(setup.Wildcards, false);

			// display the strings embedded in the found files
			foreach (string file in files)
			{
				System.IO.FileStream reader = System.IO.File.Open(
					file,
					System.IO.FileMode.Open,
					System.IO.FileAccess.Read,
					System.IO.FileShare.Read
				);

				string text = "";
				long offset = -1;		// bogus value until assigned below
				for (;;)
				{
					// try to read a character from the input stream
					int value = reader.ReadByte();
					if (value == -1)
						break;

					// if parsing UTF-16, gather another byte before analysis
					if (setup.Unicode)
					{
						int temp = reader.ReadByte();
						if (temp == -1)
							break;

						value += 256 * temp;
					}

					char ch = (char) value;

					// only accept letters as the start of the string
					if (text.Length == 0)
					{
						if (Org.Nutbox.Platform.Chars.IsLetter(ch) ||
							ch == '(' ||
							ch == '{' ||
							ch == '[' ||
							ch == '<'
						)
						{
							offset = reader.Position - (setup.Unicode ? 2 : 1);
							text += ch;
							continue;
						}
					}
					else if (
						Org.Nutbox.Platform.Chars.IsLetter(ch) ||
						Org.Nutbox.Platform.Chars.IsDigit(ch) ||
						Org.Nutbox.Platform.Chars.IsPunct(ch) ||
						ch == ' '
					)
					{
						text += ch;
						continue;
					}

					if (text.Length >= setup.Width)
						Write(file, offset, text, setup.Offset);
					text = "";
				};

				// print the line in progress when the file ended, if any
				if (text.Length >= setup.Width)
				{
					Write(file, offset, text, setup.Offset);
					text = "";
				}
			}
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
