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

namespace Org.Nutbox.Platform
{
	using System.Collections.Generic;	// List<string>
	using System.Diagnostics;			// DataReceivedEventArgs

	public sealed class Process
	{
		// Process:
		// Satisfy FxCop v1.36 demands...
		private Process()
		{
		}

		// JoinCommandLine:
		// Creates a joined string of command-line parameters, taking into
		// consideration issues such as embedded spaces and reserved characters.
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

		public class Status
		{
			private int code;
			public int Code
			{
				get { return code; }
				set { code = value; }
			}

			private string text;
			public string Text
			{
				get { return text; }
				set { text = value; }
			}
		}

		// Invokator:
		// This class only exists because we need a few fields to handle the
		// asynchronous events that we get while executing the command.
		private class Invokator
		{
			private int _code;
			public int Code
			{
				get { return _code; }
			}

			private bool _echo;
			public bool Echo
			{
				get { return _echo; }
				set { _echo = value; }
			}

			private System.Text.StringBuilder _text;
			public string Text
			{
				get { return _text.ToString(); }
			}

			// Handy wrapper around the house-keeping necessary to launch a process
			// In Python, it would have been invoked using this format:
			//
			//   ( status, output ) = Execute("foo.exe", "arg1 arg2")
			//
			// But, alas, C# is nowhere as elegant as Python and/or Boo.
			//
			// This method handles all the dirty details of asynchronously reading
			// the standard error and standard input streams.
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

			// note: Here used to be an Execute(string, List<string>) variant, but
			// note: FxCop v1.36 whines and threatens to tell M$ that a generic
			// note: list is being used in a publicly visible interface.
		}

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

		public static Status Execute(string command, string[] arguments, bool echo)
		{
			return Execute(command, JoinCommandLine(arguments), echo);
		}

		public static Status Execute(string cmdline, bool echo)
		{
			// todo: enhance Nutbox.Platform.Process.Execute(string, bool)
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
}
