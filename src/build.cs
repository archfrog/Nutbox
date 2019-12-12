#region license
// Copyright (C) 2010-2020 Mikael Egevig (mikael@egevig.org).  All rights reserved.
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

/** \file
 *  Primitive build system that is portable to all platforms that support Microsoft .NET, Novell Mono, and perhaps GNU dotGNU.
 *
 *  The help screen is displayed if the option "-help" appears in the list of parameters.
 *
 *  Microsoft.NET:
 *
 *  	csc /checked+ build.cs
 *  	build -help | more
 *
 *  Novell Mono:
 *
 *  	mcs /checked+ build.cs
 *  	mono build.exe -help | more
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Org.Egevig.Nutbox.Build
{
	public class Environment
	{
		public static string GetVersion()
		{
			// try Mono first
			System.Type type = System.Type.GetType("Mono.Runtime");
			if (type != null)
			{
				MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
				if (displayName != null)
				{
					string version = (string) displayName.Invoke(null, null);
					int pos = version.IndexOf(' ');
					if (pos != -1)
						version = version.Substring(0, pos);
					return version;
				}
			}

			// then try .NET
			return RuntimeEnvironment.GetSystemVersion().Substring(1);	// discard leading 'v'
		}
	}


	/** Handy class that handles the intricate details of executing an external command in .NET. */
	public class Process
	{
		/** Creates a joined string of the specified command-line parameters.
		 *
		 *  \note The method currently only takes spaces, not reserved characters, into consideration.
		 */
		public static string JoinCommandLine(string[] parameters)
		{
			string result = "";

			foreach (string parameter in parameters)
			{
				// separate each parameter with a single space
				if (result.Length != 0)
					result += ' ';

				// handle the simple case first
				if (parameter.IndexOf(' ') == -1)
				{
					result += parameter;
					continue;
				}

				// now for the complex case (with embedded spaces)
				result += '\"';
				result += parameter;
				result += '\"';
			}

			return result;
		}

		/** The result of executing an external command. */
		public class Status
		{
			/** The external command's exit code. */
			private int _code;
			public int Code
			{
				get { return _code; }
				set { _code = value; }
			}

			/** The external command's screen output (stdout AND stderr!). */
			private string _text;
			public string Text
			{
				get { return _text; }
				set { _text = value; }
			}
		}

		/** An ad-hoc wrapper class that stores the state needed to execute an external command. */
		private class Invokator
		{
			/** The exit code of the external command. */
			private int _code;
			public int Code
			{
				get { return _code; }
			}

			/** Whether or not to echo (display) the command's output on the screen WHILE it runs. */
			private bool _echo;
			public bool Echo
			{
				get { return _echo; }
				set { _echo = value; }
			}

			/** The output of the external command. */
			private System.Text.StringBuilder _text;
			public string Text
			{
				get { return _text.ToString(); }
			}

			/** Handy wrapper around the house-keeping necessary to launch a process.
			 *
			 *  In Python, it would have been invoked using this format:
			 *
			 *  	( status, output ) = Execute("foo.exe", "arg1 arg2")
			 *
			 *  But, alas, C# is nowhere as elegant as Python and/or Boo.
			 *
			 *  This method handles all the dirty details of asynchronously reading the standard error and standard input streams.
			 */
			public void Execute(string command, string arguments)
			{
				// set up process parameters
				System.Diagnostics.Process process = new System.Diagnostics.Process();
				process.StartInfo.FileName = command;
				process.StartInfo.Arguments = arguments;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
				process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

				// initialize the instance (todo: clean up interface (echo arg))
				_code = 0;
				// _echo is set up by the caller (defaults to false)
				_text = new System.Text.StringBuilder("");

				// try starting the process
				try
				{
					process.Start();

					// begin asynchronously reading stdout and stderr
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();

					// wait for the process to exit, then query its exit code
					process.WaitForExit();
					_code = process.ExitCode;
				}
				finally
				{
					// clean up no matter what happens
					process.Close();
				}

				// the instance can now be queried to determine the result
			}

			/** Callback that is invoked whenever the external command outputs data to stdout and/or stderr. */
			private void OutputHandler(object sender, DataReceivedEventArgs args)
			{
				if (args.Data == null)
					return;

				if (_text.Length != 0)
					_text.Append(System.Environment.NewLine);
				_text.Append(args.Data);

				if (_echo)
					System.Console.WriteLine(args.Data);
			}

			/** Here used to be an Execute(string, List<string>) variant, but FxCop v1.36 whines and threatens to tell M$ that a
			 *  generic list is being used in a publicly visible interface.
			 */
		}

		/** Executes the specified command with the specified arguments, echoing if \c echo is true. */
		public static Status Execute(string command, string arguments, bool echo)
		{
			// create, initialize and use Invokator instance
			Invokator invokator = new Invokator();
			invokator.Echo = echo;
			invokator.Execute(command, arguments);

			// query the instance for the result
			Status result = new Status();
			result.Code = invokator.Code;
			result.Text = invokator.Text;

			return result;
		}

		/** Executes the specified command with the specified arguments, echoing if \c echo is true. */
		public static Status Execute(string command, string[] arguments, bool echo)
		{
			return Execute(command, JoinCommandLine(arguments), echo);
		}

		/** Executes the specified command, after splitting it into executable and arguments, echoing if \c echo is true. */
		public static Status Execute(string cmdline, bool echo)
		{
			int pos = cmdline.IndexOf(' ');
			string command;
			string arguments;
			if (pos == -1)
			{
				command   = cmdline;
				arguments = "";
			}
			else
			{
				command = cmdline.Substring(0, pos);
				arguments = cmdline.Substring(pos + 1);
			}
			return Execute(command, arguments, echo);
		}
	}

	/** An exception definition used throughout this simple tool. */
	public class BuildError: System.Exception
	{
		public BuildError(string message):
			base(message)
		{
		}
	}

	/** The configuration/setup information provided by the user on the command-line. */
	public class Setup
	{
		/** If true, the <tt>-checked+</tt> option will be passed to the C# compiler. */
		private bool _checked = true;
		public bool Checked
		{
			get { return _checked; }
		}

		/** If true, the tool performs an unconditional build without using Make semantics. */
		private bool _clean = false;
		public bool Clean
		{
			get { return _clean; }
		}

		/** The name of the compiler executable. */
		private string _compiler = (System.Type.GetType("Mono.Runtime") != null) ? "mcs" : "csc.exe";
		public string Compiler
		{
			get { return _compiler; }
		}

		/** If true, Doxygen will be invoked to generate elaborate HTML documentation for the project. */
		private bool _documentation = false;
		public bool Documentation
		{
			get { return _documentation; }
		}

		/** If true, help will be displayed and the program will exit. */
		private bool _help = false;
		public bool Help
		{
			get { return _help; }
		}

		/** If true, the program copyright banner will be displayed. */
		private bool _logo = false;
		public bool Logo
		{
			get { return _logo; }
		}

		/** The basename (folder name) of where to output the C# compiler output. */
		private string _output = null;
		public string Output
		{
			get
			{
				if (_output == null)
					_output = "../obj/" + _platform + "-" + _version + "-" + (_ship ? "ship" : "test");
				return _output;
			}
		}

		/** The target environment (.NET or Mono). */
		private string _platform = (System.Type.GetType("Mono.Runtime") != null) ? "Mono" : ".NET";
		public string Platform
		{
			get { return _platform; }
		}

		/* If true, optimizations will be enabled and debug info will be disabled. */
		private bool _ship = false;
		public bool Ship
		{
			get { return _ship; }
		}

		/** If true, verbose progress information will be displayed. */
		private int _verbose = 1;
		public int Verbose
		{
			get { return _verbose; }
		}

		/** The version of the .NET/Mono runtime. */
		private string _version = Environment.GetVersion();
		public string Version
		{
			get { return _version; }
		}

		/** Checks that the parsed setup/configuration information is valid and complete. */
		public void Check()
		{
		}

		/** Parse a single command-line parameter. */
		public void Parse(string arg)
		{
			// detect empty arguments as these are generally a sign of an error somewhere in the system
			if (arg.Length == 0)
				throw new BuildError("Empty parameter detected");

			// parse an option, if applicable
			if (arg[0] == '-')
			{
				// split the option into its name and data parts
				int pos = arg.IndexOf(':');
				string name;
				string data;
				if (pos == -1)
				{
					name = arg.Substring(1);
					data = null;
				}
				else
				{
					name = arg.Substring(1, pos - 1);
					data = arg.Substring(pos + 1);
				}

				// process the name and data parts according to the name part
				switch (name.ToLowerInvariant())
				{
					case "checked":
						if (data != null)
							throw new BuildError("Extranous parameter in option: " + arg);
						_checked = !_checked;
						break;

					case "clean":
						if (data != null)
							throw new BuildError("Extranous parameter in option: " + arg);
						_clean = !_clean;
						break;

					case "doc":
						if (data != null)
							throw new BuildError("Extranous parameter in option: " + arg);
						_documentation = true;
						break;

					case "nodoc":
						if (data != null)
							throw new BuildError("Extranous parameter in option: " + arg);
						_documentation = false;
						break;


					case "platform":
						if (data == null)
							throw new BuildError("Missing parameter in option: " + arg);
						switch (data.ToUpperInvariant())
						{
							case ".NET":
								_platform = ".NET";
								_compiler = "csc.exe"; 	/** \note Won't work on Linux... */
								break;

							case "MONO":
								_platform = "Mono";
								_compiler = "mcs";
								break;

							default:
								throw new BuildError("Unknown platform: " + data);
						}
						break;

					case "help":
					case "?":
					case "h":
					case "-help":
						if (data != null)
							throw new BuildError("Extranous parameter in option: " + arg);
						_help = true;
						break;

					case "logo":
						if (data != null)
							throw new BuildError("Extranous parameter in option: " + arg);
						_logo = !_logo;
						break;

					case "mode":
						if (data == null)
							throw new BuildError("Missing parameter in option: " + arg);
						switch (data.ToLowerInvariant())
						{
							case "ship": _ship = true; break;
							case "test": _ship = false; break;
							default    : throw new BuildError("Invalid value in option: " + arg);
						}
						break;

					case "output":
						if (data == null || data.Trim().Length == 0)
							throw new BuildError("Missing parameter in option: " + arg);
						_output = data;
						break;

					case "verbose":
						_verbose = (data == null) ? 2 : int.Parse(data);
						if (_verbose < 0 || _verbose > 2)
							throw new BuildError("Invalid value in option parameter: " + arg);
						break;

					default:
						throw new BuildError("Unknown option: " + arg);
				}
				return;
			}

			// a positional parameter - not currently supported
			throw new BuildError("Extranous positional parameter: " + arg);
		}

		/** Parse a list of arguments such as those passed into Main(). */
		public void Parse(string[] args)
		{
			foreach (string arg in args)
				Parse(arg);
		}

		/** Display the online help for this command. */
		public static void ShowHelp()
		{
			System.Console.WriteLine("Syntax: \"build\" *option");
			System.Console.WriteLine();
			System.Console.WriteLine("Options:");
			System.Console.WriteLine("    -checked        Flip 'checked' flag (default: on).");
			System.Console.WriteLine("    -clean          Build unconditionally without doing an implicit make.");
			System.Console.WriteLine("    -doc|-nodoc     Enable or disable generation of docs (default: off).");
			System.Console.WriteLine("    -logo           Flip 'logo' flag (default: off).");
			System.Console.WriteLine("    -mode:name      Select 'ship' or 'test' mode (default: test).");
			System.Console.WriteLine("    -output:dir     Specify output diretory (default: ../obj/...).");
			System.Console.WriteLine("    -platform:name  Specify name of compiler platform (.NET or Mono).");
			System.Console.WriteLine("    -verbose:n      Specify verbosity level (0 to 2, default: 1).");
		}
	}

	/** The application itself. */
	public class Program
	{
		/** Find the files matching the given pattern (incl. wildcards) in the given folder. */
		public static string[] FileFind(string origin, string pattern)
		{
			List<string> result = new List<string>();
			FileFind(origin, pattern, result);
			return result.ToArray();
		}

		/** Find the files matching the given pattern (incl. wildcards) in the given folder. */
		private static void FileFind(string origin, string pattern, List<string> result)
		{
			string[] files = System.IO.Directory.GetFiles(origin, pattern);
			string[] dirs  = System.IO.Directory.GetDirectories(origin, "*");

			foreach (string file in files)
				result.Add(file);

			foreach (string dir in dirs)
				FileFind(dir, pattern, result);
		}

		/** Return the file time of the given existing file. */
		public static long FileDate(string filename)
		{
			return System.IO.File.GetLastWriteTime(filename).Ticks;
		}

		public static string MixedCase(string value)
		{
			return char.ToUpper(value[0]) + value.Substring(1);
		}

		/** Compile the specified target (basename) from the source files in the home path.
		 *
		 *  The target is linked with the specified assemblies and the specified setup is used.
		 */
		public static void Compile(string basename, string homepath, string[] assemblies, Setup setup)
		{
			List<string> args = new List<string>();

			// compute the full output path of the target
			string target = setup.Output + '/' + basename;
			System.Console.WriteLine("Building: {0}", target);

			// determine if target is up-to-date and skip build if so
			string[] files = FileFind(homepath, "*.cs");
			if (!setup.Clean && System.IO.File.Exists(target))
			{
				long filedate = FileDate(target);
				bool uptodate = true;

				// check if any source file is newer than the target file
				foreach (string file in files)
				{
					if (FileDate(file) > filedate)
					{
						uptodate = false;
						break;
					}
				}

				// check if any needed assembly is newer than the target file
				foreach (string assembly in assemblies)
				{
					if (FileDate(setup.Output + '/' + assembly) > filedate)
					{
						uptodate = false;
						break;
					}
				}

				// if up-to-date, don't build target
				if (uptodate)
					return;
			}

			// handle the SHIP/TEST cases
			if (setup.Ship)
			{
				args.Add("-define:SHIP");
				args.Add("-optimize+");
			}
			else
			{
				args.Add("-define:TEST");
				args.Add("-debug+");
			}

			// enable C# overflow checking (by default!  I simply don't understand why it is disabled by default in the compiler.)
			if (setup.Checked)
				args.Add("-checked+");

			// sign the assembly so that we get rid of future warnings and errors
			args.Add("-keyfile:Nutbox.snk");

			// tell the compiler to stop littering the screen/log file with copyright messages, if applicable
			if (!setup.Logo)
				args.Add("-nologo");

			// tell the compiler the name of the output file
			args.Add("-out:" + target);

			// reference each of the specified assemblies
			foreach (string assembly in assemblies)
			{
				if (assembly.Substring(0, 7) == "System.")
					args.Add(string.Format("-reference:{0}", assembly));
				else
					args.Add(string.Format("-reference:{0}/{1}", setup.Output, assembly));
			}

			// ensure libraries (.dll files) get built correctly
			if (System.IO.Path.GetExtension(basename) == ".dll")
				args.Add("-target:library");

			// append the source file names
			foreach (string file in files)
				args.Add(file);

			// ensure the output directory exists
			if (!System.IO.Directory.Exists(setup.Output))
				System.IO.Directory.CreateDirectory(setup.Output);

			// convert list of arguments into a single string with embedded spaces in it
			string[] arglist = args.ToArray();
			string   arguments = Process.JoinCommandLine(arglist);

			// output command if verbosity is greater than or equal to two
			if (setup.Verbose >= 2)
				System.Console.WriteLine("{0} {1}", setup.Compiler, arguments);

			// invoke the specified compiler
			Process.Status status = Process.Execute(setup.Compiler, arguments, (setup.Verbose >= 1));
			if (status.Code != 0)
			{
				// remove the failed target so as to ensure the next invokation of 'build.cs' does not succeed.
				System.IO.File.Delete(target);
				throw new BuildError("Error invoking command '" + setup.Compiler + "'");
			}
		}

		/** The main program entry point. */
		public static int Main(string[] args)
		{
			int result = 0;

			try
			{
				// tell the world who's running the show
				System.Console.WriteLine("Nutbox.build v0.08 - https://github.com/archfrog/Nutbox");
				System.Console.WriteLine("Copyright (C) 2009-2020 Mikael Egevig.  Donated to the Public Domain.");
				System.Console.WriteLine();

				// parse and check command-line parameters
				Setup setup = new Setup();
				setup.Parse(args);
				setup.Check();

				// show help, if requested by the user
				if (setup.Help)
				{
					Setup.ShowHelp();
					return 1;
				}

#if false
				/* This is a very dangerous idea if the user tries to specify "-output:."... */
				// remove the output folder to ensure everything's neat and fine (it is recreated above)
				System.IO.Directory.Delete(setup.Output, true);
#endif

				// compile the core Nutbox assembly
				Compile("Nutbox.dll", "Nutbox", new string[0], setup);

				// compile each of the Nutbox tools
				string[] programs = System.IO.Directory.GetDirectories(".", "*");
				foreach (string program1 in programs)
				{
					string program = System.IO.Path.GetFileName(program1);

					// ignore the assembly we've already built
					if (program == "Nutbox")
						continue;

					// create or update the .hlp.cs file if, applicable
					string helppath = "Org.Egevig.Nutbox." + MixedCase(program) + ".Help.Text";
					string source = program + '/' + program + ".hlp";
					string target = program + '/' + program + ".hlp.cs";
					if (setup.Clean || !System.IO.File.Exists(target) || FileDate(source) > FileDate(target))
					{
						string[] arguments = new string[]
						{
							"--format:string",
							helppath,
							source,
							target
						};

						Process.Status status = Process.Execute("../bld/txt2cs.exe", arguments, (setup.Verbose >= 1));
						if (status.Code != 0)
						{
							// remove the failed target so as to ensure the next invokation of 'build.cs' does not succeed.
							System.IO.File.Delete(target);
							throw new BuildError("Error invoking command '" + "../bld/txt2cs.exe" + "'");
						}
					}

					// some assemblies need the System.Windows.Forms.dll assembly
					string[] assembly = { "Nutbox.dll" };
					if (program == "ask" || program == "message")
						assembly = new string[]{ "Nutbox.dll", "System.Windows.Forms.dll" };

					// finally, invoke the compiler
					Compile(program + ".exe", program, assembly, setup);
				}

				// build documentation in ../obj/doc/html.
				if (setup.Documentation)
				{
					string helpdir = "../obj/docs";
					System.Console.WriteLine("Building: {0}", helpdir);
					if (System.IO.Directory.Exists(helpdir))
						System.IO.Directory.Delete(helpdir, true);
					System.IO.Directory.CreateDirectory(helpdir);
					Process.Execute("doxygen", false);
				}
			}
			catch (BuildError exception)
			{
				// report a neat error to STDOUT (I loathe tools that report errors to STDERR!)
				System.Console.WriteLine("Error: {0}", exception.Message);
				result = 1;
			}

			return result;
		}
	}
}

