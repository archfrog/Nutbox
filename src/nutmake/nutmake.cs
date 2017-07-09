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

using System.Collections.Generic;	// Dictionary<T1, T2>
using Org.Egevig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.nutmake")]
[assembly: AssemblyDescription("Simple, portable make tool")]
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

namespace Org.Egevig.Nutbox.Nutmake
{
	class Setup: Org.Egevig.Nutbox.Setup
	{
		protected ListValue _arguments = new ListValue();
		public List<string> Arguments
		{
			get { return _arguments.Value; }
		}

		protected BooleanValue _all = new BooleanValue(false);
		public bool All
		{
			get { return _all.Value; }
		}

		protected BooleanValue _continue = new BooleanValue(false);
		public bool Continue
		{
			get { return _continue.Value; }
		}

		protected StringValue _directory = new StringValue(null);
		public string Directory
		{
			get { return _directory.Value; }
		}

		protected List<string> _goals = new List<string>();
		public List<string> Goals
		{
			get { return _goals; }
		}

		protected BooleanValue _ignore = new BooleanValue(false);
		public bool Ignore
		{
			get { return _ignore.Value; }
		}

		protected BooleanValue _import = new BooleanValue(false);
		public bool Import
		{
			get { return _import.Value; }
		}

		protected StringValue _makefile = new StringValue("Nutmakefile");
		public string Makefile
		{
			get { return _makefile.Value; }
		}

		protected Dictionary<string, string> _symbols = new Dictionary<string, string>();
		public Dictionary<string, string> Symbols
		{
			get { return _symbols; }
		}

		protected BooleanValue _test = new BooleanValue(false);
		public bool TestOpt
		{
			get { return _test.Value; }
		}

		protected BooleanValue _verbose = new BooleanValue(false);
		public bool Verbose
		{
			get { return _verbose.Value; }
		}

		int ImportSymbol(string arg, int index)
		{
			// import the specified symbol
			string name = arg.Substring(index);
			string value = System.Environment.GetEnvironmentVariable(name);
			if (value == null)
				throw new Org.Egevig.Nutbox.Exception("Environment variable does not exist: " + name);
			_symbols[name] = value;
			return arg.Length;
		}

		int ClearSymbols(string arg, int index)
		{
			_symbols.Clear();
			return index;
		}

		public Setup()
		{
			Option[] options =
			{
				new StringOption("C", _directory),
				new StringOption("F", _makefile),
				new TrueOption("K", _continue),
				new TrueOption("N", _test),
				new TrueOption("all", _all),
				new FalseOption("noall", _all),
				new TrueOption("continue", _continue),
				new FalseOption("nocontinue", _continue),
				new StringOption("directory", _directory),
				new StringConstantOption("nodirectory", _directory, null),
				new StringOption("file", _makefile),
				new StringConstantOption("nofile", _makefile, "Nutmakefile"),
				new TrueOption("ignore", _ignore),
				new FalseOption("noignore", _ignore),
				new DelegateOption("import", new DelegateOption.Handler(ImportSymbol), true),
				new DelegateOption("noimport", new DelegateOption.Handler(ClearSymbols), false),
				new TrueOption("test", _test),
				new FalseOption("notest", _test),
				new TrueOption("verbose", _verbose),
				new FalseOption("noverbose", _verbose),
				new ListParameter(1, "definition or goal", _arguments, Option.eMode.Optional)
			};
			base.Add(options);
		}
	}

	class Program: Org.Egevig.Nutbox.Program
	{
		static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
			"nutmake",						// Program
			"v0.03",						// Version
			Org.Egevig.Nutbox.Copyright.Company,	// Company
			Org.Egevig.Nutbox.Copyright.Rights,	// Rights
			Org.Egevig.Nutbox.Copyright.Support,	// Support
            Org.Egevig.Nutbox.Copyright.Website,   // Website
			Org.Egevig.Nutbox.Nutmake.Help.Text,	// Help
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

			// split the arguments into definitions and goals
			foreach (string arg in setup.Arguments)
			{
				if (arg.IndexOf('=') != -1)
				{
					// check that the 'name' part of the pattern isn't the empty string
					int    pos  = arg.IndexOf('=');
					string name = arg.Substring(0, pos);
					string data = arg.Substring(pos + 1);

					if (name.Length == 0)
						throw new Org.Egevig.Nutbox.Exception("Malformed symbol definition: " + arg);

					setup.Symbols[name] = data.Trim();
				}
				else
					setup.Goals.Add(arg);
			}

