# Nutbox Master Task List

## Open Tasks
This chapter simply lists all open tasks in the `Nutbox` project.

### High Priority
1. Add `ask` command, which allows Yes/No or Cancel/Okay questions.
2. Make `fileedit` and `filenameedit` fail when no files are specified.
3. Make `cat` support binary files as well as text files.
4. Finish up automated test suite for testing each command elaborately.

### Medium Priority
1. Add `hexdump` utility.

### Low Priority
1. Fix remaining issues reported by FxCop v1.36.  This is work in progress.
   So far about two-thirds of the warnings have been fixed.  Some of the
   warnings obviously do not apply to Nutbox for which reason they will never
   be fixed.  This includes, but is not limited to, the warnings about
   catching `System.Exception`: A stand-alone application MUST catch
   `System.Exception` to be a proper application or the user will experience
   undesirable scenarios. Furthermore, the `Nested types should not be
   visible` error is a folly; using visible nested types ENHANCES readability
   and maintainability, IMHO.

## Dropped Tasks
This chapter simply lists all tasks that were dropped for one reason or
another.

1. Make txt2cs trim the input prior to generating the output. The `trim`
   command was made just for that; why overload `txt2cs`?

## Completed Tasks
This chapter simply lists all tasks that were completed at some point in time.

1. Fix broken `dircmp` command - it processes `d:\` incorrectly because it
   strips the trailing slash prior to using the parameter.
2. Figure out standard, generic way of processing parameters. The problem is
   that I'd like to specify the set of parameters and options using a suitable
   data structure and then let the command-line parser use that data structure
   to parse the input - sort of clean a C# version of GNU getopt.  Currently,
   this hangs on me not wanting to use explicit casts everywhere in the client
   code.  I am	still contemplating the issue and will perhaps find a solution
   some day.
3. Make `dircmp` use a `Dictionary` rather than the current `Hashtable`.
4. Make `Nutbox.Options` check that options are passed in the proper order.
5. Fix `Nutbox.Options` so that it handles short options with arguments.
6. Finish up the `filename` command: Add the `diskname` subcommand.
7. Finish up the online help text for all commands.
8. Move the `Information.Syntax` descriptor into the help file.
9. Figure out why `sort` insists on putting empty lines at the end.
10. Make `rm` remove read-only files if `-force` is in effect.
11. Make `filefind` capable of reading from standard input.
12. Figure out why `gnumake clean` does not delete the debug files.
	This was caused by the empty `$(NUTBOX_INVOKE)` macro: GNU Make does not,
	unlike Nutmake, trim the input line AFTER macros have been expanded.  I
	assumed GNU Make did this but, alas, it did not.  I discovered this
	because Nutmake behaved in the same manner, in an initial version;
	something that I quickly corrected so Nutmake behaves nicely.
13. Create ad-hoc make utility, `nutmake`, which can build Nutbox on all
    platforms using the same basic `Makefile`.  Yeah, I'm too lazy to spend
	ages getting to know one of the very advanced build tools out there.
	Also, I always liked meddling with development tools and making an ad-hoc
	make tool is a perfect way of enjoying life.
14. Create strongly signed assemblies.
15. Create and use `AssemblyInfo.cs` files for all tools in Nutbox. These are
    now embedded in the source file of each command as well as in the
	`Attributes.cs` file found in the Nutbox directory.

