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
	public class Information
	{
		private string _program;
		public string Program
		{
			get { return _program; }
			set { _program = value; }
		}

		private string _version;
		public string Version
		{
			get { return _version; }
			set { _version = value; }
		}

		private string _company;
		public string Company
		{
			get { return _company; }
			set { _company = value; }
		}

		private string _rights;
		public string Rights
		{
			get { return _rights; }
			set { _rights = value; }
		}

		private string _support;
		public string Support
		{
			get { return _support; }
			set { _support = value; }
		}

        private string _website;
        public string Website
        {
            get { return _website; }
            set { _website = value; }
        }

		private string _help;
		public string Help
		{
			get { return _help; }
			set { _help = value; }
		}

		private int _lower;
		public int Lower
		{
			get { return _lower; }
			set { _lower = value; }
		}

		private int _upper;
		public int Upper
		{
			get { return _upper; }
			set { _upper = value; }
		}

		public Information(
			string program,
			string version,
			string company,
			string rights,
			string support,
            string website,
			string help,
			int    lower,
			int    upper
		)
		{
			_program = program;
			_version = version;
			_company = company;
			_rights  = rights;
			_support = support;
            _website = website;
			_help    = help;
			_lower   = lower;
			_upper   = upper;
		}
	}
}
