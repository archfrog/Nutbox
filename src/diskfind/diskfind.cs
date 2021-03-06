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

using Org.Egevig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.diskfind")]
[assembly: AssemblyDescription("Finds the disks that match a particular volume label")]
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

namespace Org.Egevig.Nutbox.Diskfind
{
    class Setup: Org.Egevig.Nutbox.Setup
    {
		private StringValue _label = new StringValue(null);
		public string Label
		{
			get { return _label.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new StringParameter(1, "label", _label, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
    }

    // Program:
    // The program class that contains all the actual program code.
    class Program: Nutbox.Program
    {
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"diskfind",						        // Program
			"v1.00",						        // Version
			Org.Egevig.Nutbox.Copyright.Company,	// Company
			Org.Egevig.Nutbox.Copyright.Rights,	    // Rights
			Org.Egevig.Nutbox.Copyright.Support,	// Support
            Org.Egevig.Nutbox.Copyright.Website,    // Website
			Org.Egevig.Nutbox.Diskfind.Help.Text,	// Help
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
			string target = setup.Label.ToUpperInvariant();

			int count = 0;				// number of matches found

			// get information about all drives in the system
			System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
			foreach (System.IO.DriveInfo drive in drives)
			{
				// ignore drives without media in them
				if (!drive.IsReady)
					continue;

				// ignore all non-matching drives
				if (drive.VolumeLabel.ToUpperInvariant() != target)
					continue;

				// check that the drive name is d:\
				if (drive.Name.Length != 3 || drive.Name[1] != ':')
					throw new Org.Egevig.Nutbox.Exception("Invalid drive name: " + drive.Name);
				if (drive.Name[2] != System.IO.Path.DirectorySeparatorChar)
					throw new Org.Egevig.Nutbox.Exception("Invalid drive name: " + drive.Name);

				// now print the drive letter only (eases scripting!)
				System.Console.WriteLine(drive.Name[0]);

				count += 1;
			}

			if (count == 0)
				throw new Org.Egevig.Nutbox.Exception("Disk named '" + setup.Label + "' not found");
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
