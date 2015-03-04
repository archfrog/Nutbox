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
using Org.Lyngvig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.head")]
[assembly: AssemblyDescription("Displays the first N lines of standard input or a file")]
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

namespace Org.Lyngvig.Nutbox.Head
{
    class Setup: Org.Lyngvig.Nutbox.Setup
    {
		private StringValue mFilename = new StringValue(null);
		public string Filename			// null => read from standard input
		{
			get { return mFilename.Value; }
		}

		private IntegerValue mLines = new IntegerValue(10);
		public int Lines 				// number of lines to print
		{
			get { return mLines.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new IntegerOption("n", mLines),
				new IntegerOption("lines", mLines),
				new IntegerConstantOption("nolines", mLines, 10),
				new StringParameter(1, "filename", mFilename, Option.eMode.Optional)
			};
			base.Add(options);
		}
    }

    // Program:
    // The program class that contains all the actual program code.
    class Program: Org.Lyngvig.Nutbox.Program
    {
		static Org.Lyngvig.Nutbox.Information _info = new Org.Lyngvig.Nutbox.Information(
			"head",							// Program
			"v1.01",						// Version
			Org.Lyngvig.Nutbox.Copyright.Company,	// Company
			Org.Lyngvig.Nutbox.Copyright.Rights,	// Rights
			Org.Lyngvig.Nutbox.Copyright.Support,	// Support
            Org.Lyngvig.Nutbox.Copyright.Website,   // Website
			Org.Lyngvig.Nutbox.Head.Help.Text,		// Help
			Org.Lyngvig.Nutbox.Copyright.Lower,		// Lower
			Org.Lyngvig.Nutbox.Copyright.Upper		// Upper
		);

		public Program():
			base(_info)
		{
		}

        public override void Main(Org.Lyngvig.Nutbox.Setup nutbox_setup)
        {
			Setup setup = (Setup) nutbox_setup;

			// parse the Lines option parameter (defaults to ten)
			int count = setup.Lines;
			if (count < 1)
				throw new Org.Lyngvig.Nutbox.Exception("Invalid option parameter: -lines:" + setup.Lines);

			// set up the input stream
			System.IO.TextReader source;
			if (setup.Filename == null)
				source = System.Console.In;
			else
				source = new System.IO.StreamReader(setup.Filename, true);

			// KISS: Keep It Simple, Silly!
			List<string> lines = new List<string>();
			for (;;)
			{
				// read a single line at a time
				string line = source.ReadLine();
				if (line == null)
					break;

				// if the buffer is full, drop the first line
				if (lines.Count < count)
					lines.Add(line);
			}

			// report the lines
			foreach (string line in  lines)
				System.Console.WriteLine("{0}", line);

			// clean up
			if (source != System.Console.In)
				source.Close();
		}

		public static int Main(string[] args)
		{
			Setup setup     = new Setup();
			Program program = new Program();

			// let Org.Lyngvig.Nutbox.Program.Main() handle exceptions, etc.
			return program.Main(setup, args);
		}
    }
}
