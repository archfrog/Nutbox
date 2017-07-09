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
[assembly: AssemblyTitle("Nutbox.filesplit")]
[assembly: AssemblyDescription("Splits a file into one or more pieces")]
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

namespace Org.Egevig.Nutbox.Filesplit
{
    class Setup: Org.Egevig.Nutbox.Setup
    {
		private LongValue _size = new LongValue(0);
		public long Size
		{
			get { return _size.Value; }
		}

		private StringValue _filename = new StringValue(null);
		public string Filename
		{
			get { return _filename.Value; }
		}

		private BooleanValue _verbose = new BooleanValue(false);
		public bool Verbose
		{
			get { return _verbose.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("verbose", _verbose),
				new FalseOption("noverbose", _verbose),
				new LongParameter(1, "size", _size, Option.eMode.Mandatory),
				new StringParameter(2, "filename", _filename, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
    }

    // Program:
    // The program class that contains all the actual program code.
    class Program: Org.Egevig.Nutbox.Program
    {
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"filesplit",					// Program
			"v1.00",						// Version
			Org.Egevig.Nutbox.Copyright.Company,	// Company
			Org.Egevig.Nutbox.Copyright.Rights,	// Rights
			Org.Egevig.Nutbox.Copyright.Support,	// Support
            Org.Egevig.Nutbox.Copyright.Website,   // Website
			Org.Egevig.Nutbox.Filesplit.Help.Text,	// Help
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

			// parse and validate the size parameter
			long size = setup.Size;
			if (size <= 0)
				throw new Org.Egevig.Nutbox.Exception("Size must be greater than zero");

			if (setup.Verbose)
				System.Console.WriteLine("Source: " + setup.Filename);

			// open the source file as gently as at all possible
			System.IO.FileStream source = new System.IO.FileStream(
				setup.Filename, 		    // path
				System.IO.FileMode.Open,	// filemode
				System.IO.FileAccess.Read,	// access
				System.IO.FileShare.Read,	// share,
				65536,						// buffersize
				System.IO.FileOptions.SequentialScan	// options
			);

			byte[] bytes = new byte[65536];
			for (int index = 1; source.Position < source.Length; index += 1)
			{
				// create new files in the CURRENT directory rather than in the
				// directory that holds the source file
				string targetname = System.IO.Path.GetFileName(setup.Filename);
				targetname += "." + index.ToString(
					"D3",
					System.Globalization.CultureInfo.InvariantCulture
				);

				if (setup.Verbose)
					System.Console.WriteLine("Target: {0}", targetname);

				// create target file
				System.IO.FileStream target = new System.IO.FileStream(
					targetname,
					System.IO.FileMode.Create,  // filemode
					System.IO.FileAccess.Write,	// access
					System.IO.FileShare.None,	// share,
					65536,						// buffersize
					System.IO.FileOptions.SequentialScan	// options
				);

				// copy the current fragment to the current target file
				long part = size;
				while (part > 0)
				{
					long step = (part < 65536 ? part : 65535);
					int  read = source.Read(bytes, 0, (int) step);
					target.Write(bytes, 0, read);

					part -= read;

					if (read < step)
						break;
				}

				target.Close();
			}

			source.Close();
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
