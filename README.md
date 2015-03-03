# The Nutbox Console Utility Collection
Nutbox is a very slowly growing collection of tools for the Microsoft .NET and Mono platforms.  Nutbox is portable to Windows,
Linux, Solaris, and Macintosh, and perhaps even more systems.

## Using Nutbox with Mono
The main difference between using .NET and Mono is that you have to invoke Mono explicitly to execute the command in question.  So
where you'd write:

    which foo.exe

on Windows, you have to write:

    mono $NUTBOX/bin/which.exe foo.exe

If, however, you use the INSTALL.sh shell script provided, it will create suitable wrapper scripts so that you only need to write:

    which foo.exe

Just like under Windows.  If you don't want to use the `INSTALL.sh` script and you're using Linux, you can use these commands to
make the Linux kernel recognize .NET executables and automatically invoke Mono.  Do this once only for each system:

	su
	echo 'binfmt_misc /proc/sys/fs/binfmt_misc binfmt_misc none' >> /etc/fstab

Then do this on every boot (presumably by modifying `/etc/rc.local`):

	modprobe binfmt
	echo ':CLR:M::MZ::/usr/bin/mono:' > /proc/sys/fs/binfmt_misc/register

Finally, you need to mark the .NET executables as executable using the `chmod` command:

	chmod +x program.exe

This should give you fluent, seamless execution of .NET executables on Linux.

## DOCUMENTATION
The documentation for each Nutbox command is now included in the tool itself: Simply invoke the tool in question with the `--help`
option and it displays a multi-page help screen.  To make life easier on yourself, you can pipe the output of the command in
question over into `less`:

     command --help | less

No plans exists to create separate documentation as I don't like to have to maintain multiple sets of documentation.

## CONTACT
I can be contacted at mikael@lyngvig.org - please do not hesitate to contact me with your concerns, be it bugs, troubles using a
command, or unexpected behavior.  Nutbox is quite mature but obviously will have bugs and short-comings like any software.

## WEBSITE
The official website of Nutbox is: http://nutbox.lyngvig.org.

## EMAIL
My name is Mikael Lyngvig and my email address is mikael@lyngvig.org.

