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

using System.Collections.Generic;	// List<string>
using Org.Egevig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.txt2c")]
[assembly: AssemblyDescription("Converts a text file into a C source file")]
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

namespace Org.Egevig.Nutbox.Txt2c
{
    class Setup: Org.Egevig.Nutbox.Setup
    {
		public enum eFormat
		{
			Array,
			String
		}

		private StringValue mFormat = new StringValue("string");
		public eFormat Format
		{
			get
			{
				switch (mFormat.Value.ToUpperInvariant())
				{
					case "ARRAY": return eFormat.Array;
					case "STRING": return eFormat.String;
					default :
						throw new Nutbox.Exception("Invalid format: " + mFormat.Value);
				}
			}
		}

		private StringValue mName = new StringValue(null);
		public string Name
		{
			get { return mName.Value; }
		}

		private StringValue mSource = new StringValue(null);
		public string Source
		{
			get { return mSource.Value; }
		}

		private StringValue mTarget = new StringValue(null);
		public string Target
		{
			get { return mTarget.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new StringOption("format", mFormat),
				new StringConstantOption("noformat", mFormat, "string"),
				new StringParameter(1, "name", mName, Option.eMode.Mandatory),
				new StringParameter(2, "source", mSource, Option.eMode.Mandatory),
				new StringParameter(3, "target", mTarget, Option.eMode.Optional)
			};
			base.Add(options);
		}
    }

	class Program: Org.Egevig.Nutbox.Program
    {
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"txt2c",						        // Program
			"v1.00",						        // Version
			Org.Egevig.Nutbox.Copyright.Company,	// Company
			Org.Egevig.Nutbox.Copyright.Rights,	    // Rights
			Org.Egevig.Nutbox.Copyright.Support,	// Support
            Org.Egevig.Nutbox.Copyright.Website,    // Website
			Org.Egevig.Nutbox.Txt2c.Help.Text,		// Help
			Org.Egevig.Nutbox.Copyright.Lower,		// Lower
			Org.Egevig.Nutbox.Copyright.Upper		// Upper
		);

		public Program():
			base(_info)
		{
		}

		// quotes the value so as to make it a valid C# string literal
		public static string SourceQuote(string value)
		{
			string result = "";

			foreach (char ch in value)
			{
				switch (ch)
				{
					case '\0': result += "\\0";	break;
					case '\a': result += "\\a"; break;
					case '\b': result += "\\b"; break;
					case '\f': result += "\\f"; break;
					case '\n': result += "\\n"; break;
					case '\r': result += "\\r"; break;
					case '\t': result += "\\t"; break;
					case '\v': result += "\\v"; break;
					case '\'': result += "\\'"; break;
					case '\"': result += "\\\""; break;
					case '\\': result += "\\\\"; break;

					default:
						result += ch;
						break;
				}
			}

			return result;
		}

        public override void Main(Org.Egevig.Nutbox.Setup nutbox_setup)
        {
			Setup setup = (Setup) nutbox_setup;

			// load the source file into a list of lines
			List<string> lines = new List<string>();
			System.IO.StreamReader source = new System.IO.StreamReader(setup.Source);
			for (;;)
			{
				string line = source.ReadLine();
				if (line == null)
					break;

				lines.Add(line);
			}
			source.Close();

			// select the output file name (specified or computed)
			string targetname = setup.Target;
			if (targetname == null)
				targetname = setup.Source + ".c";
			System.IO.TextWriter target;
			target = new System.IO.StreamWriter(targetname);

			// write the prologue
			// todo: retrieve the product name (Nutbox) somewhere from
			target.WriteLine(
				"// C source file generated by Nutbox.{0} {1} on {2}.",
				_info.Program,
				_info.Version,
				Org.Egevig.Nutbox.Platform.Time.Standard()
			);
			target.WriteLine("// DO NOT EDIT THIS AUTOMATICALLY GENERATED SOURCE FILE!");
			switch (setup.Format)
			{
				case Setup.eFormat.Array:
					target.WriteLine("const char *{0}[] =", setup.Name);
					target.WriteLine("{");
					break;

				case Setup.eFormat.String:
					target.WriteLine("const char {0}[] = ", setup.Name);
					break;

				default:
					throw new Org.Egevig.Nutbox.Exception("Internal error - unexpected output format");
			}

			// write the contents
			for (int i = 0; i < lines.Count; i += 1)
			{
				string line = lines[i];

				switch (setup.Format)
				{
					case Setup.eFormat.Array:
						target.Write("\t\"{0}\"", SourceQuote(line));
						if (i < lines.Count - 1)
							target.Write(",");
						target.WriteLine();
						break;

					case Setup.eFormat.String:
						if (i < lines.Count - 1)
							line += "\n";
						target.WriteLine("\t\"{0}\"", SourceQuote(line));
						break;

					default:
						throw new Org.Egevig.Nutbox.Exception("Internal error - unexpected output format");
				}
			}

			// write the epilogue
			switch (setup.Format)
			{
				case Setup.eFormat.Array:
					target.WriteLine("};");
					break;

				case Setup.eFormat.String:
					target.WriteLine(";");
					break;

				default:
					throw new Org.Egevig.Nutbox.Exception("Internal error - unexpected output format");
			}

			if (target != System.Console.Out)
				target.Close();
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
