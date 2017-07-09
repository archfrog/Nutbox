#region license
// Copyleft (-) 2009-2017 Mikael Egevig (mikael@egevig.org).  Donated to the Public Domain.
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

//****************************************************************************
// KNOWN BUGS:
//
//    1. dircmp does not properly handle junctions, links, etc.  Only files
//       and directories are handled gracefully by the current version of
//       the program.  The result is undefined if dircmp is applied to
//       junctions, links, etc.
//
// Future:
//    1. Change the implementation to not store the source and target trees
//       in hashtables.  The current implementation easily sucks away 150
//       MEGABYTES of memory.  A proper implementation would only use
//       O(1) memory and even very little at that. (Very low priority.)
//    2. Warn if an excluded directory is never encountered (script writers!).
//****************************************************************************

using System;
using System.Collections.Generic;
using System.Threading;
using Org.Egevig.Nutbox.Options;

using System.Reflection;
[assembly: AssemblyTitle("Nutbox.dircmp")]
[assembly: AssemblyDescription("Compares or synchronizes two directory trees")]
[assembly: AssemblyConfiguration("SHIP")]
[assembly: AssemblyCompany("Mikael Egevig")]
[assembly: AssemblyProduct("Nutbox")]
[assembly: AssemblyCopyright("Copyleft (-) 2009-2017 Mikael Egevig")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyName("")]
[assembly: System.CLSCompliant(true)]

namespace Org.Egevig.Nutbox.Dircmp
{
    class Setup: Org.Egevig.Nutbox.Setup
    {
        // _deep: true => compare file contents
        private BooleanValue _deep = new BooleanValue(false);
        public bool Deep
        {
            get { return _deep.Value; }
        }

        private ListValue _exclude = new ListValue();
        public List<string> Exclude
        {
            get { return _exclude.Value; }
        }

        // _force: true => delete read-only files and dirs
        private BooleanValue _force = new BooleanValue(false);
        public bool Force
        {
            get { return _force.Value; }
        }

        // _full: true => display equal items too
        private BooleanValue _full = new BooleanValue(false);
        public bool Full
        {
            get { return _full.Value; }
        }

        private StringValue _sourcename = new StringValue(null);
        public string SourceName
        {
            get { return _sourcename.Value; }
        }

        // _sync: true => synchronize target with source
        private BooleanValue _sync = new BooleanValue(false);
        public bool Sync
        {
            get { return _sync.Value; }
        }

        private StringValue _targetname = new StringValue(null);
        public string TargetName
        {
            get { return _targetname.Value; }
        }

        public Setup()
        {
            Option[] options =
            {
                // options MUST be listed before parameters
                new TrueOption("deep", _deep),
                new FalseOption("nodeep", _deep),
                new TrueOption("force", _force),
                new FalseOption("noforce", _force),
                new TrueOption("full", _full),
                new FalseOption("nofull", _full),
                new TrueOption("sync", _sync),
                new FalseOption("nosync", _sync),
                new ListOption("exclude", _exclude),
                new StringParameter(1, "source", _sourcename, Option.eMode.Mandatory),
                new StringParameter(2, "target", _targetname, Option.eMode.Mandatory)
            };
            base.Add(options);
        }
    }

    class Program: Org.Egevig.Nutbox.Program
    {
        static Org.Egevig.Nutbox.Information _info = new Org.Egevig.Nutbox.Information(
            "dircmp",                   	        // Program
            "v1.23",                    	        // Version
            Org.Egevig.Nutbox.Copyright.Company,    // Company
            Org.Egevig.Nutbox.Copyright.Rights,     // Rights
            Org.Egevig.Nutbox.Copyright.Support,    // Support
            Org.Egevig.Nutbox.Copyright.Website,    // Website
            Org.Egevig.Nutbox.Dircmp.Help.Text,     // Help
            Org.Egevig.Nutbox.Copyright.Lower,		// Lower
            Org.Egevig.Nutbox.Copyright.Upper		// Upper
        );

        public Program():
            base(_info)
        {
        }

        /// <summary>
        /// The Database class is simply a short name for the generic type
        /// Dictionary<string, char>.  I know no other way of doing this in C#.
        /// </summary>
        public class Database: Dictionary<string, char>
        {
        }

