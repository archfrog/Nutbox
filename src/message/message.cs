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

using System.Collections.Generic;
using System.Windows.Forms;
using Org.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.message")]
[assembly: AssemblyDescription("Displays a message and waits for user input")]
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

namespace Org.Nutbox.Message
{
	class Setup: Org.Nutbox.Setup
	{
		private StringValue _title = new StringValue("");
		public string Title
		{
			get { return _title.Value; }
		}

		private ListValue _words = new ListValue();
		public List<string> Words
		{
			get { return _words.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new StringOption("title", _title),
				new StringConstantOption("notitle", _title, ""),
				new ListParameter(1, "word", _words, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
	}

	class Program: Org.Nutbox.Program
	{
		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"message",	   					// Program
			"v1.00",						// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights,	// Rights
			Org.Nutbox.Copyright.Support,	// Support
            Org.Nutbox.Copyright.Website,   // Website
			Org.Nutbox.Message.Help.Text,	// Help
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

			// join words into a single phrase
			string phrase = "";
			foreach (string word in setup.Words)
			{
				if (phrase.Length > 0)
					phrase += ' ';
				phrase += word;
			}

			// determine the title to use
			string title = setup.Title;
			if (title == null || title == "")
				title = "Message";

			// let the system handle the rest
			MessageBox.Show(phrase, title);
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
