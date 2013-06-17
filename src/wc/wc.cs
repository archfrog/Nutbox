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

using System.Collections.Generic;	// List<T>
using Org.Nutbox.Options;			// ListValue()

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.wc")]
[assembly: AssemblyDescription("Counts the number of chars, word, and lines in one or more files")]
[assembly: AssemblyConfiguration("SHIP")]
[assembly: AssemblyCompany("Mikael Lyngvig")]
[assembly: AssemblyProduct("Nutbox")]
[assembly: AssemblyCopyright("Copyleft (-) 2009-2012 Mikael Lyngvig")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyName("")]
[assembly: System.CLSCompliant(true)]

namespace Org.Nutbox.Wc
{
    class Setup: Org.Nutbox.Setup
    {
		public const int CHARS  = 1;
		public const int LINES  = 2;
		public const int MAXLEN = 4;
		public const int WORDS  = 8;

		private int mBits = 0;
		public int Bits
		{
			get { return mBits; }
		}

		private ListValue mWildcards = new ListValue();
		public string[] Wildcards
		{
			get { return mWildcards.Value.ToArray(); }
		}

		int CharsHandler(string arg, int index)
		{
			mBits |= CHARS;
			return index;
		}

		int LinesHandler(string arg, int index)
		{
			mBits |= LINES;
			return index;
		}

		int MaxLenHandler(string arg, int index)
		{
			mBits |= MAXLEN;
			return index;
		}

		int WordsHandler(string arg, int index)
		{
			mBits |= WORDS;
			return index;
		}

		private BooleanValue _recurse = new BooleanValue(false);
		public bool Recurse			// true => recurse subdirectories
		{
			get { return _recurse.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				// note: This is an interesting case of old meets new: The
				// note: Org.Nutbox.Options module is NOT intended to handle bitwise
				// note: options (options that contribute to the same bitfield),
				// note: but that's how GNU WC works.  So we bridge the gap
				// note: between the two using the special DelegateOption class.
				new DelegateOption("c",     new DelegateOption.Handler(CharsHandler), false),
				new DelegateOption("bytes", new DelegateOption.Handler(CharsHandler), false),
				new DelegateOption("chars", new DelegateOption.Handler(CharsHandler), false),
				new DelegateOption("l",     new DelegateOption.Handler(LinesHandler), false),
				new DelegateOption("lines", new DelegateOption.Handler(LinesHandler), false),
				new DelegateOption("L",     new DelegateOption.Handler(MaxLenHandler), false),
				new DelegateOption("max-line-length", new DelegateOption.Handler(MaxLenHandler), false),
				new DelegateOption("w",     new DelegateOption.Handler(WordsHandler), false),
				new DelegateOption("words", new DelegateOption.Handler(WordsHandler), false),
				new TrueOption("r", _recurse),
				new TrueOption("recurse", _recurse),
				new FalseOption("norecurse", _recurse),
				new ListParameter(1, "wildcard", mWildcards, Option.eMode.Optional)
			};
			base.Add(options);
		}
    }

    class Program: Org.Nutbox.Program
    {
		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"wc",							// Program
			"v1.01",						// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights,	// Rights
			Org.Nutbox.Copyright.Support,	// Support
			Org.Nutbox.Wc.Help.Text,		// Help
			Org.Nutbox.Copyright.Lower,		// Lower
			Org.Nutbox.Copyright.Upper		// Upper
		);

		public Program(): 
			base(_info)
		{
		}

		public struct Counts
		{
			public int Chars;
			public int Words;
			public int Lines;
			public int MaxLen;			// length of longest line

			// todo: figure out how to define a user-defined operator in C#
			public void Add(Counts value)
			{
				Chars += value.Chars;
				Words += value.Words;
				Lines += value.Lines;
				MaxLen = (MaxLen > value.MaxLen) ? MaxLen : value.MaxLen;
			}
		};

		public static bool IsPrint(char value)
		{
			return !(System.Char.IsControl(value) || System.Char.IsWhiteSpace(value));
		}

		public static Counts WordCount(System.IO.TextReader reader)
		{
			Counts result = new Counts();

			// iterate over each line in the input
			int index = 0;				// index of char in current line
			bool skip = false;			// ignore newline after carriage-return
			bool word = false;			// true => we're processing a word
			for (;;)
			{
				// try to get a character
				int got = reader.Read();
				if (got == -1)
					break;

				char ch = (char) got;
				result.Chars += 1;

				// ignore newline after carriage-return
				if (skip && ch == '\n')
					continue;
				skip = false;

				if (IsPrint(ch))
				{
					index += 1;
					word = true;
					continue;
				}

				// register end of word, if we're inside a word
				if (word)
				{
					result.Words += 1;
					word = false;
				}

				// process non-printable character according to its kind
				switch (ch)
				{
					case ' ':
						index += 1;
						break;

					case '\t':
						index |= (8 - 1);
						index += 1;
						break;

					case '\r':
						skip = true; 	// ignore following newline, if any
						goto case '\n';

					case '\n':
						result.Lines += 1;

						if (index > result.MaxLen)
							result.MaxLen = index;
						index = 0;
						break;

					default :
						// we end up here if it is an 8-bit extended ASCII character
						index += 1;
						break;
				}
			}

			return result;
		}

		public static void Print(string filename, Counts value, int bits)
		{
			if (bits == 0)
				throw new Org.Nutbox.InternalError("Bitfield must be non-zero");

			if ((bits & Setup.LINES) != 0)
				System.Console.Write("{0,7}", value.Lines);
			if ((bits & Setup.WORDS) != 0)
				System.Console.Write(" {0,7}", value.Words);
			if ((bits & Setup.CHARS) != 0)
				System.Console.Write(" {0,7}", value.Chars);
			if ((bits & Setup.MAXLEN) != 0)
				System.Console.Write(" {0,7}", value.MaxLen);
			System.Console.WriteLine(" {0}", filename);
		}

        public override void Main(Org.Nutbox.Setup nutbox_setup)
        {
			Setup setup = (Setup) nutbox_setup;

			// assign default value to setup.Bits
			// note: must be done here to handle the options correctly!
			int bits = setup.Bits;
			if (bits == 0)
				bits = Setup.CHARS | Setup.WORDS | Setup.LINES;

			// handle the simple case of input being the standard input device
			if (setup.Wildcards.Length == 0)
			{
				Counts counts = WordCount(System.Console.In);
				Print("", counts, bits); // POSIX 'wc' uses an empty string as the name
				// note: no total when we're doing only a single file...
				return;
			}

			// handle the case of multiple input files
			// ... expand wildcards
			string[] files = Org.Nutbox.Platform.File.Find(setup.Wildcards, setup.Recurse);

			// report the stats of each file to the standard output device
			Counts total = new Counts();
			foreach (string file in files)
			{
				System.IO.TextReader reader = new System.IO.StreamReader(file, true);
				Counts counts = WordCount(reader);
				total.Add(counts);
				reader.Close();

				Print(file, counts, bits);
			}

			// print the total, if more than one file examined
			if (files.Length > 1)
				Print("total", total, bits);
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