			// change the current directory, if applicable
			if (setup.Directory != null)
				System.IO.Directory.SetCurrentDirectory(setup.Directory);

			// load the makefile
			Makefile makefile = new Makefile();
			makefile.All      = setup.All;
			makefile.Continue = setup.Continue;
			makefile.Ignore   = setup.Ignore;
			makefile.Test     = setup.TestOpt;
			makefile.Verbose  = setup.Verbose;
			makefile.Load(setup.Makefile, setup.Symbols);

			// check that the loader found at least one goal
			if (makefile.Goal == null)
				throw new Org.Egevig.Nutbox.Exception("Empty makefile");

			// if no explicit goal is given, use the 'first' goal
			if (setup.Goals.Count == 0)
				setup.Goals.Add(makefile.Goal.FileName);

			// Make each target
			foreach (string goal in setup.Goals)
				makefile.Make(goal);
		}

		public static int Main(string[] args)
		{
			Setup setup     = new Setup();
			Program program = new Program();

			// let Org.Egevig.Nutbox.Program.Main() handle exceptions, etc.
			return program.Main(setup, args);
		}
	}

	class Makefile
	{
		public class Target
		{
			private string _commands;		// newline separated list of commands
			public string Commands
			{
				get { return _commands; }
				set { _commands = value; }
			}

			private string[] _dependencies;
			public string[]	Dependencies
			{
				get { return _dependencies; }
			}

			private string  _filename;
			public string   FileName
			{
				get { return _filename; }
			}

			private bool _updated;			// true => has been built once
			public bool Updated
			{
				get { return _updated; }
				set { _updated = value; }
			}

			public Target(string filename, string[] deps)
			{
				_commands     = "";
				_dependencies = deps;
				_filename     = filename;
				_updated      = false;
			}
		}

		protected bool _all = false;
		public bool All
		{
			get { return _all; }
			set { _all = value; }
		}

		protected bool _continue = false;	// continue even if targets can't be made
		public bool Continue
		{
			get { return _continue; }
			set { _continue = value; }
		}

		protected bool _ignore = false;
		public bool Ignore
		{
			get { return _ignore; }
			set { _ignore = value; }
		}

		protected bool _test = false;
		public bool Test
		{
			get { return _test; }
			set { _test = value; }
		}

		protected bool _verbose = false;
		public bool Verbose
		{
			get { return _verbose; }
			set { _verbose = value; }
		}

		protected Target _goal = null;
		public Target Goal
		{
			get { return _goal; }
		}

		// targets:
		// The global 'database' of targets.  A simple dictionary, but that is
		// all that is needed to keep track of the targets.  I guess one could
		// argue that it would be nice if the targets were built in order of
		// appearance, but stuff like that does not matter much to me: As long
		// as all targets are built successfully, I don't care about the order.
		Dictionary<string, Target> targets = new Dictionary<string, Target>();

		// IsSymbolChar:
		// Returns true if the value is a valid target name character.
		public static bool IsSymbolChar(char value)
		{
			if (System.Char.IsLetter(value))
				return true;
			if (System.Char.IsDigit(value))
				return true;
			if (value == '_' || value == '/' || value == '.' || value == '\\' || value == '-')
				return true;
			return false;
		}

		// ParseName:
		// Parses the specified line starting at the specified index, assigns
		// the parse result to the out parameter, and returns the index AFTER
		// the last parsed character.
		public static int ParseName(string line, int index, out string value)
		{
			if (index == line.Length)
				throw new Org.Egevig.Nutbox.Exception("Unexpected end of line");

			string name = "";
			do
			{
				if (!IsSymbolChar(line[index]))
					throw new Org.Egevig.Nutbox.Exception("Name expected");
				name  += line[index];
				index += 1;
			} while (index < line.Length && IsSymbolChar(line[index]));
			value = name;

			return index;
		}

		// ParseCommand:
		// Parses the specified line starting at the specified index, updates
		// the out parameter, and returns the index AFTER the last parsed char.
		public static int ParseCommand(string line, int index, out string value)
		{
			if (index == line.Length)
				throw new Org.Egevig.Nutbox.Exception("Unexpected end of line");

			switch (line[index])
			{
				case ':' :
					value = ":";
					index += 1;
					break;

				case '?':
					index += 1;
					if (index == line.Length || line[index] != '=')
						throw new Org.Egevig.Nutbox.Exception("Unexpected end of command");
					index += 1;
					value = "?=";
					break;

				case '+':
					index += 1;
					if (index == line.Length || line[index] != '=')
						throw new Org.Egevig.Nutbox.Exception("Unexpected end of command");
					index += 1;
					value = "+=";
					break;

				case '=':
					index += 1;
					value = "=";
					break;

				default :
					throw new Org.Egevig.Nutbox.Exception("Unknown or missing command: " + line.Substring(index));
			}

			return index;
		}

		// SkipSpaceOpt:
		// Skips optional white-space and returns the index of the character
		// AFTER the skipped white-space, if any.
		// note: Since we only allow tabs in the beginning of the line, there's
		// note: no neeed to (or any sense of) handling tabs here.
		public static int SkipSpaceOpt(string line, int index)
		{
			while (index < line.Length && (line[index] == ' ' || line[index] == '\t'))
				index += 1;
			return index;
		}

		// Load:
		// Load and parse a Makefile.
		// Once the Makefile has been loaded, it can be made using th Make()
		// method.
		public void Load(string name, Dictionary<string, string> symbols)
		{
			System.IO.TextReader source = new System.IO.StreamReader(name, true);

			bool done = false;
			int number = 0;				// line number in file named 'name'
			int index = 0;				// character index in line number 'number'
			Target target = null;		// != null => inside rule
			do
			{
				number += 1;			// maintain line number

				try
				{
					index = 0;
					string line = source.ReadLine();
					if (line == null)
					{
						// create a dummy empty line to close open rules, etc.
						done = true;
						line = "";
					}

					// todo: expand tabs so as to make 'index' valid for error
					// todo: reporting even when the tabs have been processed.

					// calculate and remove indent
					int indent = 0;
					while (line.StartsWith("\t", System.StringComparison.Ordinal))
					{
						line    = line.Substring(1);
						indent += 1;
					}
					if (indent > 1)
						throw new Org.Egevig.Nutbox.Exception("Too much indentation detected");

					// barf on leading spaces
					if (line.Length > 0 && line[0] == ' ')
						throw new Org.Egevig.Nutbox.Exception(name + "(" + number.ToString() + ") Error: Leading space detected");

					// truncate comment lines
					if (line.IndexOf('#') != -1)
						line = line.Substring(0, line.IndexOf('#'));

					// discard trailing white-space: we don't want it for anything
					line = line.TrimEnd();

					// expand macros
					line = Org.Egevig.Nutbox.Platform.Shell.ExpandMacros(line, symbols);

					// delete extranous space on this construction:
					// $(EMPTY) foo bar
					// here $(EMPTY) is replaced by the empty string, making an
					// extra space appear before foo, which again breaks the make
					line = line.TrimStart();

					// parse inner or outer declaration (inner = rule)
					switch (indent)
					{
						// looking for comments, assignments, and rules
						case 0:
						{
							target = null;

							if (line.Length == 0)
								continue;

							string symbol;
							index = ParseName(line, index, out symbol);
							index = SkipSpaceOpt(line, index);
							string command;
							index = ParseCommand(line, index, out command);
							index = SkipSpaceOpt(line, index);
							string argument	= line.Substring(index);

							// main command dispatcher
							switch (command)
							{
								// basic assignment, simply update the symbol
								case "=":
									symbols[symbol] = argument;
									break;

								// addition, require the symbol to exist already
								case "+=":
									if (!symbols.ContainsKey(symbol))
										throw new Org.Egevig.Nutbox.Exception("Unknown macro: " + symbol);
									if (symbols[symbol].Length > 0)
										symbols[symbol] += ' ';
									symbols[symbol] += argument;
									break;

								// optional assignment, only assign if undefined
								case "?=":
									if (symbols.ContainsKey(symbol))
										break;
									goto case "=";

								// a target, add it to the dictionary of targets
								case ":":	// a target
									if (targets.ContainsKey(symbol))
										throw new Org.Egevig.Nutbox.Exception("Target already defined: " + symbol);
									string[] deps;
									if (argument.Length == 0)
										deps = new string[0];
									else
										deps = argument.Trim().Split(' ');
									target = new Target(symbol, deps);
									targets[symbol] = target;

									// record the default goal so the client can query it
									if (_goal == null)
										_goal = target;
									break;

								default:
									throw new Org.Egevig.Nutbox.Exception("Internal error - unknown command");
							}
							break;
						}

						// first line of rule
						case 1:
						{
							if (target == null)
								throw new Org.Egevig.Nutbox.Exception("Command outside of rule");
							if (line.Length == 0)
								throw new Org.Egevig.Nutbox.Exception("Empty command detected");
							if (indent != 1)
								throw new Org.Egevig.Nutbox.Exception("Incorrect indentation (one tab stop expected)");
							if (target.Commands.Length > 0)
								target.Commands += '\n';
							target.Commands += line;
							break;
						}

						default:
							throw new Org.Egevig.Nutbox.Exception("Invalid parser state");
					}
				}
				catch (Org.Egevig.Nutbox.Exception that)
				{
					// repackage the exception with file and line number info
					// added so that Org.Egevig.Nutbox.Program can report a proper error
					throw new Org.Egevig.Nutbox.TextFileException(name, number, index, that.Message);
				}
			} while (!done);
		}

		// Make:
		// Attempts to make the specified goal by making all dependencies
		// (recursively) and then making the specified goal.
		public void Make(string goal)
		{
			// check that the goal is defined in the Makefile
			if (!targets.ContainsKey(goal))
				throw new Org.Egevig.Nutbox.Exception("Unknown target: " + goal);
			Target target = targets[goal];

			// make each dependency prior to checking if the target is up-to-date
			foreach (string dep in target.Dependencies)
			{
				// invoke ourselves recursively to make non-existent deps
				if (targets.ContainsKey(dep))
					Make(dep);
				else if (!Org.Egevig.Nutbox.Platform.Disk.Exists(dep))
					throw new Org.Egevig.Nutbox.Exception("Unknown target: " + dep);
			}

			// check if the target is up-to-date or outdated
			bool outdated = false;

			// handle the -all option
			outdated |= All;

			if (!outdated && !Org.Egevig.Nutbox.Platform.Disk.Exists(goal))
				outdated = true;

			// check if one or more dependencies are newer
			if (!outdated)
			{
				// get the modified time of the goal object
				System.DateTime goaltime = Org.Egevig.Nutbox.Platform.Disk.GetLastWriteTimeUtc(goal);

				// iterate through each dependency and see if it is newer than
				// the goal.
				foreach (string dep in target.Dependencies)
				{
					System.DateTime deptime;

					// sanity check: was the target really created?
					if (!_continue && !Org.Egevig.Nutbox.Platform.Disk.Exists(dep))
						throw new Org.Egevig.Nutbox.Exception("Target updated but non-existent?!");
					deptime = Org.Egevig.Nutbox.Platform.Disk.GetLastWriteTimeUtc(dep);

					// the core of any make tool: the two magical lines...
					if (deptime > goaltime)
						outdated = true;
				}
			}

			// the target was up-to-date, return silently
			if (!outdated || target.Updated)
				return;
			target.Updated = true;

			Build(goal);
		}

		// Build:
		// Builds the target in question (invokes the translator).
		// note: Build does not recursively build anything; it only builds the
		// note: exact that that was specified as 'goal'.
		public void Build(string goal)
		{
			// in case this public method is invoked by a client
			if (!targets.ContainsKey(goal))
				throw new Org.Egevig.Nutbox.Exception("Unknown target: " + goal);
			Target target = targets[goal];

			// let the user know what we're about to do
			if (_verbose)
				System.Console.WriteLine("Building: {0}", goal);

			// stupid Split() returns a list with one item even on an empty string
			if (target.Commands.Length == 0)
				return;
			string[] commands = target.Commands.Split('\n');

			// execute each command in turn, aborting if a non-zero status code
			// is detected.
			foreach (string command in commands)
			{
				if (_verbose)
					System.Console.WriteLine(command);

				// if we're testing, simply display the commands to invoke
				if (_test)
					continue;

				Org.Egevig.Nutbox.Platform.Process.Status status;
				status = Org.Egevig.Nutbox.Platform.Process.Execute(command, _verbose);
				if (status.Code != 0)
				{
					if (!_ignore)
						throw new Org.Egevig.Nutbox.Exception("Error invoking command: " + command);
					if (_verbose)
						System.Console.WriteLine("Warning: Error '" + status.Code + "' during execution of last command");
				}
			}
		}
	}
}
