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

using System.Collections.Generic;	// List<T>
using Org.Egevig.Nutbox.Options;			// ListValue()

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.yes")]
[assembly: AssemblyDescription("Displays the specified string repeatedly until terminated")]
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

namespace Org.Egevig.Nutbox.Yes
{
    class Setup: Org.Egevig.Nutbox.Setup
    {
		private ListValue mWords = new ListValue();
		public List<string> Words
		{
			get { return mWords.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new ListParameter(1, "word", mWords, Option.eMode.Optional)
			};
			base.Add(options);
		}
    }

    class Program: Org.Egevig.Nutbox.Program
    {
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"yes",							        // Program
			"v1.00",						        // Version
			Org.Egevig.Nutbox.Copyright.Company,	// Company
			Org.Egevig.Nutbox.Copyright.Rights,	    // Rights
			Org.Egevig.Nutbox.Copyright.Support,	// Support
            Org.Egevig.Nutbox.Copyright.Website,    // Website
			Org.Egevig.Nutbox.Yes.Help.Text,		// Help
			Org.Egevig.Nutbox.Copyright.Lower,		// Lower
			Org.Egevig.Nutbox.Copyright.Upper		// Upper
		);

		public Program():
			base(_info)
		{
		}

		// note: FxCop does not like List<string> to be used in public methods!
		private static string Join(List<string> values, string separator)
		{
			if (values.Count == 0)
				return "";
			string result = values[0];
			for (int i = 1; i < values.Count; i += 1)
			{
				result += separator;
				result += values[i];
			}
			return result;
		}

        public override void Main(Org.Egevig.Nutbox.Setup nutbox_setup)
        {
			Setup setup = (Setup) nutbox_setup;

			// join the parameters into one easy-to-use string
			string text = Join(setup.Words, " ");
			if (text.Length == 0)
				text = "y";

			// output the string forever
			for (;;)
				System.Console.WriteLine(text);
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
