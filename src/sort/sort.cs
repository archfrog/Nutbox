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

using System.Collections.Generic;	// List<T>
using Org.Egevig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.sort")]
[assembly: AssemblyDescription("Sorts the input and writes it to the standard output")]
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

namespace Org.Egevig.Nutbox.Sort
{
    class Setup: Org.Egevig.Nutbox.Setup
    {
		private BooleanValue mCase = new BooleanValue(false);
		public bool   Case
		{
			get { return mCase.Value; }
		}

		private StringValue mSource = new StringValue(null);
		public string Source 		// null => read from standard input
		{
			get { return mSource.Value; }
		}

		private StringValue mTarget = new StringValue(null);
		public string Target		// null => write to standard output
		{
			get { return mTarget.Value; }
		}

		private BooleanValue mReverse = new BooleanValue(false);
		public bool Reverse
		{
			get { return mReverse.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("c", mCase),
				new TrueOption("case", mCase),
				new FalseOption("nocase", mCase),
				new StringOption("o", mTarget),
				new StringOption("output", mTarget),
				new StringConstantOption("nooutput", mTarget, null),
				new TrueOption("r", mReverse),
				new TrueOption("reverse", mReverse),
				new FalseOption("noreverse", mReverse),
				new StringParameter(1, "source", mSource, Option.eMode.Optional)
			};
			base.Add(options);
		}
    }

	class StringCompareCase: System.Collections.IComparer
	{
		int System.Collections.IComparer.Compare(object x, object y)
		{
			return Org.Egevig.Nutbox.Platform.String.strcmp((string) x, (string) y);
		}
	}

	class StringCompareCaseReverse: System.Collections.IComparer
	{
		int System.Collections.IComparer.Compare(object x, object y)
		{
			return Org.Egevig.Nutbox.Platform.String.strcmp((string) y, (string) x);
		}
	}

	class StringCompareNoCase: System.Collections.IComparer
	{
		int System.Collections.IComparer.Compare(object x, object y)
		{
			return Org.Egevig.Nutbox.Platform.String.stricmp((string) x, (string) y);
		}
	}

	class StringCompareNoCaseReverse: System.Collections.IComparer
	{
		int System.Collections.IComparer.Compare(object x, object y)
		{
			return Org.Egevig.Nutbox.Platform.String.stricmp((string) y, (string) x);
		}
	}

    class Program: Org.Egevig.Nutbox.Program
    {
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"sort",							        // Program
			"v1.00",						        // Version
			Org.Egevig.Nutbox.Copyright.Company,	// Company
			Org.Egevig.Nutbox.Copyright.Rights,	    // Rights
			Org.Egevig.Nutbox.Copyright.Support,	// Support
            Org.Egevig.Nutbox.Copyright.Website,    // Website
			Org.Egevig.Nutbox.Sort.Help.Text,		// Help
			Org.Egevig.Nutbox.Copyright.Lower,		// Lower
			Org.Egevig.Nutbox.Copyright.Upper		// Upper
		);

		public Program():
			base(_info)
		{
		}

        public override void Main(Org.Egevig.Nutbox.Setup nutbox_setup)
        {
			Setup setup = (Setup) nutbox_setup;

#if TEST
			// test my ad-hoc string comparison functions...
			System.Diagnostics.Debug.Assert(Org.Egevig.Nutbox.Platform.String.strcmp("", "") == 0);
			System.Diagnostics.Debug.Assert(Org.Egevig.Nutbox.Platform.String.strcmp("APE", "ape") == -1);
			System.Diagnostics.Debug.Assert(Org.Egevig.Nutbox.Platform.String.strcmp("ape", "APE") == 1);
			System.Diagnostics.Debug.Assert(Org.Egevig.Nutbox.Platform.String.strcmp("", "APE") == -1);
			System.Diagnostics.Debug.Assert(Org.Egevig.Nutbox.Platform.String.strcmp("APE", "") == 1);
			System.Diagnostics.Debug.Assert(Org.Egevig.Nutbox.Platform.String.stricmp("", "") == 0);
			System.Diagnostics.Debug.Assert(Org.Egevig.Nutbox.Platform.String.stricmp("APE", "ape") == 0);
			System.Diagnostics.Debug.Assert(Org.Egevig.Nutbox.Platform.String.stricmp("ape", "APE") == 0);
			System.Diagnostics.Debug.Assert(Org.Egevig.Nutbox.Platform.String.stricmp("", "APE") == -1);
			System.Diagnostics.Debug.Assert(Org.Egevig.Nutbox.Platform.String.stricmp("APE", "") == 1);
#endif

			// set up the input stream
			System.IO.TextReader source;
			if (setup.Source == null)
				source = System.Console.In;
			else
				source = new System.IO.StreamReader(setup.Source, true);

			// read the input file into a list of lines
			List<string> lines = new List<string>();
			for (;;)
			{
				// read a single line at a time
				string line = source.ReadLine();
				if (line == null)
					break;

				lines.Add(line);
			}

			// clean up
			if (source != System.Console.In)
				source.Close();

			// convert the list of lines to an array
			string[] strings = lines.ToArray();

			// figure out which comparer to use and sort accordingly
			System.Collections.IComparer comparer;
			if (setup.Case && setup.Reverse)
				comparer = new StringCompareCaseReverse();
			else if (setup.Case)
				comparer = new StringCompareCase();
			else if (setup.Reverse)
				comparer = new StringCompareNoCaseReverse();
			else
				comparer = new StringCompareNoCase();
			System.Array.Sort(strings, comparer);

			// set up the output stream
			System.IO.TextWriter target;
			if (setup.Target == null)
				target = System.Console.Out;
			else
				target = System.IO.File.CreateText(setup.Target);

			// output the result
			foreach (string line in strings)
				target.WriteLine(line);

			// clean up
			if (target != System.Console.Out)
				target.Close();
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
