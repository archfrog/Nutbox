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

using System;
using System.Collections.Generic;

namespace Org.Nutbox
{
    // DirList:
    // A hashtable containing disk information (DiskInfo) for all the items in
    // a given directory tree.
    public class DirList
    {
        /// <summary>
        /// DirMap is simply an abbreviation for Dictionary<string, DiskItem>.
        /// </summary>
        public class DirMap: Dictionary<string, DiskItem>
        {
        }

        // I AM looking for a better way of doing this, but this must suffice
        // until the happy, glorious day that I find a better alternative.
        private System.Exception _exception;    // exception, if any
        public System.Exception Exception
        {
            get { return _exception; }
            set { _exception = value; }
        }

        // the list of exclusions
        private List<string> _exclude;
        public List<string> Exclude
        {
            get { return _exclude; }
        }

        // the root directory of the directory tree listing
        string _origin;

        DirMap _values = new DirMap();
        public DirMap Values
        {
            get	{ return _values; }
        }

        // DirList:
        // Construct a new DirList of a given directory tree
        public DirList(string origin) :
            this(origin, new List<string>())

        {
        }

        // DirList:
        // Construct a new DirList of a given directory tree with exclusions.
        public DirList(string origin, List<string> exclude)
        {
            _origin = origin;
            if (_origin[_origin.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                _origin += System.IO.Path.DirectorySeparatorChar;

            // save instance-wide copy of list of excluded directories
            _exclude = exclude;
        }

        // Load:
        // Single-threaded version DirList loader.  Does not catch exceptions.
        public void Load()
        {
            Exception = null;
            treeWalk(_origin);
        }

        // ThreadedLoad:
        // Thread-oriented version of Load() - this version catches exceptions,
        // if any, and records them in .Exception.
        public void ThreadedLoad()
        {
            // note: .NET does not automatically propagate subthread
            // note: exceptions to the main thread, so we have to do it
            // note: manually by recording any exceptions and later on
            // note: re-throwing them (as far as silly me knows...).
            try
            {
                Exception = null;
                treeWalk(_origin);
            }
            catch (System.Exception that)
            {
                // propagate the exception to the client of this module
                Exception = that;
            }
        }

        // treeWalk:
        // Recursive method that first adds all files to the DirList and then
        // recurses the subdirectories
        void treeWalk(string origin)
        {
            // process exclusions - ignore excluded directories
            string basename = origin.Substring(_origin.Length - 1);

            // ignore excluded directories
            if (_exclude.IndexOf(basename) != -1)
                return;

            // ignore "?:\System Volume Information", "?:\$RECYCLE.BIN", "?:\RECYCLER"
            if (Nutbox.Platform.Directory.IsSystemReserved(origin))
                return;

            // save the attributes of the directory in question
            _values[basename] = new DiskItem(origin, new System.IO.DirectoryInfo(origin));

            // add each file in the directory
            foreach (string file in System.IO.Directory.GetFiles(origin))
            {
                // remove the prefix (the absolute path given as a parameter)
                string name = file.Substring(_origin.Length - 1);

                // ignore excluded files
                if (_exclude.IndexOf(name) != -1)
                    continue;

                _values[name] = new DiskItem(file, new System.IO.FileInfo(file));
            }

            // recursively process each subdirectory
            foreach (string dir in System.IO.Directory.GetDirectories(origin))
            {
                treeWalk(dir);
            }
        }
    }
}
