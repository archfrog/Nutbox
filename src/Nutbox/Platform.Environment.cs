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

namespace Org.Egevig.Nutbox.Platform
{
	using System.Collections.Generic;	// Dictionary<T1, T2>

	public sealed class Environment
	{
		// Environment:
		// Satisfy FxCop v1.36 demands...
		private Environment()
		{
		}

		// return a table of known newline character sequences
		public static string[] NewLines()
		{
			return new string[3] {
				"\r\n",						// Windows
				"\n",						// Unix
				"\r"						// Macintosh
			};
		}

		/// <summary>
		/// Same as System.Environment.GetEnvironmentVariables(), except it
		/// returns a Dictionary<string, string> rather than an IDictionary.
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, string> GetEnvironmentVariables()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			System.Collections.IDictionary env = System.Environment.GetEnvironmentVariables();

			foreach (string key in env.Keys)
			{
				// simplify client code by making all names uppercase...
				string name = key.ToUpperInvariant();
				result[name] = (string) env[key];
			}

			return result;
		}
	}
}
