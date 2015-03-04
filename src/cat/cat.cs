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
using Org.Lyngvig.Nutbox.Options;			// ListValue()

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.cat")]
[assembly: AssemblyDescription("Copies one or more files to the standard output device")]
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

namespace Org.Lyngvig.Nutbox.Cat
{
    class Setup: Nutbox.Setup
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
				new ListParameter(1, "wildcard", mWildcards, Option.eMode.Optional)
			};
			base.Add(options);
		}
    }

    class Program: Org.Lyngvig.Nutbox.Program
    {
		static Org.Lyngvig.Nutbox.Information _info = new Org.Lyngvig.Nutbox.Information(
			"cat",						    // Program
			"v1.00",					    // Version
			Org.Lyngvig.Nutbox.Copyright.Company,	// Company
			Org.Lyngvig.Nutbox.Copyright.Rights,	// Rights
			Org.Lyngvig.Nutbox.Copyright.Support,	// Support
            Org.Lyngvig.Nutbox.Copyright.Website,   // Website
			Org.Lyngvig.Nutbox.Cat.Help.Text,		// Help
			Org.Lyngvig.Nutbox.Copyright.Lower,		// Lower
			Org.Lyngvig.Nutbox.Copyright.Upper		// Upper
		);

		public Program():
			base(_info)
		{
		}

		public static void ExecuteCat(System.IO.TextReader reader, System.IO.TextWriter writer)
		{
			// iterate over each line in the input
			for (;;)
			{
				// read a single line at a time
				string line = reader.ReadLine();
				if (line == null)
					break;

				// yup, basic input -> NOP -> output algorithm here
				writer.WriteLine(line);
			}
		}

        public override void Main(Org.Lyngvig.Nutbox.Setup nutbox_setup)
        {
			Setup setup = (Setup) nutbox_setup;

			// handle the simple case of input being the standard input device
			if (setup.Wildcards.Length == 0)
			{
				ExecuteCat(System.Console.In, System.Console.Out);
				return;
			}

			// handle the case of multiple input files
			// ... expand wildcards
			string[] files = Nutbox.Platform.File.Find(setup.Wildcards, false);

			// copy each file to the standard output device
			foreach (string file in files)
			{
				System.IO.TextReader reader = new System.IO.StreamReader(file, true);
				ExecuteCat(reader, System.Console.Out);
				reader.Close();
			}
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
