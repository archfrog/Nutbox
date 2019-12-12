#region license
// Copyleft (-) 2009-2020 Mikael Egevig (mikael@egevig.org).  Donated to the Public Domain.
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
	public sealed class String
	{
		private String()
		{
		}

		/// <summary>
		/// Compares two strings the old-fashioned way so as to get a proper
		/// effect of doing a case-sensitive compare (uppercase first, then
		/// lowercase).  This may be part of .NET v2.0, but I haven't figured
		/// out how to do it.  System.Collections.Compare.DefaultInvariant
		/// produces unexpected output when used - because it insists on doing
		/// a case-INSENSITIVE compare of strings, which is contrary to what it
		/// is supposed to do, if you ask me.
		/// </summary>
		/// <param name="first">The left string to compare.</param>
		/// <param name="other">The right string to compare.</param>
		/// <returns></returns>
		public static int strcmp(string first, string other)
		{
			for (int i = 0; i < first.Length; i++)
			{
				// if other is shorter than first, but otherwise equal to first
				if (i == other.Length)
					return 1;

				if (first[i] < other[i])
					return -1;
				else if (first[i] > other[i])
					return 1;
			}

			// if other is longer than first, but otherwise equal to first
			if (other.Length > first.Length)
				return -1;

			return 0;
		}

		public static int stricmp(string first, string other)
		{
			for (int i = 0; i < first.Length; i++)
			{
				// if other is shorter than first, but otherwise equal to first
				if (i == other.Length)
					return 1;

				char firstupper = System.Char.ToUpperInvariant(first[i]);
				char otherupper = System.Char.ToUpperInvariant(other[i]);
				if (firstupper < otherupper)
					return -1;
				else if (firstupper > otherupper)
					return 1;
			}

			// if other is longer than first, but otherwise equal to first
			if (other.Length > first.Length)
				return -1;

			return 0;
		}
	}
}
