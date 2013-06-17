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
	using System.Collections.Generic;		// List<T>

	public sealed class Directory
	{
		// Directory:
		// Satisfy FxCop v1.36 demands...
		private Directory()
		{
		}

		public static readonly char Sep = System.IO.Path.DirectorySeparatorChar;

		// Delete:
		// Enhanced version of System.IO.Directory.Delete that takes a 'force'
		// parameter.  If this is true, read-only items are also removed.
		public static void Delete(string name, bool recurse, bool force)
		{
			// silently ignore the request if the force option is enabled
			if (force && !System.IO.Directory.Exists(name))
				return;

			// if recursing, recursively process each subdirectory
			if (recurse)
			{
				string[] subdirs = System.IO.Directory.GetDirectories(name);

				foreach (string subdir in subdirs)
					Delete(subdir, recurse, force);
			}

			// if recursing, delete all files in the current directory
			if (recurse)
			{
				string[] files = System.IO.Directory.GetFiles(name);

				foreach (string file in files)
				{
					if (force)
						System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
					System.IO.File.Delete(file);
				}
			}

			// remove the read-only attribute, if any
			if (force)
				System.IO.File.SetAttributes(name, System.IO.FileAttributes.Normal);

			// now delete the actual directory itself
			System.IO.Directory.Delete(name, recurse);
		}

#if false
		// FindEx(string, bool):
		// Alternative implementation of Find() that handles embedded wildcards
		// FindEx() handles the case of "*\*\*.cs".
		public static string[] FindEx(string wildcard, bool recurse)
		{
			List<string> result = new List<string>();

			string[] steps = wildcard.Split(Sep);
			throw new Nutbox.Exception("Unimplemented feature: FindEx()");

			// return string array instead of list
			return result.ToArray();
		}
#endif

		// Find(string, bool):
		// Wrapper around System.IO.Directory.GetDirectories() that handles
		// all sorts of bizarre cases and also support recursive processing
		public static string[] Find(string wildcard, bool recurse)
		{
			// set up sentinel used to as a cludge around empty directory names
			string cludge = "." + Sep + "." + Sep + "." + Sep + "." + Sep + ".";

			// expand wildcard and check that each file exists
			List<string> result = new List<string>();

			if (System.IO.Directory.Exists(wildcard))
			{
				// simple case where 'wildcard' is a simple directory name
				// this includes "." and ".." and all variations thereof
				result.Add(wildcard);

				if (recurse)
				{
					string[] matches = Find(wildcard + Sep + "*", recurse);

					foreach (string match in matches)
						result.Add(match);
				}
			}
			else if (wildcard.IndexOf('?') != -1 ||	wildcard.IndexOf('*') != -1)
			{
				// complex case with wildcards in the directory name
				string directory = System.IO.Path.GetDirectoryName(wildcard);
				string filename = System.IO.Path.GetFileName(wildcard);

				// handle the lame .NET case where the directory name is empty
				// this is done by using a sentinel of @".\.\.\.\."
				if (directory.Length == 0)
					directory = cludge;

				// skip system-reserved directories (to avoid errors)
				string abspath = System.IO.Path.GetFullPath(directory);
				if (Nutbox.Platform.Directory.IsSystemReserved(abspath))
					return new string[0];

				string[] dirs = System.IO.Directory.GetDirectories(directory, filename);

				// add each file to our list of files to process
				foreach (string dir in dirs)
				{
					// remove the sentinel, if any
					string name = dir;
					if (name.IndexOf(cludge, System.StringComparison.Ordinal) == 0)
						name = dir.Substring(cludge.Length + 1);

					// skip system-reserved directories (to avoid errors)
					abspath = System.IO.Path.GetFullPath(name);
					if (Nutbox.Platform.Directory.IsSystemReserved(abspath))
						continue;

					// skip duplicate directories (to avoid lame output)
					if (result.Contains(name))
						continue;

					// add the directory to our list of directories to process
					result.Add(name);
				}

				if (recurse)
				{
					string[] subdirs = System.IO.Directory.GetDirectories(directory, "*");

					foreach (string subdir in subdirs)
					{
						string[] matches = Find(subdir + Sep + filename, recurse);

						foreach (string match in matches)
						{
							if (result.Contains(match))
								continue;

							result.Add(match);
						}
					}
				}
			}
			else
				throw new Nutbox.Exception("No match found: " + wildcard);

			return result.ToArray();
		}

		public static string[] Find(string[] wildcards, bool recurse)
		{
			List<string> found = new List<string>();

			foreach (string wildcard in wildcards)
			{
				string[] matches = Find(wildcard, recurse);

				// report error if the wildcard didn't match anything
				if (matches.Length == 0)
					throw new Nutbox.Exception("No matches found: " + wildcard);

				// copy to our assembled list of directories
				foreach (string match in matches)
					found.Add(match);
			}

			// convert the collection to an array of string
			return found.ToArray();
		}

		/// <summary>
		/// Simple wrapper that tests if a given directory is empty.
		/// </summary>
		/// <param name="value">The path of the directory to test.</param>
		/// <returns>Returns true if empty, false otherwise.</returns>
		public static bool IsEmpty(string value)
		{
			return System.IO.Directory.GetFileSystemEntries(value).Length == 0;
		}

		// IsSystemReserved:
		// Return true if the specified ABSOLUTE path name is the name of a
		// reserved Windows directory
		public static bool IsSystemReserved(string value)
		{
			string path = value.Substring(1);

			switch (path)
			{
				case @":\System Volume Information":
				case @":\$RECYCLE.BIN":
				case @":\RECYCLER":
					return true;

				default:
					return false;
			}
		}

		// Size:
		// Returns the size of the specified directory (and all subdirs).
		public static long Size(string value)
		{
			long result = 0;

			// determine the size of the files in the current directory
			string[] files = System.IO.Directory.GetFiles(value, "*");
			foreach (string file in files)
			{
				System.IO.FileInfo fileinfo = new System.IO.FileInfo(file);
				result += fileinfo.Length;
				fileinfo = null;
			}
			// determine size of the subdirectories
			string[] subdirs = System.IO.Directory.GetDirectories(value, "*");
			foreach (string subdir in subdirs)
			{
				// ignore system reserved directories
				if (IsSystemReserved(System.IO.Path.GetFullPath(subdir)))
					continue;

				result += Size(subdir);
			}

			return result;
		}
	}
}
