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

namespace Org.Lyngvig.Nutbox.Platform
{
	using System.Collections.Generic;	// List<string>

	/// <summary>
	/// A generic wrapper class for useful disk-oriented static methods.
	/// </summary>
	/// <returns></returns>
	public sealed class Disk
	{
		/// <summary>
		/// Private constructor that ensures that nobody instantiates this class.
		/// </summary>
		/// <returns></returns>
		private Disk()
		{
		}

		/// <summary>
		/// Copies the time stamp of a file or a directory to another file or
		/// directory, handling any combination of file/dir to file/dir.
		/// </summary>
		/// <param name="source">The name of the disk item supplying the time stamp.</param>
		/// <param name="target">The name of the disk item receiving the time stamp.</param>
		/// <returns></returns>
		public static void CopyTimeStamp(string source, string target)
		{
			System.IO.FileSystemInfo sourceinfo;
			System.IO.FileSystemInfo targetinfo;

			if (System.IO.Directory.Exists(source))
				sourceinfo = new System.IO.DirectoryInfo(source);
			else if (System.IO.File.Exists(source))
				sourceinfo = new System.IO.FileInfo(source);
			else
				throw new Nutbox.Exception("File not found: " + source);

			if (System.IO.Directory.Exists(target))
				targetinfo = new System.IO.DirectoryInfo(target);
			else if (System.IO.File.Exists(target))
				targetinfo = new System.IO.FileInfo(target);
			else
				throw new Nutbox.Exception("File not found: " + target);

			targetinfo.CreationTimeUtc   = sourceinfo.CreationTimeUtc;
			targetinfo.LastAccessTimeUtc = sourceinfo.LastAccessTimeUtc;
			targetinfo.LastWriteTimeUtc  = sourceinfo.LastWriteTimeUtc;
		}

		/// <summary>
		/// Enhanced version of System.IO.File.Exists that handles both files
		/// and directories.  This version works a bit like CMD.EXE's exists
		/// function.
		/// </summary>
		/// <param name="name">The name of the disk item to check for existence.</param>
		/// <returns></returns>
		public static bool Exists(string name)
		{
			if (System.IO.File.Exists(name))
				return true;
			if (System.IO.Directory.Exists(name))
				return true;

			return false;
		}

		public static bool IsAbsolute(string value)
		{
			char sep = System.IO.Path.DirectorySeparatorChar;

			// paths that begin with a slash are absolute paths
			if (value.Length >= 1 && value[0] == sep)
				return true;

			// paths that contain a drive letter and colon are absolute paths
			if (value.Length >= 2 && value[1] == ':')
				return true;

			// paths that begin with two slashes are absolute paths
			// note: this code is redundant but nice for readability...
			if (value.Length >= 2 && value[0] == sep && value[1] == sep)
				return true;

			return false;
		}

		/// <summary>
		/// Enhanced version of System.IO.File.GetLastWriteTimeUtc() that works
		/// on both files and directories.  Very useful when you don't know
		/// what the type of the disk item is.
		/// </summary>
		/// <param name="name">The name of the disk item you want the modified
		/// time of.</param>
		/// <returns></returns>
		public static System.DateTime GetLastWriteTimeUtc(string name)
		{
			if (System.IO.File.Exists(name))
				return System.IO.File.GetLastWriteTimeUtc(name);
			else if (System.IO.Directory.Exists(name))
				return System.IO.Directory.GetLastWriteTimeUtc(name);
			else
				throw new Nutbox.Exception("Unable to get time of item: " + name);
		}

