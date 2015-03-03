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
[assembly: AssemblyTitle("Nutbox.diskfill")]
[assembly: AssemblyDescription("Fills a disk with data until it is nearly full")]
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

namespace Org.Nutbox.Diskfill
{
	class Setup: Nutbox.Setup
	{
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

		private LongValue _reserve = new LongValue(0);
		public long Reserve
		{
			get { return _reserve.Value; }
		}

		public Setup()
		{
			Option[] options =
			{
				new TrueOption("delete", _delete),
				new FalseOption("nodelete", _delete),
				new LongOption("reserve", _reserve),
				new LongConstantOption("noreserve", _reserve, 0),
				new StringParameter(1, "target", _target, Option.eMode.Mandatory)
			};
			base.Add(options);
		}
	}

	class Program: Org.Nutbox.Program
	{
		const int BLOCKSIZE = 65536;

		static Org.Nutbox.Information _info = new Org.Nutbox.Information(
			"diskfill",						// Program
			"v1.11",						// Version
			Org.Nutbox.Copyright.Company,	// Company
			Org.Nutbox.Copyright.Rights,	// Rights
			Org.Nutbox.Copyright.Support,	// Support
			Org.Nutbox.Diskfill.Help.Text,	// Help
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

			string target = System.IO.Path.GetFullPath(setup.Target);
			if (target.Length > 0 && target[target.Length - 1] != System.IO.Path.DirectorySeparatorChar)
				target += System.IO.Path.DirectorySeparatorChar;
			if (target.Length < 2 || target[1] != ':')
				throw new Org.Nutbox.Exception("Unable to determine drive: " + setup.Target);
			target += "diskfill.dat";
			string drive = target.Substring(0, 1);

			// if diskfill.dat already exists, remove it prior to computing free space
			if (System.IO.File.Exists(target))
				System.IO.File.Delete(target);

			// create diskfill.dat and fill it with zeroes
			System.IO.FileStream file = System.IO.File.Create(
				target,					// path
				BLOCKSIZE,				// buffersize
				System.IO.FileOptions.WriteThrough |  //options
				System.IO.FileOptions.SequentialScan
			);
			byte[] bytes = new byte[BLOCKSIZE];
			for (;;)
			{
				// calculate size of block to write (step)
				// note: DriveInfo does not report the exact number of free
				// note: bytes,	which renders it almost useless for our purposes
				System.IO.DriveInfo info = new System.IO.DriveInfo(drive);
				long free = info.AvailableFreeSpace;

				// handle the -reserve option
				free = (free <= setup.Reserve ? 0 : free - setup.Reserve);

				// calculate the size of the block to write this time
				int step = (free < BLOCKSIZE) ? (int) free : BLOCKSIZE;

#if false
				// note: This line of code reveals a rather nasty bug in
				// note: .NET or Win32: System.IO.DriveInfo is unreliable as
				// note: it only updates the info.AvailableFreeSpace field
				// note: every four writes to the disk or so.
				System.Console.WriteLine("free = {0}, step = {1}", free, step);
#endif

				// write the actual data bytes
				file.Write(bytes, 0, step);

				// the last block is always less than the block size
				if (step < BLOCKSIZE)
					break;
			}
			file.Flush();
			file.Close();

			// delete diskfill.dat, if applicable
			if (setup.Delete)
				System.IO.File.Delete(target + "diskfill.dat");
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
