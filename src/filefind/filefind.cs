#region license
// Copyleft (-) 2009-2017 Mikael Egevig (mikael@egevig.org).  Donated to the Public Domain.
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

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Org.Egevig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.filefind")]
[assembly: AssemblyDescription("Finds a string matching a regular expression in one or more files")]
[assembly: AssemblyConfiguration("SHIP")]
[assembly: AssemblyCompany("Mikael Egevig")]
[assembly: AssemblyProduct("Nutbox")]
[assembly: AssemblyCopyright("Copyleft (-) 2009-2017 Mikael Egevig")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyName("")]
[assembly: System.CLSCompliant(true)]

namespace Org.Egevig.Nutbox.Filefind
{
	class Setup: Org.Egevig.Nutbox.Setup
	{
		private BooleanValue mAbsolute = new BooleanValue(false);
		public bool Absolute 	// true => display absolute file names
		{
			get { return mAbsolute.Value; }
		}

		private BooleanValue mCase = new BooleanValue(false);
		public bool Case				// true => perform a case-sensitive search
		{
			get { return mCase.Value; }
		}

		private BooleanValue mExact = new BooleanValue(false);
		public bool Exact				// true => perform an exact match (no regex)
		{
			get { return mExact.Value; }
		}

		private BooleanValue mLines = new BooleanValue(false);
		public bool Lines
		{
			get { return mLines.Value; }
		}

		private StringValue mPattern = new StringValue(null);
		public string Pattern
		{
			get { return mPattern.Value; }
		}

		private BooleanValue mRecurse = new BooleanValue(false);
		public bool Recurse				// true => recurse subdirectories
		{
			get { return mRecurse.Value; }
		}

		private BooleanValue mRevert = new BooleanValue(false);
		public bool Revert				// true => display only non-matching lines
		{
			get { return mRevert.Value; }
		}

		private BooleanValue mTrim = new BooleanValue(false);
		public bool Trim				// true => trim output prior to displaying it
		{
			get { return mTrim.Value; }
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
				new TrueOption("a", mAbsolute),
				new TrueOption("absolute", mAbsolute),
				new FalseOption("noabsolute", mAbsolute),
				new TrueOption("c", mCase),
				new TrueOption("case", mCase),
				new FalseOption("nocase", mCase),
				new TrueOption("e", mExact),
				new TrueOption("exact", mExact),
				new TrueOption("l", mLines),
				new TrueOption("lines", mLines),
				new FalseOption("noexact", mExact),
				new TrueOption("r", mRecurse),
				new TrueOption("recurse", mRecurse),
				new FalseOption("norecurse", mRecurse),
				new TrueOption("v", mRevert),
				new TrueOption("revert", mRevert),
				new FalseOption("norevert", mRevert),
				new TrueOption("t", mTrim),
				new TrueOption("trim", mTrim),
				new FalseOption("notrim", mTrim),
				new StringParameter(1, "pattern", mPattern, Option.eMode.Mandatory),
				new ListParameter(2, "wildcard", mWildcards, Option.eMode.Optional)
			};
			base.Add(options);
		}
	}

	class Program: Org.Egevig.Nutbox.Program
	{
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"filefind",						// Program
			"v1.11",						// Version
			Org.Egevig.Nutbox.Copyright.Company,	// Company
			Org.Egevig.Nutbox.Copyright.Rights,	// Rights
			Org.Egevig.Nutbox.Copyright.Support,	// Support
            Org.Egevig.Nutbox.Copyright.Website,   // Website
			Org.Egevig.Nutbox.Filefind.Help.Text,	// Help
			Org.Egevig.Nutbox.Copyright.Lower,		// Lower
			Org.Egevig.Nutbox.Copyright.Upper		// Upper
		);

		public Program():
			base(_info)
		{
		}

		public static void ExecuteFileFind(
			string name,
			System.IO.TextReader reader,
			System.IO.TextWriter writer,
			string pattern,
			Regex regex,
			bool Prefix,				// prefix the name of the file
			bool Case,
			bool Exact,
			bool Revert,
			bool Trim
		)
		{
			// assign this always to avoid complaints from CSC
			string upcasemPattern = pattern.ToUpper(); // requires: -nocase -exact

			// iterate through each line
			for (int index = 0; ; index += 1)
			{
				string line = reader.ReadLine();
				if (line == null)
					break;

				// ugly, but that's life sometimes
				// todo: change clumsy search code to use a delegate
				if (Exact)
				{
					if (Case)
					{
						if ((line.IndexOf(pattern) == -1) ^ Revert)
							continue;
					}
					else
					{
						if ((line.ToUpper().IndexOf(upcasemPattern) == -1) ^ Revert)
							continue;
					}
				}
				else if (!regex.IsMatch(line) ^ Revert)
					continue;

				// trim leading and trailing white-space for nicer output
				if (Trim)
					line = line.Trim();

				// output the matched line
				if (Prefix)
					writer.Write("{0}({1})  ", name, index + 1);
				writer.WriteLine("{0}", line);
			}
		}

		public override void Main(Nutbox.Setup nutbox_setup)
		{
			Setup setup = (Setup) nutbox_setup;

			// ... set up the regular expression matching engine
			RegexOptions flags = RegexOptions.Compiled;
			if (!setup.Case)
				flags |= RegexOptions.IgnoreCase;
			Regex regex = (setup.Exact ? null : new Regex(setup.Pattern, flags));

			// handle the simple case that input is equal to standard input
			if (setup.Wildcards.Length == 0)
			{
				ExecuteFileFind(
					"stdin",
					System.Console.In,
					System.Console.Out,
					setup.Pattern,
					regex,
					setup.Lines,		// Prefix
					setup.Case,
					setup.Exact,
					setup.Revert,
					setup.Trim
				);
				return;
			}

			// handle the complex case of one or more input files

			// expand wildcards
			string[] files = Org.Egevig.Nutbox.Platform.File.Find(setup.Wildcards, setup.Recurse);

			// check that each specified and found file actually exists
			foreach (string file in files)
			{
				if (!System.IO.File.Exists(file))
					throw new Org.Egevig.Nutbox.Exception("File not found: " + file);
			}

			// search each file in the list of files to be searched
			foreach (string file in files)
			{
				// ... process the Absolute option
				string name = file;
				if (setup.Absolute)
					name = System.IO.Path.GetFullPath(name);

				System.IO.StreamReader reader = new System.IO.StreamReader(name);
				ExecuteFileFind(
					name,
					reader,
					System.Console.Out,
					setup.Pattern,
					regex,
					(files.Length > 1) | setup.Lines,	// Prefix
					setup.Case,
					setup.Exact,
					setup.Revert,
					setup.Trim
				);
				reader.Close();
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
