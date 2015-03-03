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

using System.Collections.Generic;	// List<string>
using Org.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.trim")]
[assembly: AssemblyDescription("Trims excess white-space off the end of lines and empty lines off the end of file")]
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

namespace Org.Nutbox.Trim
{
    class Setup: Org.Nutbox.Setup
    {
		private ListValue mWildcards = new ListValue();
		public string[] Wildcards
		{
			get { return mWildcards.Value.ToArray(); }
		}

		private BooleanValue mRecurse = new BooleanValue(false);
		public bool Recurse				// true => recurse subdirectories
		{
			get { return mRecurse.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("r", mRecurse),
				new TrueOption("recurse", mRecurse),
				new FalseOption("norecurse", mRecurse),
				new ListParameter(1, "wildcard", mWildcards, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
    }

    class Program: Org.Nutbox.Program
    {
		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"trim",							// Program
			"v1.00",						// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights,	// Rights
			Org.Nutbox.Copyright.Support,	// Support
            Org.Nutbox.Copyright.Website,   // Website
			Org.Nutbox.Trim.Help.Text,		// Help
			Org.Nutbox.Copyright.Lower,		// Lower
			Org.Nutbox.Copyright.Upper		// Upper
		);

		public Program():
			base(_info)
		{
		}

        public override void Main(Org.Nutbox.Setup nutbox_setup)
        {
			Setup setup = (Setup) nutbox_setup;

			// expand wildcards
			string[] files = Org.Nutbox.Platform.File.Find(setup.Wildcards, setup.Recurse);

			// check that each specified and found file actually exists
			foreach (string file in files)
			{
				if (!System.IO.File.Exists(file))
					throw new Org.Nutbox.Exception("File not found: " + file);
			}

			// iterate over each file and trim it
			foreach (string file in files)
			{
				System.IO.StreamReader reader = new System.IO.StreamReader(file);
				List<string> lines  = new List<string>();

				// iterate until no more lines
				bool changed = false;
				for (;;)
				{
					// read a line and exit the loop if no more lines
					string line = reader.ReadLine();
					if (line == null)
						break;

					// trim the trailing white-space off the line
					string trimmed = line.TrimEnd();
					if (line != trimmed)
						changed = true;

					// cache the result
					lines.Add(trimmed);
				}

				// trim empty lines off the end (using lame algorithm)
				for (int i = lines.Count - 1; i >= 0; i--)
				{
					if (lines[i].Length != 0)
						break;

					changed = true;
					lines.RemoveAt(i);
				}

				// clean up (required to get access to the file)
				reader.Close();

				// only write the file if it has been changed
				if (changed)
				{
					System.IO.StreamWriter writer = new System.IO.StreamWriter(file);

					// write the trimmed lines to the output file
					foreach (string line in lines)
						writer.WriteLine(line);

					// yup, let's not rely too much on the destructor
					writer.Close();
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
