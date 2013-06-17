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

using System.Collections.Generic;
using Org.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.which")]
[assembly: AssemblyDescription("Locates all occurences of a file in the path")]
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

namespace Org.Nutbox.Which
{
	class Setup: Org.Nutbox.Setup
	{
		private BooleanValue mAll = new BooleanValue(false);
		public bool All
		{
			get { return mAll.Value; }
		}

		private ListValue mNames = new ListValue();
		public List<string> Names
		{
			get { return mNames.Value; }
		}

		private StringValue mVariable = new StringValue("PATH");
		public string Variable
		{
			get { return mVariable.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("a", mAll),
				new TrueOption("all", mAll),
				new FalseOption("noall", mAll),
				new StringOption("v", mVariable),
				new StringOption("variable", mVariable),
				new StringConstantOption("novariable", mVariable, "PATH"),
				new ListParameter(1, "filename", mNames, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
	}

	class Program: Org.Nutbox.Program
	{
		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"which",						// Program
			"v1.02",						// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights,	// Rights
			Org.Nutbox.Copyright.Support,	// Support
			Org.Nutbox.Which.Help.Text,		// Help
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

			// locate each specified name in turn
			foreach (string name in setup.Names)
			{
				// look up the environment variable and split it into its parts
				string   path = System.Environment.GetEnvironmentVariable(setup.Variable);
				if (path == null)
					throw new Org.Nutbox.Exception("Undefined environment variable: " + setup.Variable);
				string[] dirs = path.Split(System.IO.Path.PathSeparator);

				string[] locations = Org.Nutbox.Platform.File.Locate(dirs, name);
				if (locations.Length == 0)
					throw new Org.Nutbox.Exception("Unable to locate: " + name);

				// report all the found locations
				if (setup.All)
				{
					foreach (string location in locations)
						System.Console.WriteLine("{0}", location);
					continue;
				}

				// report only the first found location
				System.Console.WriteLine("{0}", locations[0]);
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
