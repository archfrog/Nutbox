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

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Org.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.monowrap")]
[assembly: AssemblyDescription("Generates a Unix shell script for invoking .NET applications using Mono")]
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

namespace Org.Nutbox.Monowrap
{
	class Setup: Org.Nutbox.Setup
	{
		private ListValue _wildcards = new ListValue();
		public string[] Wildcards
		{
			get { return _wildcards.Value.ToArray(); }
		}

		private BooleanValue _chmod = new BooleanValue(false);
		public bool Chmod					// true => invoke chmod
		{
			get { return _chmod.Value; }
		}

		private BooleanValue _recurse = new BooleanValue(false);
		public bool Recurse					// true => recurse subdirectories
		{
			get { return _recurse.Value; }
		}

		private StringValue _monopath = new StringValue("mono");
		public string Monopath				// command used for invoking mono
		{
			get { return _monopath.Value; }
		}

		private StringValue _shell = new StringValue("/bin/bash");
		public string Shell					// name of shell to invoke
		{
			get { return _shell.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("chmod", _chmod),
				new FalseOption("nochmod", _chmod),
				new StringOption("monopath", _monopath),
				new StringConstantOption("nomonopath", _monopath, "mono"),
				new TrueOption("recurse", _recurse),
				new FalseOption("norecurse", _recurse),
				new StringOption("shell", _shell),
				new StringConstantOption("noshell", _shell, "/bin/bash"),
				new ListParameter(1, "wildcard", _wildcards, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
	}

	class Program: Org.Nutbox.Program
	{
		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"monowrap",						// Program
			"v1.00",						// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights,	// Rights
			Org.Nutbox.Copyright.Support,	// Support
            Org.Nutbox.Copyright.Website,   // Website
			Org.Nutbox.Monowrap.Help.Text,	// Help
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
				// if not existing, throw an exception
				if (!System.IO.File.Exists(file))
					throw new Org.Nutbox.Exception("File not files: " + file);

				// if not ending in ".exe", throw an exception
				if (System.IO.Path.GetExtension(file) != ".exe")
					throw new Org.Nutbox.Exception("Invalid file type: " + file);
			}

			// search each file in the list of files to be searched

			// ... iterate through each file
			foreach (string file in files)
			{
				// ensure the script works globally from all locations
				string absname = System.IO.Path.GetFullPath(file);

				// remove the .exe extension (so we have the name of the script)
				string scriptname = file.Substring(0, file.LastIndexOf('.'));

				// generate the script file contents (two lines)
				string text = "";
				text += "#" + setup.Shell + "\n";
				text += Org.Nutbox.Platform.Shell.Quote(setup.Monopath) +
						" $MONO_OPTIONS " +
						Org.Nutbox.Platform.Shell.Quote(absname) +
						" $*\n";

				// write the script
				System.IO.File.WriteAllText(scriptname, text);

				// invoke 'chmod' to set the proper attributes
				if (setup.Chmod)
				{
					string[] arguments = {
						"+x-w",
						scriptname
					};
					Org.Nutbox.Platform.Process.Status status;
					status = Org.Nutbox.Platform.Process.Execute("chmod", arguments, false);
					if (status.Code != 0)
						throw new Org.Nutbox.Exception("Unable to set attributes: " + scriptname);
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