        // Compare:
        // Compares two hashtables (path -> DiskItem) against each other and return a
        // result hashtable (path -> status), where status is one of:
        //
        // status   meaning
        //    +     The item must be added to target (copied from source to target)
        //    -     The item must be deleted from target
        //    *     The item has changed and must be copied from source to target once again
        //    =     The item is unchanged and no operation is necessary.
        //
        // The use of strings to indicate the status is a historical remnant from the Python
        // implementation of dircmp.  Ideally, I guess one would use Dictionary<string, Status>,
        // where Status is a suitable enumeration type.
        public static Database Compare(Org.Egevig.Nutbox.DirList.DirMap source, Org.Egevig.Nutbox.DirList.DirMap target, bool Deep)
        {
            Database result = new Database();

            // ugly code follows but that's because C# hash tables being iterated are immutable
            // (even the freaking value may not be changed!) so we must finish our calculations
            // before creating the respective key/data pair in the hash table

            // assume all items in the source are "to be added" to the target (assumption is fixed below)
            foreach (string key in source.Keys)
                result[key] = '+';

            // process each item in the target set of objects
            foreach (string key in target.Keys)
            {
                if (!source.ContainsKey(key))
                {
                    result[key] = '-';  // item exists only in target: mark as to be deleted
                    continue;
                }

                // item exists in both source and target: compare items
                Org.Egevig.Nutbox.DiskItem.Comparison cmp = Org.Egevig.Nutbox.DiskItem.DiskItemCompare(
                    (DiskItem) source[key],
                    (DiskItem) target[key],
                    Deep
                );
                result[key] = (cmp.DataChanged || cmp.SizeChanged || cmp.TimeChanged || cmp.TypeChanged) ? '*' : '=';
            }

            return result;
        }

