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

//****************************************************************************
// NOTICE:
// This version translates from Windows 1252 codepage to UTF-8 encoding.  It
// does so because _I_ happen to like Unicode more than I like Windoze.
// If you need more flexible functionality, please contact me at
// mikael@lyngvig.org.  But Unicode works well with _all_ browsers (including
// Apple Safari and Opera), for which reason you should probably use it.
//****************************************************************************

using Org.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.htm2html")]
[assembly: AssemblyDescription("Converts a Microsoft Office Word 20xx HTM file into HTML 4.01/Loose")]
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

namespace Org.Nutbox.Htm2html
{
    class Setup: Org.Nutbox.Setup
    {
		private StringValue _source = new StringValue(null);
		public string Source
		{
			get { return _source.Value; }
		}

		private StringValue _target = new StringValue(null);
		public string Target
		{
			get { return _target.Value; }
		}

		private BooleanValue _delete = new BooleanValue(false);
		public bool Delete
		{
			get { return _delete.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("delete", _delete),
				new FalseOption("nodelete", _delete),
				new StringParameter(1, "source", _source, Option.eMode.Mandatory),
				new StringParameter(2, "target", _target, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
    }

    // Program:
    // The program class that contains all the actual program code.
    class Program: Org.Nutbox.Program
    {
		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"htm2html",						// Program
			"v1.10",						// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights,	// Rights
			Org.Nutbox.Copyright.Support,	// Support
			Org.Nutbox.Htm2html.Help.Text,	// Help
			Org.Nutbox.Copyright.Lower,		// Lower
			Org.Nutbox.Copyright.Upper		// Upper
		);

		public Program(): 
			base(_info)
		{
		}

		public static void HtmToHtml(string source_name, string target_name)
		{
			string DOCTYPE = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">";

			System.Text.Encoding source_encoding = System.Text.Encoding.GetEncoding(1252);
			string source_data = System.IO.File.ReadAllText(source_name, source_encoding);

			// fix bad STYLE tag
			source_data = source_data.Replace("<style>", "<style type=\"text/css\">");

			// patch up charset tag (we save in UTF8 format!)
			source_data = source_data.Replace("charset=windows-1252", "charset=UTF-8");

			// fix bad "<a name>" and "<a href>" tags - delete leading underscore in
			// names as HTML 4.01 explicitly requires names to begin with a letter
			source_data = source_data.Replace("href=\"#_", "href=\"#");
			source_data = source_data.Replace("name=\"_", "name=\"");

			// prepend DOCTYPE tag (make it the first tag in the document)
			source_data = DOCTYPE + "\n" + source_data;

			// write result to targetname
			System.Text.Encoding target_encoding = new System.Text.UTF8Encoding();
			System.IO.File.WriteAllText(target_name, source_data, target_encoding);
		}

        public override void Main(Org.Nutbox.Setup nutbox_setup)
        {
			Setup setup = (Setup) nutbox_setup;

			string source = setup.Source;	// consistent coding...
			string target = setup.Target;
			if (target == null)
				target = System.IO.Path.ChangeExtension(setup.Source, ".html");

			HtmToHtml(source, target);
			if (setup.Delete)
				System.IO.File.Delete(source);
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
