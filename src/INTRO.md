# Nutbox Maintainer's Introduction
Welcome to the Nutbox source code.  It is written purely in C# and all commands
use a standard format that ensures consistent and predictable behavior:

1. Each command is stored in its own subdirectory off the `src` subdirectory.
2. Each command consists of two files: `foo.cs` and `foo.hlp`.
3. The program code of each command is stored in `foo/foo.cs`, where `foo` is
   the name of a command.
4. The help text (documentation) for each command is stored in `foo/foo.hlp`,
   where `foo` is a sample command name.
5. Each program module (`foo.cs`) defines two classes: `Setup` and `Program`.
6. `foo.Setup` always derives from `Nutbox.Setup`.  It defines all the options
   and parameters, and the command-line syntax of the command in question.
7. `foo.Program` contains the main program code.
8. The `Main()` program of `foo` only creates an instance of `foo.Setup` and
   `foo.Program` and then transfers control to `base.Main(Setup)`.
9. `base.Main(Setup)` then takes care of the following things:

  a. Parsing the parameters (by invoking `Nutbox.Setup` to do that).
  b. Displaying the logo, if the `--logo` option is specified (not used
     anymore).
  c. Displaying the help text, if the `--help` option is specified
     (this causes the program to exit with a status of two).
  e. Invoking `foo.Main(Setup)` to execute the actual program code.
  f. Catching exceptions and printing suitable error messages.

So the model is:

1. The program creates a `program.Setup` instance, which is defined to match
   the needs of the program.  The `Setup` instance defines all the properties
   that the program has.
2. The constructor of the `program.Setup` instance defines the options and
   other parameters accepted by the program by calling `base.Add()`.
3. The program then passes on the parameters and the newly created `Setup`
   instance to `Nutbox.Program.Main(Setup, string[])`.
4. `Nutbox.Program.Main()` then asks `Nutbox.Setup` to parse the parameters
   according to the rules defined by `program.Setup.Setup()`.
5. `Nutbox.Program.Main()` then INVOKES `program.Main(Setup)`, by doing a sort
   of callback, which then implements the program code.
6. `Nutbox.Program.Main()` is the sole agent that handles exceptions.  This
   is done this way to ensure CONSISTENCY and PREDICTABILITY: That ALL
   commands work precisely the same way when it comes to handling exceptions.
   If new exceptions are added to the Nutbox system, only
   `Nutbox.Program.Main()` needs to be modified.

## Notes
1. Member variables (fields) are generally named `mXxx`, where `Xxx` is the
   actual name of the member variable.  This is supplemented by proper
   properties of the form `Xxx`: The property `Xxx` returns mXxx in its `get`
   method and updates `mXxx` in its `set` method.
2. The code is written with readability and maintainability in mind, not
   speed.  The only reason I do `if (text.Length == 0)` checks (instead of
   `if (text == "")`) is because Microsoft's FxCop whines on the latter
   form - even though it is ten times more readable than the first form.
3. I generally strive to, but have not yet accomplished, FxCop compliance.
   Some things will never be done the FxCop way, I think, in Nutbox because
   a few of the FxCop requirements are lame.  Most of them are very sensible,
   though.
4. `AssemblyInfo.cs` is EMBEDDED into the various command's source files.  I
   choose to do so because I cannot see any need for an external
   `AssemblyInfo.cs` file - and I hate to forget things just because I never
   accidentally stumble across them.  By putting them at the very beginning of
   each source file, I am certain that I will eventually see the
   `AssemblyInfo` attributes - and therefore discover errors in them.
5. The version number is bumped using the Nutbox `fileedit` command:
   `fileedit 0.11.11.0=0.12.12.0 *.cs --recurse`
6. Options may appear anywhere in the command-line (this should be
   documented somewhere else, but now I've at least written it once).

## .NET Version
Nutbox is currently built using the Microsoft.NET v4.0 compiler.  I
occasionally build using Mono to test if everything works as expected.  Nutbox
also builds out-of-the-box using Microsoft.NET v2.0.

## Patches
If you want to submit patches, please simply submit two things:

    1. The modified source files.
    2. The version number that the source files were when you started.

Then I'll do the tedious job of diffing and merging until everything's okay.
(I know about 'patch', etc., but I like to know about the changes to Nutbox.)

## Code Reuse
Code is shared globally by putting it in the src/Nutbox directory.  It is
currently somewhat messy, but I will eventually get around to clean it up.
But it does contain some quite useful classes, methods, and types.  You are
welcome to copy and paste code from src/Nutbox into your own applications -
without stating anywhere that you did so (this is part of the Nutbox Public
Domain license!).

## Contact
I can be contacted at the email [Mikael Egevig](mailto:mikael@egevig.org).

