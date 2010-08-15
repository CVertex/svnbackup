using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ConsoleFx;
using ICSharpCode.SharpZipLib.Zip;

namespace SvnBackup {

    /// <summary>
    /// svnbackup <svn path> <repository path> <dump file> [-f] [-z] 
    /// dump file == "what-ever-format-you-please{name}{stamp}.zip" 
    /// </summary>
    [CommandLine()]
    [ParameterUsage(ProgramMode.Normal, MinOccurences = 3, MaxOccurences = 5)]
    [ParameterUsage(ProgramMode.Help, MinOccurences = 0, MaxOccurences = 0)]
    public sealed class Program: ConsoleProgram {

        [ParameterProperty(ProgramMode.Normal, 0)]
        public string SvnPath {
            get; set;
        }

        [ParameterProperty(ProgramMode.Normal, 1)]
        public string RepoPath {
            get;
            set;
        }

        [ParameterProperty(ProgramMode.Normal, 2)]
        public string DumpFile {
            get;
            set;
        }

        public bool ZipFile {
            get;
            set;
        }
        
        public bool DumpFileIsFormatString {
            get;
            set;
        }

        [ConsoleFx.Switch("f", MinParameters = 0, MaxParameters = 0)]
        [SwitchUsage(ProgramMode.Normal, SwitchUsage.Optional)]
        [SwitchUsage(ProgramMode.Help, SwitchUsage.NotAllowed)]
        public void ProcessNameSwitch(string[] parameters) {
            DumpFileIsFormatString = true;
        }

        [ConsoleFx.Switch("z", MinParameters = 0, MaxParameters = 0)]
        [SwitchUsage(ProgramMode.Normal, SwitchUsage.Optional)]
        [SwitchUsage(ProgramMode.Help, SwitchUsage.NotAllowed)]
        public void ProcessZipFileSwitch(string[] parameters) {
            ZipFile = true;
        }


        private string FormatFileName(string fn)
        {
            return fn.ToFormattedString(new {name = GetRepoName(), stamp = DateTime.Now.ToString("yyyyMMddhhmm")});
        }

        private string GetDumpFileName()
        {

            var fn = DumpFileIsFormatString ?
                        FormatFileName(DumpFile)
                        : 
                        DumpFile;

            fn = Path.ChangeExtension(fn, "svn_dump");

            return fn;
        }

        string GetSvnAdminDumpBatchFileName()
        {
            return String.Format("{0}_{1}.bat", GetRepoName(), Guid.NewGuid().ToString());
        }

        string GetSvnAdminDumpFileName()
        {
            return ZipFile? 
                
                // if it's a zip file, the svn admin dump will go to a temp file first
                Path.GetTempFileName(): 
                
                GetDumpFileName();
        }

        string GetZipFileName()
        {
            var computed = GetDumpFileName();
            computed = Path.ChangeExtension(computed, "zip");
            return computed;
        }




        string GetRepoName ()
        {
            return new DirectoryInfo(RepoPath).Name;
        }

        

        void Validate()
        {
            // TODO: various validation checks
            // check the svn path exists with svnadmin.exe
            // check the repo path exists
            // run svnadmin verify to ensure the repo is sound
            // 
            List<string> msgs = new List<string>();

            if (!Directory.Exists(RepoPath))
            {
                msgs.Add("Repository path does not exist");
            }
        }


