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

using System;
using System.Collections.Generic;
using Org.Egevig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.rm")]
[assembly: AssemblyDescription("Removes one or more files and/or directories.")]
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

namespace Org.Egevig.Nutbox.Rm
{
    class Setup: Org.Egevig.Nutbox.Setup
    {
		private ListValue mWildcards = new ListValue();
		public List<string> Wildcards
		{
			get { return mWildcards.Value; }
		}

		private BooleanValue mForce = new BooleanValue(false);
		public bool Force
		{
			get { return mForce.Value; }
		}

		private BooleanValue mRecurse = new BooleanValue(false);
		public bool Recurse
		{
			get { return mRecurse.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("f", mForce),   	// POSIX-ish alias for --force
				new TrueOption("force", mForce),
				new FalseOption("noforce", mForce),
				new TrueOption("r", mRecurse),	// POSIX-ish alias for --recurse
				new TrueOption("recurse", mRecurse),
				new FalseOption("norecurse", mRecurse),
				new ListParameter(1, "wildcard", mWildcards, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
    }

    // Program:
    // The program class that contains all the actual program code.
    class Program: Org.Egevig.Nutbox.Program
    {
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"rm",							        // Program
			"v1.00",						        // Version
			Org.Egevig.Nutbox.Copyright.Company,	// Company
			Org.Egevig.Nutbox.Copyright.Rights,	    // Rights
			Org.Egevig.Nutbox.Copyright.Support,	// Support
            Org.Egevig.Nutbox.Copyright.Website,    // Website
			Org.Egevig.Nutbox.Rm.Help.Text,		    // Help
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

			// expand wildcards into actual file and directory names
			List<string> found = new List<string>();
			foreach (string wildcard in setup.Wildcards)
			{
				// handle file/dir names without wildcards
				if (wildcard.IndexOf('*') == -1 && wildcard.IndexOf('?') == -1)
				{
					found.Add(wildcard);
					continue;
				}

				// must be a file specification (rm does not do match on dirs)
				string[] matches = Org.Egevig.Nutbox.Platform.File.Find(wildcard, false);
				if (!setup.Force && matches.Length == 0)
					throw new Org.Egevig.Nutbox.Exception("No matching files found: " + wildcard);

				// add all matched items to the list of found items
				foreach (string match in matches)
					found.Add(match);
			}

			// remove all the found items
			foreach (string item in found)
			{
				// if a directory, remove it (possibly recursively)
				if (System.IO.Directory.Exists(item))
				{
					if (!setup.Recurse)
						throw new Org.Egevig.Nutbox.Exception("-recurse option not specified for directory: " + item);

					Org.Egevig.Nutbox.Platform.Directory.Delete(item, setup.Recurse, setup.Force);
					continue;
				}

				// if a file, remove it
				if (System.IO.File.Exists(item))
				{
					Org.Egevig.Nutbox.Platform.File.Delete(item, setup.Force);
					continue;
				}

				// non-existing item, report error if not -force
				if (!setup.Force)
					throw new Org.Egevig.Nutbox.Exception("Item not found: " + item);
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
