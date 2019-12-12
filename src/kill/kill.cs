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
[assembly: AssemblyTitle("Nutbox.kill")]
[assembly: AssemblyDescription("Kills the specified process")]
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

namespace Org.Egevig.Nutbox.Kill
{
	class Setup: Org.Egevig.Nutbox.Setup
	{
		private BooleanValue mForce = new BooleanValue(false);
		public bool Force
		{
			get { return mForce.Value; }
		}

		private StringValue mProcess = new StringValue(null);
		public string Process
		{
			get { return mProcess.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("9", mForce),
				new TrueOption("force", mForce),
				new FalseOption("noforce", mForce),
				new StringParameter(1, "process", mProcess, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
	}

	class Program: Org.Egevig.Nutbox.Program
	{
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"kill",							        // Program
			"v1.00",						        // Version
			Org.Egevig.Nutbox.Copyright.Company,	// Company
			Org.Egevig.Nutbox.Copyright.Rights,	    // Rights
			Org.Egevig.Nutbox.Copyright.Support,	// Support
            Org.Egevig.Nutbox.Copyright.Website,    // Website
			Org.Egevig.Nutbox.Kill.Help.Text,		// Help
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

			// try to look up the process
			int id;
			System.Diagnostics.Process process;
			if (System.Int32.TryParse(setup.Process, out id))
			{
				process = System.Diagnostics.Process.GetProcessById(id);

				if (process == null)
					throw new Org.Egevig.Nutbox.Exception("Process not found");
			}
			else
			{
				System.Diagnostics.Process[] processes;
				processes = System.Diagnostics.Process.GetProcessesByName(setup.Process);

				// check that we got one and only one process
				if (processes.Length == 0)
					throw new Org.Egevig.Nutbox.Exception("Process not found");
				if (processes.Length > 1)
					throw new Org.Egevig.Nutbox.Exception("Multiple processes found");
				process = processes[0];
			}

			// now kill it
			if (setup.Force)
				process.Kill();
			else
				process.CloseMainWindow();
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
