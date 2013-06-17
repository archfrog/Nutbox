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

namespace Org.Nutbox
{
	public abstract class Program
	{
		private Information _info;
		public Information Info
		{
			get { return _info; }
			set { _info = value; }
		}

		protected Program(Information info)
		{
			Info = info;
		}

		bool logoShown = false;

		public void ShowHelp(System.IO.TextWriter writer)
		{
#if TEST
			writer.WriteLine("VERSION");
			writer.WriteLine("    This is a TEST version, which may include internal integrity");
			writer.WriteLine("    checks, which MAY slow down the program significantly and/or");
			writer.WriteLine("    make the program report internal errors.");
			writer.WriteLine();
#endif
			// trim just in case some extranous spaces sneaked in somewhere
			writer.WriteLine(Info.Help.Trim());
		}

		public void ShowLogo(System.IO.TextWriter writer)
		{
			if (logoShown)
				return;
			logoShown = true;

			writer.Write(
				"Nutbox.{0} {1}/.NET-{2}.{3}",
				Info.Program,
				Info.Version,
				System.Environment.Version.Major,
				System.Environment.Version.Minor
			);
#if TEST
			writer.Write(" (TEST)");
#endif
			writer.WriteLine(" - http://www.nutbox.org");
			if (Info.Lower == Info.Upper)
			{
				writer.WriteLine(
					"Copyleft (-) {0} {1}.  {2}.",
					Info.Lower,
					Info.Company,
					Info.Rights
				);
			}
			else
			{
				writer.WriteLine(
					"Copyleft (-) {0}-{1} {2}.  {3}.",
					Info.Lower,
					Info.Upper,
					Info.Company,
					Info.Rights
				);
			}
			writer.WriteLine();
		}

		public abstract void Main(Setup setup);

		public int Main(Setup setup, string[] args)
		{
			int result = 0;

			try
			{
				setup.Parse(args);
				setup.Check();

				if (setup.Logo)
					ShowLogo(System.Console.Out);

				Main(setup);
			}

			catch (Nutbox.ShowHelpError)
			{
				// we dump the help text to stdout so as to be nice to newbies
				ShowLogo(System.Console.Out);
				ShowHelp(System.Console.Out);
				result = 2;
			}
			catch (Nutbox.ExitWithExitCode that)
			{
				// this exception is NOT an error; it simply yields a user-
				// defined exit code after having cleaned up everything.
				result = that.Code;
			}
#if SHIP
// In TEST mode we want to know everything about all exceptions.
// In SHIP mode we want to provide a reliable and robust interface to the user.
			catch (Nutbox.InternalError that)
			{
				ShowLogo(System.Console.Error);
				System.Console.Error.WriteLine("Internal error: {0}", that.Message);
				result = 1;
			}
			catch (Nutbox.TextFileException that)
			{
				ShowLogo(System.Console.Error);
				string text = "(" + that.Name;
				if (that.Line != 0 && that.Char != 0)
					text += ":" + that.Line.ToString() + "," + that.Char.ToString();
				else if (that.Line != 0)
					text += ":" + that.Line.ToString();
				text += ")";
				text += " Error: ";
				text += that.Message;
				System.Console.Error.WriteLine(text);
				result = 1;
			}
			catch (System.Exception that)
			{
				// note: also catches our own Nutbox.Exception
				ShowLogo(System.Console.Error);
				System.Console.Error.WriteLine("Error: " + that.Message);
				result = 1;
			}
#else
			finally
			{
			}
#endif

			return result;
		}
	}
}
