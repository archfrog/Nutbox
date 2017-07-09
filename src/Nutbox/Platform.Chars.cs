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

// To put it in the words of a former supervisor:
// This module defeats the purpose of .NET v2.0's Unicode support.
//
// The module probably seems nuts, but I like my POSIX-style tools to display
// POSIX-style behavior.  And I haven't figured out how to do it with .NET.
// Most likely, this is my fault and I am to blame (naughty boy!).

namespace Org.Egevig.Nutbox.Platform
{
	public sealed class Chars
	{
		private Chars()
		{
		}

		/// <summary>
		/// Returns true if the specified character is a Western letter.
		/// Note: It is intentional that we do not currently support other
		/// Note: scripts than the Western script.  Using .NET 2.0's Unicode
		/// Note: aware System.Char.IsLetter() method tends to produce lots of
		/// Note: garbage for Western readers.
		/// </summary>
		/// <param name="ch">The character to test.</param>
		/// <returns></returns>
		public static bool IsLetter(char ch)
		{
#if true
			if (ch >= 'a' && ch <= 'z')
				return true;
			if (ch >= 'A' && ch <= 'Z')
				return true;
#else
			// this works pretty badly; it ought to work well, but it does not
			if (System.Char.IsLetter(ch))
				return true;
#endif
			return false;
		}

		public static bool IsDigit(char ch)
		{
			// this works fine, though
			return System.Char.IsDigit(ch);
		}

		/// <summary>
		/// Returns true if the specified character is a Western punctuation
		/// character.  See IsLetter() for important info on why this oddity.
		/// </summary>
		/// <param name="ch">The character to test.</param>
		/// <returns>Returns true if the character is a Western punctuation
		/// character.</returns>
		public static bool IsPunct(char ch)
		{
			return "!\"#$%^&*()_-+=?/<>,.{}[]:;'^|".IndexOf(ch) != -1;
		}
	}
}
