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

using System.Collections.Generic;	// List<string>
using Org.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.txt2cs")]
[assembly: AssemblyDescription("Converts a text file to a C# source file")]
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

namespace Org.Nutbox.Txt2cs
{
    class Setup: Org.Nutbox.Setup
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
						throw new Org.Nutbox.Exception("Invalid format: " + mFormat.Value);
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

        public override void Check()
        {
			base.Check();

			if (Name.Split('.').Length < 3)
				throw new Org.Nutbox.Exception("Too few elements in name: " + Name);
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

	class Program: Org.Nutbox.Program
    {
		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"txt2cs",						// Program
			"v1.02",						// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights,	// Rights
			Org.Nutbox.Copyright.Support,	// Support
			Org.Nutbox.Txt2cs.Help.Text,	// Help
			Org.Nutbox.Copyright.Lower,		// Lower
			Org.Nutbox.Copyright.Upper		// Upper
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
					// note: don't quote single quote; it is valid in C#
					case '\"': result += "\\\""; break;
					case '\\': result += "\\\\"; break;

					// todo: specific Unicode characters?

					default:
						result += ch;
						break;
				}
			}

			return result;
		}

        public override void Main(Org.Nutbox.Setup nutbox_setup)
        {
			Setup setup = (Setup) nutbox_setup;

			// parse name
			string[] parts    = setup.Name.Split('.');
			string namespace_ = System.String.Join(".", parts, 0, parts.Length - 2);
			string class_     = parts[parts.Length - 2];
			string variable_  = parts[parts.Length - 1];

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
				targetname = setup.Source + ".cs";
			System.IO.TextWriter target;
			target = new System.IO.StreamWriter(targetname);

			// write the prologue
			// todo: retrieve the product name (Nutbox) somewhere from
			target.WriteLine(
				"// C# source file generated by Nutbox.{0} {1} on {2}.",
				_info.Program,
				_info.Version,
				Org.Nutbox.Platform.Time.Standard()
			);
			target.WriteLine("// DO NOT EDIT THIS AUTOMATICALLY GENERATED SOURCE FILE!");
			target.WriteLine("namespace {0}", namespace_);
			target.WriteLine("{");
			target.WriteLine("\tpublic sealed class {0}", class_);
			target.WriteLine("\t{");
			target.WriteLine("// Satisfy FxCop v1.36 by sealing the class and making a private constructor.");
			target.WriteLine("\t\tprivate {0}()", class_);
			target.WriteLine("\t\t{");
			target.WriteLine("\t\t}");
			target.WriteLine();
			switch (setup.Format)
			{
				case Setup.eFormat.Array:
					target.WriteLine("\t\tpublic static string[] {0} =", variable_);
					target.WriteLine("\t\t{");
					break;

				case Setup.eFormat.String:
					target.WriteLine("\t\tpublic const string {0} = ", variable_);
					break;

				default:
					throw new Org.Nutbox.Exception("Internal error - unexpected output format");
			}

			// write the contents
			for (int i = 0; i < lines.Count; i += 1)
			{
				string line = lines[i];

				switch (setup.Format)
				{
					case Setup.eFormat.Array:
						target.Write("\t\t\t\"{0}\"", SourceQuote(line));
						if (i < lines.Count - 1)
							target.Write(",");
						target.WriteLine();
						break;

					case Setup.eFormat.String:
						if (i < lines.Count - 1)
							line += "\n";
						target.Write("\t\t\t\"{0}\"", SourceQuote(line));
						if (i < lines.Count - 1)
							target.Write(" +");
						target.WriteLine();
						break;

					default:
						throw new Org.Nutbox.Exception("Internal error - unexpected output format");
				}
			}

			// write the epilogue
			switch (setup.Format)
			{
				case Setup.eFormat.Array:
					target.WriteLine("\t\t};");
					break;

				case Setup.eFormat.String:
					target.WriteLine("\t\t;", variable_);
					break;

				default:
					throw new Org.Nutbox.Exception("Internal error - unexpected output format");
			}
			target.WriteLine("\t}");
			target.WriteLine("}");

			if (target != System.Console.Out)
				target.Close();
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
