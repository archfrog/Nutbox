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
	using System.Collections.Generic;	// Dictionary<T1, T2>

	public sealed class Shell
	{
		// Shell:
		// Satisfy FxCop v1.36 demands...
		private Shell()
		{
		}

		// ExpandMacros:
		// Expands the embedded macros of the form $(NAME) by looking up the
		// name in the dictionary 'symbols'.  Suitable errors, albeit very
		// primitive (no filename or line number is shown), are reported if
		// a malformed or invalid macro is detected.
		public static string ExpandMacros(string line, Dictionary<string, string> symbols)
		{
			string result = "";

			int state = 0;
			string name = "";
			foreach (char ch in line)
			{
				switch (state)
				{
					case 0:
						if (ch == '$')
						{
							state = 1;
							break;
						}
						result += ch;
						break;

					case 1:
						if (ch != '(')
							throw new Nutbox.Exception("Expected '('");
						state = 2;
						break;

					case 2:
						if (!System.Char.IsLetter(ch))
							throw new Nutbox.Exception("Invalid macro name");
						name += ch;
						state = 3;
						break;

					case 3:
						if (System.Char.IsLetter(ch) || ch == '_')
							name += ch;
						else if (ch == ')')
						{
							string upper = name.ToUpperInvariant();

							// expand the macro
							if (!symbols.ContainsKey(upper))
								throw new Nutbox.Exception("Undefined symbol: " + name);
							result += symbols[upper];

							state = 0;
							name = "";
						}
						else
							throw new Nutbox.Exception("Expected ')'");
						break;

					default:
						throw new Nutbox.Exception("Internal error - invalid state");
				}
			}

			if (state != 0)
				throw new Nutbox.Exception("End of line in symbol reference: " + line.Trim());

			return result;
		}

		// Quote:
		// Quotes the specified command, if necessary, so it can be executed
		// by the shell.  The current implementation is very simple but works
		// in most cases.
		public static string Quote(string value)
		{
			if (value.IndexOf(' ') == -1)
				return value;

			return '\"' + value + '\"';
		}
	}
}
