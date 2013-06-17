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

using Org.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.diskfree")]
[assembly: AssemblyDescription("Displays the number of bytes free on a disk")]
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

namespace Org.Nutbox.Diskfree
{
	class Setup: Org.Nutbox.Setup
	{
		private StringValue _target = new StringValue(null);
		public string Target
		{
			get { return _target.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new StringParameter(1, "target", _target, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
	}

	class Program: Org.Nutbox.Program
	{
		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"diskfree",						// Program
			"v1.00",						// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights,	// Rights
			Org.Nutbox.Copyright.Support,	// Support
			Org.Nutbox.Diskfree.Help.Text,	// Help
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

			if (!System.IO.Directory.Exists(setup.Target))
				throw new Org.Nutbox.Exception("Not a directory: " + setup.Target);

			string target = System.IO.Path.GetFullPath(setup.Target);
			if (target.Length > 0 && target[target.Length - 1] != System.IO.Path.DirectorySeparatorChar)
				target += System.IO.Path.DirectorySeparatorChar;
			if (target.Length < 2 || target[1] != ':')
				throw new Org.Nutbox.Exception("Unable to determine drive: " + setup.Target);
			string drive = target.Substring(0, 1);

			System.IO.DriveInfo info = new System.IO.DriveInfo(drive);
			long free = info.AvailableFreeSpace;
			System.Console.WriteLine("{0}", free);
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
