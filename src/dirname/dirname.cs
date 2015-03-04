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

using Org.Lyngvig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.dirname")]
[assembly: AssemblyDescription("The Nutbox version of the POSIX 'dirname' command")]
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

namespace Org.Lyngvig.Nutbox.Dirname
{
    class Setup: Org.Lyngvig.Nutbox.Setup
    {
		private StringValue mPath = new StringValue(null);
		public string Path
		{
			get { return mPath.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new StringParameter(1, "path", mPath, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
    }

    class Program: Org.Lyngvig.Nutbox.Program
    {
		static Org.Lyngvig.Nutbox.Information _info = new Org.Lyngvig.Nutbox.Information(
			"dirname",						// Program
			"v1.00",						// Version
			Org.Lyngvig.Nutbox.Copyright.Company,	// Company
			Org.Lyngvig.Nutbox.Copyright.Rights,	// Rights
			Org.Lyngvig.Nutbox.Copyright.Support,	// Support
            Org.Lyngvig.Nutbox.Copyright.Website,   // Website
			Org.Lyngvig.Nutbox.Dirname.Help.Text,	// Help
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
			char sep = System.IO.Path.DirectorySeparatorChar;

			string dirname = setup.Path;

			// handle some special cases (due to Windoze's illogical path system)
			bool special = false;
			special |= (dirname.Length == 1 && dirname[0] == sep);
			special |= (dirname.Length == 2 && dirname[1] == ':');
			special |= (dirname.Length == 3 && dirname[1] == ':' && dirname[2] == sep);
			special |= (dirname == "." || dirname == "..");

			if (!special)
			{
				// finally, we've arrived in the land of the sane...

				// ... drop trailing slash off path, if any
				// note: Org.Lyngvig.Nutbox.Options guarantee that no option is the empty string
				if (dirname[dirname.Length - 1] == System.IO.Path.DirectorySeparatorChar)
					dirname = dirname.Substring(0, dirname.Length - 1);

				// ... extract the directory part of the name
				dirname = System.IO.Path.GetDirectoryName(dirname);
			}


			// output the result
			System.Console.WriteLine(dirname);
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