        public override void Main(Org.Egevig.Nutbox.Setup nutbox_setup)
        {
            Setup setup = (Setup) nutbox_setup;

            // check that all exclusions are relative
            foreach (string exclude in setup.Exclude)
            {
                if (exclude.Length == 0)
                    throw new Org.Egevig.Nutbox.Exception("Empty exclusion specified");

                // exclusions must start with (back)slash
                if (exclude[0] != System.IO.Path.DirectorySeparatorChar)
                    throw new Org.Egevig.Nutbox.Exception("Exclusion must start with a path separator: " + exclude);

                // exclusions must not end with a (back)slash
                if (exclude[exclude.Length - 1] == System.IO.Path.DirectorySeparatorChar)
                    throw new Org.Egevig.Nutbox.Exception("Exclusion must not end in a path separator: " + exclude);
            }

            // ensure the source directory exists
            if (!System.IO.Directory.Exists(setup.SourceName))
                throw new Org.Egevig.Nutbox.Exception("Source directory does not exist: " + setup.SourceName);

            // handle the case of the missing target directory
            if (!System.IO.Directory.Exists(setup.TargetName))
            {
                if (!setup.Sync)
                    // if not syncing, demand that the target exists (for scripting purposes)
                    throw new Org.Egevig.Nutbox.Exception("Target directory does not exist: " + setup.TargetName);

                // if syncing, create the non-existing target directory
                System.IO.Directory.CreateDirectory(setup.TargetName);
            }

            // note: we here strip the trailing backslash so as to normalize
            // note: all names to a known format (makes stuff simpler later
            // note: on when we concatenate path elements to form a path).
            string _sourcename = System.IO.Path.GetFullPath(setup.SourceName);
            _sourcename = Org.Egevig.Nutbox.Platform.Disk.Normalize(_sourcename);
            if (_sourcename[_sourcename.Length - 1] == System.IO.Path.DirectorySeparatorChar)
                _sourcename = _sourcename.Substring(0, _sourcename.Length - 1);

            // note: we here strip the trailing backslash so as to normalize
            // note: all names to a known format (makes stuff simpler later
            // note: on when we concatenate path elements to form a path).
            string _targetname = System.IO.Path.GetFullPath(setup.TargetName);
            _targetname = Org.Egevig.Nutbox.Platform.Disk.Normalize(_targetname);
            if (_targetname[_targetname.Length - 1] == System.IO.Path.DirectorySeparatorChar)
                _targetname = _targetname.Substring(0, _targetname.Length - 1);

            // handle the case of source being identical to target
            // we flag this as an error for the sake of script writers
            if (_sourcename == _targetname)
                throw new Org.Egevig.Nutbox.Exception("Source and target are identical");

            // create lists of items in source and in target (multi-threaded!)
            Org.Egevig.Nutbox.DirList source = new Org.Egevig.Nutbox.DirList(_sourcename, setup.Exclude);
            Org.Egevig.Nutbox.DirList target = new Org.Egevig.Nutbox.DirList(_targetname, setup.Exclude);
            Thread sourceThread = new Thread(new ThreadStart(source.ThreadedLoad));
            Thread targetThread = new Thread(new ThreadStart(target.ThreadedLoad));
            sourceThread.Start();
            targetThread.Start();
            sourceThread.Join();
            targetThread.Join();

            // check for caught exceptions and propage them to the main thread
            if (source.Exception != null)
                throw source.Exception;
            if (target.Exception != null)
                throw target.Exception;

            // analyze source and target items
            Database comparison = Compare(source.Values, target.Values, setup.Deep);

            // sort the keys so as to report the files alphabetically
            ICollection<string> keys_unsorted = comparison.Keys;
            string[] keys = new string[comparison.Keys.Count];
            keys_unsorted.CopyTo(keys, 0);
            Array.Sort(keys);

            // report differences
            foreach (string key in keys)
            {
                // skip processing identical items unless -full is specified
                if (!setup.Full && comparison[key] == '=')
                    continue;

                // determine if the final result is a directory or not
                bool isDirectory;
                if (source.Values.ContainsKey(key))
                    isDirectory = source.Values[key].IsDirectory;
                else
                    isDirectory = target.Values[key].IsDirectory;

                // this is the core of the reporting logic ;-)
                System.Console.WriteLine(
                    "{0} {1}{2}",
                    comparison[key],
                    key,
                    // append a (back)slash to subdirectories
                    (isDirectory && key.Length > 1) ?
                        "" + System.IO.Path.DirectorySeparatorChar     :
                        ""
                );

                // the rest of the loop only applies to synchronizations
                // note: we synchronize right after having printed the
                // note: operation, so that there's some sense to it all
                if (!setup.Sync)
                    continue;

                string sourcename = _sourcename + key;
                string targetname = _targetname + key;

                // synchronize item - directory or file
                switch (comparison[key])
                {
                    case '=':
                        break;

                    case '*':
                        // if source is a directory and target is a file
                        if (source.Values[key].IsDirectory &&
                            !target.Values[key].IsDirectory)
                        {
                            Org.Egevig.Nutbox.Platform.File.Delete(targetname, setup.Force);
                            System.IO.Directory.CreateDirectory(targetname);
                        }

                        // if source is a file and target is a directory
                        if (!source.Values[key].IsDirectory &&
                            target.Values[key].IsDirectory)
                            Org.Egevig.Nutbox.Platform.Directory.Delete(targetname, true, setup.Force);

                        // post-process directories later on (copy their time stamps)
                        if (source.Values[key].IsDirectory)
                            continue;

                        // copy everything including file attributes
                        System.IO.File.Copy(sourcename, targetname, true);
                        break;

                    case '+':           // add source to target
                        // create target directory if it does not exist
                        if (source.Values[key].IsDirectory)
                        {
                            System.IO.Directory.CreateDirectory(targetname);
                            continue;
                        }

                        // note: this fixes a bug in v1.20 (and all earlier
                        // note: versions) that made 'dircmp' die if it had
                        // note: previously removed the directory itself
                        string dirname = System.IO.Path.GetDirectoryName(targetname);
                        System.IO.Directory.CreateDirectory(dirname);

                        // ensure the file has been removed if it was renamed
                        // e.g. "Foo Or Bar" is renamed to "Foo or Bar"
                        if (System.IO.File.Exists(targetname))
                            Org.Egevig.Nutbox.Platform.File.Delete(targetname, setup.Force);

                        // copy everything including file attributes
                        System.IO.File.Copy(sourcename, targetname);
                        break;

                    case '-':
                        // remove source from target
                        if (target.Values[key].IsDirectory)
                        {
                            // ignore directories that have been erased (to avoid failing)
                            if (!System.IO.Directory.Exists(targetname))
                                continue;

                            Org.Egevig.Nutbox.Platform.Directory.Delete(targetname, true, setup.Force);
                            continue;
                        }

                        if (System.IO.File.Exists(targetname))
                        {
                            Org.Egevig.Nutbox.Platform.File.Delete(targetname, setup.Force);
                        }
                        break;
                }
            }

            if (setup.Sync)
            {
                // copy directory dates of all affected directories
                // stupid to do it here, but it is a cludge of sorts
                foreach (string key in keys)
                {
                    string sourcename = _sourcename + key;
                    string targetname = _targetname + key;

                    switch (comparison[key])
                    {
                        case '=':
                            break;

                        case '*':
                            goto case '+';

                        case '+':
                            // Windoze does not allow setting the date of a root directory
                            // One of the thousands of bizarre quirks in Windows...
                            if (targetname.Length == 3 &&
                                targetname[1] == ':' &&
                                targetname[2] == System.IO.Path.DirectorySeparatorChar)
                                continue;

                            // ignore files
                            if (!source.Values[key].IsDirectory)
                                continue;

                            Org.Egevig.Nutbox.Platform.Disk.CopyTimeStamp(sourcename, targetname);
                            break;

                        case '-':
                            break;
                    }
                }
            }
        }

        public static int Main(string[] args)
        {
            Setup setup     = new Setup();
            Program program = new Program();

            // let Org.Egevig.Nutbox.Program.Main() handle exceptions, etc.
            return program.Main(setup, args);
        }
    }
}
