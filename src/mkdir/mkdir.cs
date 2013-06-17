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

using System.Collections.Generic;	// List<T>
using Org.Nutbox.Options;			// XxxValue(), XxxOption, XxxParameter()

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.mkdir")]
[assembly: AssemblyDescription("Creates the specified directory and/or parent directories")]
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

namespace Org.Nutbox.Mkdir
{
	public class Setup: Org.Nutbox.Setup
	{
		private ListValue _directories = new ListValue();
		public List<string> Directories
		{
			get { return _directories.Value; }
		}

		private BooleanValue _parent = new BooleanValue(false);
		public bool Parent
		{
			get { return _parent.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("parent", _parent),
				new FalseOption("noparent", _parent),
				new ListParameter(1, "directory", _directories, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
	}

	class Program: Org.Nutbox.Program
	{
		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"mkdir",						// Program
			"v1.00",						// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights, 	// Rights
			Org.Nutbox.Copyright.Support,	// Support
			Org.Nutbox.Mkdir.Help.Text,		// Help
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

			foreach (string directory in setup.Directories)
			{
				if (!setup.Parent)
				{
					// if not creating parents, we must check if the parent exists
					if (System.IO.Directory.Exists(directory))
						throw new Org.Nutbox.Exception("Directory exists: " + directory);

					// build name of parent directory
					string parent = directory;
					if (parent[parent.Length - 1] != System.IO.Path.DirectorySeparatorChar)
						parent += System.IO.Path.DirectorySeparatorChar;
					parent += "..";			// yeah, yeah, but it works!

					// if the parent does not found, report an error
					if (!System.IO.Directory.Exists(parent))
						throw new Org.Nutbox.Exception("Cannot make directory: " + directory);
				}

				// now let .NET create the all intermediates and the dir itself
				System.IO.Directory.CreateDirectory(directory);
			}
		}

		static int Main(string[] args)
		{
			Setup setup     = new Setup();
			Program program = new Program();

			// let Org.Nutbox.Program.Main() handle exceptions, etc.
			return program.Main(setup, args);
		}
	}
}
