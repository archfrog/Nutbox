README.txt for Nutbox

SUMMARY
Nutbox is an ever-growing collection of tools for the Microsoft .NET and Mono 
platforms.  Nutbox is portable to Windows, Linux, Solaris, and Macintosh.

USING MONO
The main difference between using .NET and Mono is that you have to invoke Mono 
explicitly to execute the command in question.  So where you'd write:

    which foo.exe

on Windows, you have to write:

    mono $NUTBOX/bin/which.exe foo.exe

If, however, you use the INSTALL.sh shell script provided, it will create 
suitable wrapper scripts so that you only need to write:

    which foo.exe

Just like under Windows.

Obviously, it would be really neat if somebody where to make a patch to the
Linux kernel so that it could invoke Mono silently behind the scene when it 
encounters a .NET executable.  But, then again, the current method works well,
so why bother?

USING dotGNU
I have tried to run Nutbox under dotGNU, but I get bizarre errors about the
image having invalid metadata.  Nutbox runs fine on Microsoft .NET and on 
Mono, so I cannot believe these error messages - or, perhaps, I can
believe them, but feel that dotGNU should handle the Microsoft .NET v2.0.50727
executables whether there are errors in them or not.  If you succeed in	running	
Nutbox under dotGNU, please let me know by email.

The errors are:

	C:\Program Files (x86)\Portable.NET\0.8.0\bin>ilrun u:bin\basename.exe
	metadata error in token 0x1B000001: invalid type specification
	metadata error in token 0x02000004: cannot locate parent type
	u:bin\basename.exe: invalid metadata in image

I get these errors both when building with Microsoft.NET and when building
with Mono.  It may be that dotGNU is only source compatible with .NET, but that
would sort of defeat the whole purpose of dotGNU, IMHO.

DOCUMENTATION
The documentation for each Nutbox command is now included in the tool itself:
Simply invoke the tool in question with the '--help' option and it displays a
multi-page help screen.  To make life easier on yourself, you can pipe the
output of the command in question over into 'more':

     command --help | more

No plans exists to create separate documentation.

CONTACT
I can be contacted at mikael@lyngvig.org - please do not hesitate to contact
me with your concerns, be it bugs, troubles using a command, or unexpected
behavior.  Nutbox is still in its infancy and therefore prone to have bugs and
undesired behavior - and especially to lack features that you want or need.

WEBSITE
The official website of Nutbox is: http://www.nutbox.org.

EMAIL
My name is Mikael Lyngvig and my email address is mikael@lyngvig.org.
