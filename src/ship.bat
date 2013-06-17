@echo off
rem Batch file for creating the Nutbox distribution archives
if "%2" == "" goto ErrorSyntax

rem Ship binaries
pushd ..
if exist %2\nutbox-%1-bin.zip del %2\nutbox-%1-bin.zip > nul
if errorlevel 1 goto Error
zip -9rX %2\nutbox-%1-bin.zip * -x .git\* -x bld\* -x new\* -x obj\* -x ref\* -x src\* -x tst\* > nul
if errorlevel 1 goto Error
popd
if errorlevel 1 goto Error

rem Ship sources
pushd ..
if errorlevel 1 goto Error
if exist %2\nutbox-%1-src.zip del %2\nutbox-%1-src.zip > nul
if errorlevel 1 goto Error
zip -9rX %2\nutbox-%1-src.zip * -x *.hlp.cs -x *.log -x *.bnf -x .git\* -x bin\* -x new\* -x obj\* -x ref\* > nul
if errorlevel 1 goto Error
popd
if errorlevel 1 goto Error

goto End

:ErrorSyntax
echo Syntax: "ship.bat" version target
echo.
echo Creates the Nutbox distribution archives.
goto End

:Error
echo Error: Something bad happened
goto End

:End

