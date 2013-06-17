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

namespace Org.Nutbox.Platform
{
	public sealed class Time
	{
		// Time:
		// Satisfy FxCop v1.36 demands...
		private Time()
		{
		}

		// Standard:
		// Returns the current time in the standard format.
		public static string Standard()
		{
			return Standardize(System.DateTime.Now);
		}

		// Standardize:
		// Converts a given time to standard format (YYYY.MM.DD.HH.MM.SS)
		public static string Standardize(System.DateTime value)
		{
			// note: the format is standard across all platforms and locales
			// note: The CultureInfo parameter is probably redundant, but
			// note: FxCop v1.36 demands it.
			string result = value.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
			result = result.Replace("-", ".");
			result = result.Replace("T", ".");
			result = result.Replace(":", ".");
			result = result.Substring(0, 19);
			return result;
		}

		/// <summary>
		/// Parses a "standard" time string (YYYY.MM.DD.HH.II.SS.LLLLLLLL) and
		/// returns true if successful.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryParse(string value, out System.DateTime result)
		{
			string time = value;
			if (time.Length >= 5)
				time = time.Substring(0, 4) + '-' + time.Substring(4 + 1);
			if (time.Length >= 8)
				time = time.Substring(0, 7) + '-' + time.Substring(7 + 1);
			if (time.Length >= 11)
				time = time.Substring(0, 10) + 'T' + time.Substring(10 + 1);
			if (time.Length >= 14)
				time = time.Substring(0, 13) + ':' + time.Substring(13 + 1);
			if (time.Length >= 17)
				time = time.Substring(0, 16) + ':' + time.Substring(16 + 1);

			return System.DateTime.TryParse(time, out result);
		}

	}
}
