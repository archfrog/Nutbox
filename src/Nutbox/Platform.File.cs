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
	using System.Collections.Generic;

	public sealed class File
	{
		// File:
		// Required by FxCop v1.36.
		private File()
		{
		}

		/// <summary>
		/// Enhanced version of .NET v2.0 System.IO.File.Delete.  This method
		/// takes a boolean variable that indicates whether it should force the
		/// deletion (i.e. delete read-only files) or not.
		/// </summary>
		/// <param name="name">The disk name of the file to delete.</param>
		/// <param name="force">Indicates whether read-only files should be
		/// deleted.</param>
		public static void Delete(string name, bool force)
		{
			if (force)
				System.IO.File.SetAttributes(name, System.IO.FileAttributes.Normal);

			System.IO.File.Delete(name);
		}

		// Find:
		// Finds all files matching the specified wildcard.
		// If 'recurse' is true, it searches all subdirectories too.
		public static string[] Find(string wildcard, bool recurse)
		{
			// set up sentinel used to as a cludge around empty directory names
			char sep = System.IO.Path.DirectorySeparatorChar;
			string cludge = "." + sep + "." + sep + "." + sep + "." + sep + ".";

			// expand wildcard and check that each file exists
			List<string> result = new List<string>();

			// handle the lame .NET case where the directory name is empty
			// this is done by using a sentinel of @".\.\.\.\."
			string directory = System.IO.Path.GetDirectoryName(wildcard);
			if (directory.Length == 0)
				directory = cludge;

			// skip system-reserved directories (to avoid errors)
			string abspath = System.IO.Path.GetFullPath(directory);
			if (Nutbox.Platform.Directory.IsSystemReserved(abspath))
				return new string[0];

			string filename = System.IO.Path.GetFileName(wildcard);
			string[] files = System.IO.Directory.GetFiles(directory, filename);

			// add each file to our list of files to process
			foreach (string file in files)
			{
				// skip duplicate directories (to avoid lame output)
				if (result.Contains(file))
					continue;

				// remove the sentinel, if any
				string name = file;
				if (name.IndexOf(cludge, System.StringComparison.Ordinal) == 0)
					name = file.Substring(cludge.Length + 1);

				// add the directory to our list of directories to process
				result.Add(name);
			}

			// recurse subdirectories, if applicable
			if (recurse)
			{
				string[] subdirs = System.IO.Directory.GetDirectories(directory, "*");

				// ... iterate over found subdirectories
				foreach (string dir in subdirs)
				{
					string[] children = Find(dir + Directory.Sep + filename, recurse);

					// ... iterate over each found file in the current subdir
					foreach (string child in children)
					{
						// remove the sentinel, if any
						string name = child;
						if (name.IndexOf(cludge, System.StringComparison.Ordinal) == 0)
							name = name.Substring(cludge.Length);

						result.Add(name);
					}
				}
			}

			return result.ToArray();
		}

		public static string[] Find(string[] wildcards, bool recurse)
		{
			List<string> result = new List<string>();

			foreach (string wildcard in wildcards)
			{
				string[] matches = Find(wildcard, recurse);

				// report error if the wildcard didn't match anything
				if (matches.Length == 0)
					throw new Nutbox.Exception("No matches found: " + wildcard);

				// copy to our assembled list of directories
				foreach (string match in matches)
					result.Add(match);
			}

			// convert the collection to an array of string
			return result.ToArray();
		}

		// Locate:
		// Locates all the occurences of 'name' in the specified directory list.
		public static string[] Locate(string[] dirs, string name)
		{
			List<string> result = new List<string>();

			// walk through the PATH enviroment variable and record matches
			foreach (string dir in dirs)
			{
				// ignore empty directories (handle malformed PATHs)
				if (dir.Length == 0)
					continue;

				// force all paths into absolute paths so as to be consistent
				string abspath = System.IO.Path.GetFullPath(dir);

				// ignore empty path specifications
				if (abspath.Length == 0)
					continue;

				// ensure the path is terminated with a backslash (\)
				if (abspath[abspath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
					abspath += System.IO.Path.DirectorySeparatorChar;

				// add the name of the command itself
				abspath += name;

				// check if the specified name exists in the current directory
				if (System.IO.File.Exists(abspath))
					result.Add(abspath);
			}

			return result.ToArray();
		}

		public static System.DateTime TimeOf(string name)
		{
			return System.IO.File.GetLastWriteTime(name);
		}
	}
}
