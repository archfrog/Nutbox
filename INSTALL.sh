#!/bin/bash
# BASH shell script for installing Nutbox on *nix.
# Execute using the command: bash ./INSTALL.sh source target
# Note: Assumes mono is in the path, so you better make sure it is there!
# Copyleft (-) 2009-2020 Mikael Egevig.  Donated to the Public Domain.
if [ "$2" == "" ]; then
	echo Syntax: INSTALL.sh source target;
	echo;
	echo Where 'source' is the source directory of the temporary copy of Nutbox.;
	echo Where 'target' is the target directory to install Nutbox to.;
	exit 0;
fi

# if the source directory does not exist, bail out
if [ ! -d $1 ]; then
	echo Error: The source directory does not exist: $1;
	exit 1;
fi

# if unable to locate INSTALL.sh (the root dir), bail out
if [ ! -f $1/INSTALL.sh ]; then
	echo Error: Unable to locate the root of the Nutbox temporary copy.;
	exit 1;
fi

# if the target directory exists, bail out
if [ -d $2 ]; then
	echo Error: The target directory already exists: $2;
	exit 1;
fi

# make the target directory and copy bin and doc only
mkdir $2
cp -a $1/bin $2/bin
cp -a $1/doc $2/doc

# create shell wrappers that invoke mono automatically
# change the access attributes to "+x-w" at the same time
mono $MONO_OPTIONS $2/bin/monowrap.exe --chmod $2/bin/*.exe
chmod a-w-x $2/bin/*.exe $2/bin/*.dll

