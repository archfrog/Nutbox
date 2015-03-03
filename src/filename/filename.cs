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

using Org.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.filename")]
[assembly: AssemblyDescription("Manipulates a file name according to a command")]
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

namespace Org.Nutbox.Filename
{
	class Setup: Org.Nutbox.Setup
	{
		private StringValue _command = new StringValue(null);
		public string Command
		{
			get { return _command.Value; }
		}

		private StringValue _path = new StringValue(null);
		public string Path
		{
			get { return _path.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new StringParameter(1, "command", _command, Option.eMode.Mandatory),
				new StringParameter(2, "path", _path, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
	}

    class Program: Org.Nutbox.Program
    {
		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"filename",					   	// Program
			"v1.02",					   	// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights,    // Rights
			Org.Nutbox.Copyright.Support,	// Support
			Org.Nutbox.Filename.Help.Text,	// HelpText
			Org.Nutbox.Copyright.Lower,		// Lower
			Org.Nutbox.Copyright.Upper		// Upper
		);

		public Program(): 
			base(_info)
		{
		}

		public override void Main(Nutbox.Setup nutbox_setup)
		{
			Setup setup = (Setup) nutbox_setup;

			string result;
			switch (setup.Command.ToUpperInvariant())
			{
				case "LOWER":
					result = setup.Path.ToLower(System.Globalization.CultureInfo.CurrentCulture);
					break;

				case "UPPER":
					result = setup.Path.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
					break;

				case "ABSOLUTE":
					result = System.IO.Path.GetFullPath(setup.Path);
					break;

				case "FILENAME":
					result = System.IO.Path.GetFileName(setup.Path);
					break;

				case "DIRNAME":
					result = System.IO.Path.GetDirectoryName(setup.Path);
					if (result.Length == 0)
						result = ".";
					break;

				case "DRIVENAME":
				{
					string fullpath = System.IO.Path.GetFullPath(setup.Path);
					if (fullpath.Length < 2 || fullpath[1] != ':')
						throw new Org.Nutbox.Exception("Unable to determine disk name: " + setup.Path);
					result = fullpath.Substring(0, 1).ToUpperInvariant();
					break;
				}

				case "DISKNAME":
				{
					string fullpath = System.IO.Path.GetFullPath(setup.Path);
					System.IO.DriveInfo drive = new System.IO.DriveInfo(fullpath.Substring(0, 2));
					result = drive.VolumeLabel;
					break;
				}

				case "NORMALIZE":
					result = Org.Nutbox.Platform.Disk.Normalize(setup.Path);
					break;

				case "EXTENSION":
					result = System.IO.Path.GetExtension(setup.Path);
					if (result.Length >= 1 && result[0] == '.')
						result = result.Substring(1);
					break;

				default:
					throw new Org.Nutbox.Exception("Unknown command: " + setup.Command);
			}

			// output the result
			System.Console.WriteLine("{0}", result);
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