        void Backup()
        {

            string dumpFileName = GetSvnAdminDumpFileName();

            using (var process = new Process())
            {

                ConsoleEx.WriteLine("{0} repository commencing dump to {1}", GetRepoName(), dumpFileName);

                Func<string, string> quote = (inputStr) => "\"" + inputStr + "\"";

                var batchFileName = GetSvnAdminDumpBatchFileName();

                using (var batchFile = File.CreateText(batchFileName))
                {

                    string[] args = {
                                    "dump",
                                    quote(RepoPath),
                                    ">",
                                    quote(dumpFileName)
                                };


                    List<string> parts = new List<string>();
                    parts.Add(quote(Path.Combine(SvnPath, "svnadmin")));
                    parts.AddRange(args);

                    var batchFileContents = String.Join(" ",parts.ToArray());

                    batchFile.WriteLine(batchFileContents);
                }


                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.Arguments = "";
                process.StartInfo.FileName = batchFileName;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();

                long totalReadBytes = new FileInfo(dumpFileName).Length;

                ConsoleEx.WriteLine("{0} repository successfully dumped to {1} with size of {2} bytes", GetRepoName(), dumpFileName, totalReadBytes);
                

                // delete the batch file
                try
                {
                    File.Delete(batchFileName);
                }
                catch (Exception ex)
                {
                    ConsoleEx.WriteLine("Warning! Could not delete batch file {0} - Exception message {1}", batchFileName,ex.Message);
                }
            }


            if (!File.Exists(dumpFileName))
            {
                throw new Exception("The dumpings of svnadmin doesn't exist. svnadmin failed in it's duty");
            }

            if (ZipFile)
            {

                

                // zip up dump file
                string zipFileName = GetZipFileName();

                ConsoleEx.WriteLine("{0} repository commencing zipping to {1}", GetRepoName(), zipFileName);

                // Depending on the directory this could be very large and would require more attention
                // in a commercial package.

                string[] filenames = {dumpFileName};

                // 'using' statements gaurantee the stream is closed properly which is a big source
                // of problems otherwise.  Its exception safe as well which is great.
                using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFileName))) {

                    s.SetLevel(9); // 0 - store only to 9 - means best compression

                    byte[] buffer = new byte[4096];

                    foreach (string file in filenames) {

                        // Using GetFileName makes the result compatible with XP
                        // as the resulting path is not absolute.
                        ZipEntry entry = new ZipEntry(Path.GetFileName(GetDumpFileName()));

                        // Setup the entry data as required.

                        // Crc and size are handled by the library for seakable streams
                        // so no need to do them here.

                        // Could also use the last write time or similar for the file.
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);

                        using (FileStream fs = File.OpenRead(file)) {

                            // Using a fixed size buffer here makes no noticeable difference for output
                            // but keeps a lid on memory usage.
                            int sourceBytes;
                            do {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                        }
                    }

                    // NOTE by Vijay: Finish/Close arent needed strictly as the using() disposal will call these automagically

                    // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                    // the created file would be invalid.
                    s.Finish();

                    // Close is important to wrap things up and unlock the file.
                    s.Close();

                    ConsoleEx.WriteLine("{0} repository successfully zipped to {1}", GetRepoName(), zipFileName);
                }

            }

        }


        public override int ExecuteNormal(string[] parameters) {

            try
            {
                // validate
                Backup();
                return 0;

            } catch (Exception ex)
            {
                ConsoleEx.WriteLine("Exception Occured -- {0}",ex.ToString());
                return -1;
            }

        }

        public override void DisplayUsage() {
            ConsoleEx.WriteLine("SVN Backup for .NET");
            ConsoleEx.WriteLine("http://codeplex.com/svnbackup");
            ConsoleEx.WriteLine("Copyright (c) 2009 Vijay Santhanam");
            ConsoleEx.WriteLine();
            ConsoleEx.WriteLine("Backs up (or 'dumps') an SVN repository to a file using svnadmin");
            ConsoleEx.WriteLine();
            ConsoleEx.WriteLine("svnbackup <svn path> <repository path> <dump file> [-f] [-z]");
            ConsoleEx.WriteLine(); ;
            ConsoleEx.WriteLine("  -f     interpret dump file specified as formatted utilizing symbols {stamp} and/or {name}. Default: false");
            ConsoleEx.WriteLine("  -z     enables zipping of the dump file. Default: false");
            ConsoleEx.WriteLine();
            base.DisplayUsage();
        }
     

    }
}