		/// <summary>
		/// Moves the specified source file to the specified target name.  The
		/// code that follows is long-haired, but that's because we need to
		/// handle the case that the target is on a different volume and
		/// therefore have to be copied rather than moved.
		/// </summary>
		/// <param name="source">The source item to move.</param>
		/// <param name="target">The name/location where to move to.</param>
		/// <returns>Nothing.</returns>
		public static void Move(string source, string target, bool overwrite)
		{
			// first, let's normalize the parameters
			string source_fullpath = System.IO.Path.GetFullPath(source);
			string target_fullpath = System.IO.Path.GetFullPath(target);

			// ensure we're not moving the source unto the source
			if (source_fullpath == target_fullpath)
				throw new Nutbox.Exception("Source equals target in move operation: " + source);

			// if moving into a directory, patch up the target name
			if (System.IO.Directory.Exists(target_fullpath))
			{
				target_fullpath += System.IO.Path.DirectorySeparatorChar;
				target_fullpath += System.IO.Path.GetFileName(source_fullpath);
			}

			// if we are prohibited from overwriting targets, bail out now
			if (System.IO.File.Exists(target_fullpath) && !overwrite)
				throw new Nutbox.Exception("Target exists: " + target);

			// if on different volumes, copy instead of moving
			// note: we do not handle *nix or networked volumes...
			bool copy = false;
			if (source.Length >= 2 && source[1] == ':' &&
				target.Length >= 2 && target[1] == ':' && source[0] != target[0])
				copy = true;

			// if we're moving within a volume, let .NET handle the rest
			if (!copy)
			{
				System.IO.Directory.Move(source_fullpath, target_fullpath);
				return;
			}

			// sic, now we have to copy and delete the sources afterwards
			// note: GNU 'mv' moves read-only files as if they were writable,
			// note: let us do the same.

			// ... if the source is merely a file, let .NET handle the details
			if (System.IO.File.Exists(source))
			{
				System.IO.File.Copy(source_fullpath, target_fullpath, overwrite);
				return;
			}

			// sic, sic, sic.  We now need to copy an entire directory tree
			throw new Nutbox.InternalError("Feature (directory move) not implemented");
		}

		/// <summary>
		/// Normalize looks up the actual disk name of the specified value.  In
		/// other words, Normalize() goes through every part of the path
		/// specified and replaces it with the actual disk name found on disk.
		/// This has no effect on Unix systems, that are case sensitive, but on
		/// Windows, it has the effect that the path is converted to whatever
		/// format it actually has on disk.  Obviously, the path must be a valid
		/// item already stored on the disk for this to make sense.
		/// </summary>
		/// <param name="value">The path to normalize.</param>
		/// <returns>The normalized path.</returns>
		public static string Normalize(string value)
		{
			// convert System.IO.Path.DirectorySeparatorChar to a string
			string sep = new System.String(System.IO.Path.DirectorySeparatorChar, 1);

			// check that there's something to normalize
			string[] parts = value.Split(sep[0]);
			if (parts.Length == 0)
				throw new Nutbox.Exception("Empty path detected");

			// walk through each part of the path and normalize it in turn
			List<string> path = new List<string>();
			for (int i = 0; i < parts.Length; i += 1)
			{
				string part = parts[i];

				// if an empty directory, add it without further processing
				if (part.Length == 0)
				{
					// only the first path element may be empty (\xxx)
					if (i == 0)
					{
						if (parts.Length >= 2 && parts[1].Length == 0)
							throw new InternalError("Nutbox generally does not support UNC names yet...");
						path.Add(part);
					}

					// silently discard double separators (they happen...)
					continue;
				}

				// current or parent directory designates pass through unchanged
				if (part == "." || part == "..")
				{
					path.Add(part);
					continue;
				}

				// path elements containing wildcards (usually the last part...)
				if (part.IndexOf('?') != -1 || part.IndexOf('*') != -1)
				{
					path.Add(part);
					continue;
				}

				// drive names are converted to uppercase
				if (part.Length == 2 && part[1] == ':')
				{
					string disk = System.Char.ToUpperInvariant(part[0]) + ":";
					path.Add(disk);
					continue;
				}

				// todo: UNC names (something about looking up a server...)

				// merge the path so that we can perform a lookup on it
				bool hacked = false;
				string merged;
				if (path.Count == 0)
				{
					hacked = true;
					merged = ".";
				}
				else
					merged = System.String.Join(sep, path.ToArray()) + sep;

				// perform the actual disk lookup of the current path item
				string[] matches = System.IO.Directory.GetFileSystemEntries(merged, part);
				if (matches.Length == 0)
					throw new Nutbox.Exception("No matches found: " + merged + part);
				if (matches.Length != 1)
					throw new Nutbox.Exception("Multiple matches found: " + merged + part);

				// we found something and it was usable, add it
				string match = matches[0];
				if (hacked)
					match = match.Substring(2);
				path.Add(System.IO.Path.GetFileName(match));
			}

			return System.String.Join(sep, path.ToArray());
		}
	}
}
