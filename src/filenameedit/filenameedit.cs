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
using Org.Egevig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.filenameedit")]
[assembly: AssemblyDescription("Replaces one or more strings in one or more file names")]
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

namespace Org.Egevig.Nutbox.Filenameedit
{
	class Setup: Org.Egevig.Nutbox.Setup
	{
		// note: due to the unusual syntax of the 'fileedit' command, we have
		// note: to gather up all parameters (non-options) in _parameters and
		// note: then later divide this list into patterns and wildcards.
		private ListValue _parameters = new ListValue();
		public List<string> Parameters
		{
			get { return _parameters.Value; }
		}

		private BooleanValue _recurse = new BooleanValue(false);
		public bool Recurse			// true => recurse subdirectories
		{
			get { return _recurse.Value; }
		}

		private BooleanValue _test = new BooleanValue(false);
		public bool TestOpt			// true => display changes without making them
		{
			get { return _test.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("recurse", _recurse),
				new FalseOption("norecurse", _recurse),
				new TrueOption("test", _test),
				new FalseOption("notest", _test),
				new ListParameter(1, "wildcard", _parameters, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
	}

	class Program: Org.Egevig.Nutbox.Program
	{
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"filenameedit",	   					// Program
			"v1.00",							// Version
			Org.Egevig.Nutbox.Copyright.Company,		// Company
			Org.Egevig.Nutbox.Copyright.Rights,		// Rights
			Org.Egevig.Nutbox.Copyright.Support,		// Support
            Org.Egevig.Nutbox.Copyright.Website,   // Website
			Org.Egevig.Nutbox.Filenameedit.Help.Text,	// Help
			Org.Egevig.Nutbox.Copyright.Lower,			// Lower
			Org.Egevig.Nutbox.Copyright.Upper			// Upper
		);

		public Program():
			base(_info)
		{
		}

		public override void Main(Org.Egevig.Nutbox.Setup nutbox_setup)
		{
			Setup setup = (Setup) nutbox_setup;

			// divide setup.Parameters into Patterns and Wildcards
			List<string> patterns = new List<string>();
			List<string> wildcards = new List<string>();
			foreach (string arg in setup.Parameters)
			{
				if (arg.IndexOf('=') != -1)
				{
					// check that the 'old' part of the pattern isn't the empty string
					int    pos = arg.IndexOf('=');
					string old = arg.Substring(0, pos);
					if (old.Length == 0)
						throw new Org.Egevig.Nutbox.Exception("Invalid pattern: " + arg);

					patterns.Add(arg);
				}
				else
					wildcards.Add(arg);

			}

			// manually check that one or more wildcards were given
			if (wildcards.Count == 0)
				throw new Org.Egevig.Nutbox.Exception("No wildcards specified");

			// expand wildcards
			string[] files = Org.Egevig.Nutbox.Platform.File.Find(wildcards.ToArray(), setup.Recurse);

			// manually check that the wildcards matched something
			if (files.Length == 0)
				throw new Org.Egevig.Nutbox.Exception("No files found matching specified wildcards");

			// check that each specified and files file actually exists
			foreach (string source in files)
			{
				string directory = System.IO.Path.GetDirectoryName(source);
				if (directory.Length > 0 && directory[directory.Length - 1] != System.IO.Path.DirectorySeparatorChar)
					directory += System.IO.Path.DirectorySeparatorChar;
				string filename  = System.IO.Path.GetFileNameWithoutExtension(source);
				string extension = System.IO.Path.GetExtension(source);

				string target = filename;
				foreach (string pattern in patterns)
				{
					int pos = pattern.IndexOf('=');
					if (pos == -1)
						throw new Org.Egevig.Nutbox.InternalError("Barf, choke, and die...");
					string old_pattern = pattern.Substring(0, pos);
					string new_pattern = pattern.Substring(pos + 1);

					target = target.Replace(old_pattern, new_pattern);
				}

				target = directory + target + extension;
				if (target == source)
					continue;

				if (setup.TestOpt)
				{
					System.Console.WriteLine("{0} => {1}", source, target);
					continue;
				}

				System.IO.File.Move(source, target);
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
