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

using System.Collections.Generic;	// Dictionary<string, string[]>
using Org.Lyngvig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.pathcheck")]
[assembly: AssemblyDescription("Performs various integrity checks on the PATH environment variable")]
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

namespace Org.Lyngvig.Nutbox.Pathcheck
{
    class Setup: Org.Lyngvig.Nutbox.Setup
    {
		private BooleanValue mElaborate = new BooleanValue(false);
		public bool Elaborate			// true => perform an elaborate check
		{
			get { return mElaborate.Value; }
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
				new TrueOption("e", mElaborate),
				new TrueOption("elaborate", mElaborate),
				new FalseOption("noelaborate", mElaborate),
				new StringOption("v", mVariable),
				new StringOption("variable", mVariable),
				new StringConstantOption("novariable", mVariable, "PATH")
			};
			base.Add(options);
		}
    }

    class Program: Org.Lyngvig.Nutbox.Program
    {
		static Org.Lyngvig.Nutbox.Information _info = new Org.Lyngvig.Nutbox.Information(
			"pathcheck",					// Program
			"v1.00",						// Version
			Org.Lyngvig.Nutbox.Copyright.Company,	// Company
			Org.Lyngvig.Nutbox.Copyright.Rights,	// Rights
			Org.Lyngvig.Nutbox.Copyright.Support,	// Support
            Org.Lyngvig.Nutbox.Copyright.Website,   // Website
			Org.Lyngvig.Nutbox.Pathcheck.Help.Text,	// Help
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

			// this is what it is all about
			string value = System.Environment.GetEnvironmentVariable(setup.Variable);
			if (value == null)
				throw new Org.Lyngvig.Nutbox.Exception("Environment variable not found: " + setup.Variable);

			// this is our dictionary of names in already processed directories
			// each key is the name of a file found in the path/variable
			// each value is a list of places where it has been found
			Dictionary<string, List<string>> Database= new Dictionary<string, List<string>>();

			// scan through each path element in the PATH var and report warnings
			// note: we do warnings so as to allow the user to see all problems
			// note: at the same time, so he or she does not have to run this
			// note: tool dozens of times to see all results.
			string[] paths = value.Split(System.IO.Path.PathSeparator);
			foreach (string path in paths)
			{
				// empty paths are potential trouble; some apps might choke on
				// them.  Others might treat them as the current dir and so on.
				if (path.Length == 0)
				{
					System.Console.WriteLine("Warning: Empty path detected");
					continue;
				}

				// check that the path is not a relative path; relative paths
				// always pose a GREAT security risk in any system.
				if (path != System.IO.Path.GetFullPath(path))
				{
					System.Console.WriteLine("Warning: Directory is relative: " + path);
					continue;
				}

				// check that the path item is pointing to an existing directory
				if (!System.IO.Directory.Exists(path))
				{
					System.Console.WriteLine("Warning: Directory does not exist: " + path);
					continue;
				}

				// check that the directory is not simply empty
				string[] files = System.IO.Directory.GetFiles(path);
				if (files.Length == 0)
				{
					System.Console.WriteLine("Warning: Directory is empty: " + path);
					continue;
				}

				// now for some serious analysis: find duplicate files
				foreach (string file in files)
				{
					// grab the base name of the path found
					string name = System.IO.Path.GetFileName(file);

					// a new file, not found elsewhere
					if (!Database.ContainsKey(name))
						Database[name] = new List<string>();

					// simply save it, we'll analyze later on
					Database[name].Add(path);
				}
			}

			if (setup.Elaborate)
			{
				// sort the keys to give a predictable and nice output
				string[] keys = new string[Database.Keys.Count];
				Database.Keys.CopyTo(keys, 0);
				System.Array.Sort(keys);

				// now report the duplicate files we've found, if any
				foreach (string key in keys)
				{
					// only one instance, nothing to report
					if (Database[key].Count == 1)
						continue;

					// multiple instances, report in full detail
					System.Console.WriteLine("Warning: Multiple locations detected for '{0}':", key);
					foreach (string dir in Database[key])
						System.Console.WriteLine("    {0}", dir);
				}
			}

			// well, that's it for now.
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
