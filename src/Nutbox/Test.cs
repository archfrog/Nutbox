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

namespace Org.Nutbox
{
	public sealed class Test
	{
		// pleasing FxCop v1.36
		private Test()
		{
		}

		public static Nutbox.Platform.Process.Status Execute(int code, string cmdline)
		{
			Nutbox.Platform.Process.Status status;
			status = Nutbox.Platform.Process.Execute(cmdline, false);
			if (status.Code != code)
				System.Console.WriteLine(
					"FAILURE: " +
					"Test failed with exit code '" +
					status.Code.ToString() + "' instead of '" + code.ToString() + "'"
				);

			return status;
		}

		/// <summary>
		/// Performs a somewhat loose string compare of two strings.  The method
		/// takes into consideration that the 'first' string is stored in the
		/// standard C format (with one newline character indicates line change)
		/// and the 'other' string is stored in the external, platform-dependent
		/// format.
		/// </summary>
		/// <param name="first">The first string to compare.</param>
		/// <param name="other">The second string to compare.</param>
		/// <returns>True if they are "equal", otherwise false.</returns>
		public static bool LooseStringCompare(string first, string other)
		{
			first = first.Trim();
			other = other.Trim();

			string[] newlines = new string[] { "\r\n", "\n", "\r" };
			string[] first_split = first.Split('\n');
			string[] other_split = other.Split(newlines, System.StringSplitOptions.None);
			if (first_split.Length != other_split.Length)
				return false;

			for (int i = 0; i < first_split.Length; i += 1)
			{
				if (first_split[i] != other_split[i])
					return false;
			}

			return true;
		}
	}
}
