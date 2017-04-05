## Description
A console application for backing up an Subversion (SVN) repository. It uses svnadmin to dump the contents of a repository and optionally zips the results.

The application compiles down to one executable.

This project depends on Subversion and .NET Framework 3.5 during run-time. The source includes ConsoleFX to bind command line parameters to an object, System.Diagnostics.Process for running svnadmin and CSharpZipLib for zipping up the results.

Command line help messages looks like this:

## SVN Backup for .NET
http://github.com/cvertex/svnbackup

Backs up (or 'dumps') an SVN repository to a file using svnadmin

`svnbackup <svn path> <repository path> <dump file> -f -z`

-f interpret dump file specified as formatted utilizing symbols {stamp} and/or {name}. Default: false
-z enables zipping of the dump file. Default: false

NOTE the command line grammar suits my purposes, but you can easily change it thanks to [ConsoleFX](http://www.codeplex.com/ConsoleFX).
