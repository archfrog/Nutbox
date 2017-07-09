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

using Org.Egevig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.sleep")]
[assembly: AssemblyDescription("Waits the specified amount of time")]
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

namespace Org.Egevig.Nutbox.Sleep
{
    class Setup: Org.Egevig.Nutbox.Setup
    {
		private StringValue mDuration = new StringValue(null);
		public string Duration
		{
			get { return mDuration.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new StringParameter(1, "duration", mDuration, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
    }

    class Program: Org.Egevig.Nutbox.Program
    {
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"sleep",						        // Program
			"v1.01",						        // Version
			Org.Egevig.Nutbox.Copyright.Company,	// Company
			Org.Egevig.Nutbox.Copyright.Rights,	    // Rights
			Org.Egevig.Nutbox.Copyright.Support,	// Support
            Org.Egevig.Nutbox.Copyright.Website,    // Website
			Org.Egevig.Nutbox.Sleep.Help.Text,		// Help
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

			// note: we get the duration as a string (I'm a bit lazy here)
			// note: the option parser ought to handle this case but no.
			// first try to parse as an integer (number of seconds), then
			// try to parse as a .NET v2.0 TimeSpan value.  .NET v2.0 defaults
			// to parsing a lone integer as the number of days.  But how often
			// do you need to sleep entire days (except in build systems)
			// compared to the need for sleeping seconds.
			System.TimeSpan Duration;
			int duration;
			if (setup.Duration.ToUpperInvariant() == "FOREVER")
				Duration = System.TimeSpan.MaxValue;
			else if (System.Int32.TryParse(setup.Duration, out duration))
				Duration = new System.TimeSpan(0, 0, 0, duration);
			else if (!System.TimeSpan.TryParse(setup.Duration, out Duration))
				throw new Org.Egevig.Nutbox.Exception("Invalid duration specified: " + setup.Duration);

			// if the user specified "forever", sleep forever
			if (Duration == System.TimeSpan.MaxValue)
			{
				System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
				return;
			}

			// sleep the specified amount of time
			System.Threading.Thread.Sleep(Duration);
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
