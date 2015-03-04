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

namespace Org.Lyngvig.Nutbox
{
	public sealed class Types
	{
		// Types:
		// Satisfy FxCop v1.36 demands...
		private Types()
		{
		}

		public sealed class Long
		{
			// Long:
			// Satisfy FxCop v1.36 demands...
			private Long()
			{
			}

			// Parse:
			// Parses a long with an optional unit of measurement suffix.
			// k[b] = kilobytes, m[b] = megabytes, g[b] = gigabytes, and
			// t[b] = terabytes.  The 'b' is optional so as to satisfy those
			// very used to POSIX tools (which do not allow the trailing b).
			public static long Parse(string value)
			{
				long result = 0;
				long sign   = 1;
				int index   = 0;

				if (value.Length == 0)
					throw new Nutbox.Exception("Invalid long literal: " + value);

				// parse sign, if any
				if (value[index] == '-')
				{
					sign   = -1;
					index += 1;
				}

				// check that there's any digits
				if (index >= value.Length)
					throw new Nutbox.Exception("Invalid long literal: " + value);
				if (!System.Char.IsDigit(value[index]))
					throw new Nutbox.Exception("Invalid long literal: " + value);

				// parse the digits of the number
				for (; index < value.Length && System.Char.IsDigit(value[index]); index += 1)
				{
					result *= 10;	// note: csc /checked+ handles overflows...
					// yet another lame VB API follows.  Why convert to double?
					result += (long) System.Char.GetNumericValue(value[index]);
				}

				// parse suffix, if any
				if (index < value.Length)
				{
					long factor = 0;

					switch (System.Char.ToUpperInvariant(value[index]))
					{
						case 'K': factor =          1024; break;
						case 'M': factor =       1048576; break;
						case 'G': factor =    1073741824; break;
						case 'T': factor = 1099511627776; break;
						default:
							throw new Nutbox.Exception("Invalid long literal: " + value);
					}
					index += 1;

					// skip optional trailing 'B' (make it optional to comply with POSIX)
					if (index < value.Length)
					{
						if (System.Char.ToUpperInvariant(value[index]) != 'B')
							throw new Nutbox.Exception("Invalid long literal: " + value);
						index += 1;
					}

					result *= factor;
				}

				if (index != value.Length)
					throw new Nutbox.Exception("Invalid long literal: " + value);

				result *= sign;

				return result;
			}
		}
	}
}
