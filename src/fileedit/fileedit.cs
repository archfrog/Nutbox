#region license
// Copyleft (-) 2009-2020 Mikael Egevig (mikael@egevig.org).  Donated to the Public Domain.
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
[assembly: AssemblyTitle("Nutbox.fileedit")]
[assembly: AssemblyDescription("Replaces one or more strings in one or more files")]
[assembly: AssemblyConfiguration("SHIP")]
[assembly: AssemblyCompany("Mikael Egevig")]
[assembly: AssemblyProduct("Nutbox")]
[assembly: AssemblyCopyright("Copyleft (-) 2009-2020 Mikael Egevig")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.2.0")]
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyName("")]
[assembly: System.CLSCompliant(true)]

namespace Org.Egevig.Nutbox.Fileedit
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

		private BooleanValue _backup = new BooleanValue(false);
		public bool Backup				// true => save backup of original
		{
			get { return _backup.Value; }
		}

#if NUTBOX_FILEEDIT_CASE
		private BooleanValue _case = new BooleanValue(false);
		public bool Case			// true => perform a case-sensitive search
		{
			get { return _case.Value; }
		}
#endif
		private BooleanValue _recurse = new BooleanValue(false);
		public bool Recurse			// true => recurse subdirectories
		{
			get { return _recurse.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("b", _backup),
				new TrueOption("backup", _backup),
				new FalseOption("nobackup", _backup),
#if NUTBOX_FILEEDIT_CASE
				new TrueOption("case", _case),
				new FalseOption("nocase", _case),
#endif
				new TrueOption("r", _recurse),
				new TrueOption("recurse", _recurse),
				new FalseOption("norecurse", _recurse),
				new ListParameter(1, "wildcard", _parameters, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
	}

	class Program: Org.Egevig.Nutbox.Program
	{
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"fileedit",						        // Program
			"v1.03",						        // Version
			Org.Egevig.Nutbox.Copyright.Company,	// Company
			Org.Egevig.Nutbox.Copyright.Rights,	    // Rights
			Org.Egevig.Nutbox.Copyright.Support,	// Support
            Org.Egevig.Nutbox.Copyright.Website,    // Website
			Org.Egevig.Nutbox.Fileedit.Help.Text,	// Help
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
			List<string> patterns = new List<string>();
			List<string> wildcards = new List<string>();

			// divide setup.Parameters into Patterns and Wildcards
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

			// expand wildcards
			string[] files = Org.Egevig.Nutbox.Platform.File.Find(wildcards.ToArray(), setup.Recurse);

			// check that each specified and files file actually exists
			foreach (string file in files)
			{
				if (!System.IO.File.Exists(file))
					throw new Org.Egevig.Nutbox.Exception("File not files: " + file);
			}

			// search each file in the list of files to be searched
			// ... iterate through each file
			foreach (string file in files)
			{
				// note: we read all of the file into memory
				// note: this is done to let .NET detect encodings...
				// note: sometimes .NET reeks of Visual Basic programming
				// note: what if the file is a 6 gigabyte log file, ehm, M$?
				string source = System.IO.File.ReadAllText(file);

				// iterate through each pattern to replace
				string target = source;
				foreach (string pattern in patterns)
				{
					int sep = pattern.IndexOf('=');
					string first = pattern.Substring(0, sep);
					string other = pattern.Substring(sep + 1);

					target = target.Replace(first, other);
				}

				// write the output, if substitutions were made
				if (target != source)
				{
					if (setup.Backup)
					{
						string backup = file + ".bak";

						if (System.IO.File.Exists(backup))
							System.IO.File.Delete(backup);
						System.IO.File.Move(file, backup);
					}

					// let .NET handle encoding and all that
                    /** \todo Fix the problem that .NET does \b not select the optimal output encoding, but rather uses ASCII! */
					System.IO.File.WriteAllText(file, target);
				}
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
