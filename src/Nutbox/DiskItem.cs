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

using System;
using System.IO;

namespace Org.Egevig.Nutbox
{
    // DiskItem:
    // Contains the information we want to compare about each disk item (a file or a directory)
    public class DiskItem
    {
        private string _name;
        public string Name                  // name of the disk item
        {
            get { return _name; }
            set { _name = value; }
        }

        private System.DateTime _modified;   // modified time of the disk item
        public System.DateTime Modified      // modified time of the disk item
        {
            get { return _modified; }
            set { _modified = value; }
        }

        private long _size;                 // size of the disk item
        public long Size
        {
            get { return _size; }
            set { _size = value; }
        }

        private bool _isDirectory;
        public bool IsDirectory             // true => the disk item is a directory
        {
            get { return _isDirectory; }
            set { _isDirectory = value; }
        }

        // constructor DiskItem:
        // Construct a DiskItem instance from an absolute path name and a .NET DirectoryInfo instance
        public DiskItem(string name, DirectoryInfo info)
        {
            Name = name;
            Modified = info.LastWriteTimeUtc;
            // FxCop v1.36 barks on this one.  I like coding explicitly, though...
            Size = 0;                   // only compared against other directories
            IsDirectory = true;
        }

        // constructor DiskItem:
        // Construct a DiskItem instance from an absolute path name and a .NET FileInfo instance
        public DiskItem(string name, FileInfo info)
        {
            Name = name;
            Modified = info.LastWriteTimeUtc;
            Size = info.Length;
            // FxCop v1.36 barks on this one.  I like coding explicitly, though...
            IsDirectory = false;
        }

        // FileDataCompare:
        // Compares the data of two files and returns a boolean that indicates whether they're equal.
        // todo: Place somewhere globally as a general utility function.
        public static bool FileDataCompare(string source, string target)
        {
            FileStream sourcefile = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileStream targetfile = new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] sourcebuf = new byte[65536];
            byte[] targetbuf = new byte[65536];

            // manually check that the requirements are met
            System.Diagnostics.Debug.Assert(sourcebuf.Length == targetbuf.Length);

            // process both files to the end or a difference is found
            for (; ; )
            {
                // read a block from the source and from the target
                int sourceread = sourcefile.Read(sourcebuf, 0, sourcebuf.Length);
                int targetread = targetfile.Read(targetbuf, 0, targetbuf.Length);

                // if the files are of different length, their content differ
                if (sourceread != targetread)
                    return false;

                // if either .Read() call returned zero, we're done
                if (sourceread == 0)
                    return true;

                // todo: figure out more efficient way of doing this (compare substrings?)
                for (int index = 0; index < sourceread; index++)
                {
                    if (sourcebuf[index] != targetbuf[index])
                        return false;
                }
            }
        }

        // Comparison:
        // Structure that indicates the result of a comparison of two files.
        public struct Comparison
        {
            private bool _dataChanged;
            public bool DataChanged     // the files' content were different
            {
                get { return _dataChanged; }
                set { _dataChanged = value; }
            }

            private bool _typeChanged;
            public bool TypeChanged     // the type of the files were different
            {
                get { return _typeChanged; }
                set { _typeChanged = value; }
            }

            private bool _timeChanged;
            public bool TimeChanged     // the time of the files were different
            {
                get { return _timeChanged; }
                set { _timeChanged = value; }
            }

            private bool _sizeChanged;  // the size of the files were different
            public bool SizeChanged
            {
                get { return _sizeChanged; }
                set { _sizeChanged = value; }
            }
        }

        // DiskItemCompare:
        // Compares two DiskItem instances and returns a Comparison struct indicating the result
        public static Comparison DiskItemCompare(DiskItem first, DiskItem other, bool deep)
        {
            Comparison result = new Comparison();
            bool IsDirectory;

            result.TimeChanged = (first.Modified != other.Modified);
            result.SizeChanged = (first.Size != other.Size);
            result.TypeChanged = (first.IsDirectory ^ other.IsDirectory);
            IsDirectory = first.IsDirectory | other.IsDirectory;

            // if the -deep option was specified, compare the contents of the files
            if (deep && !IsDirectory && !result.SizeChanged && !result.TimeChanged && !result.TypeChanged)
                result.DataChanged = !FileDataCompare(first.Name, other.Name);
            else
                result.DataChanged = false;

            return result;
        }
    }
}
