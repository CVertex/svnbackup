using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ConsoleFx;
using ICSharpCode.SharpZipLib.Zip;

namespace SvnBackup {
    // svnbackup <svn path> <repository path> <dump file> [-f] [-z]
    // dump file == "what-ever-format-you-please{name}{stamp}.zip"
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

                Func<string, string> quote = (inputStr) => "\"" + inputStr + "\"";

                string[] args = {
                                    "dump",
                                    quote(RepoPath)
                                };

                

                var startInfo = new ProcessStartInfo(
                    Path.Combine(SvnPath, "svnadmin"), 
                    String.Join(" ", args)) 
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false

                };
                process.StartInfo = startInfo;
               
                process.Start();

                string svnAdminDumpResponse = "";

                // var dumped = os.ReadToEnd();
                StreamReader reader = process.StandardOutput;

                char[] dumpOutputBuffer = new char[4096];
                List<char> dumpOutput = new List<char>(100000);
                int readChars = 0;

                while (!reader.EndOfStream)
                {
                    readChars = reader.Read(dumpOutputBuffer, 0, dumpOutputBuffer.Length);
                    dumpOutput.AddRange(dumpOutputBuffer.Take(readChars));
                }


                using (StreamWriter writer = new StreamWriter(dumpFileName,false))
                {
                    writer.Write(dumpOutput.ToArray());
                }

                ConsoleEx.WriteLine("{0} repository dumped to {1}", GetRepoName(),dumpFileName);
                
            }
            if (!File.Exists(dumpFileName))
            {
                throw new Exception("The dumpings of svnadmin doesn't exist. svnadmin failed in it's duty");
            }

            if (ZipFile)
            {
                // zip up dump file
                string zipFileName = GetZipFileName();

                // Depending on the directory this could be very large and would require more attention
                // in a commercial package.

                string[] filenames = //Directory.GetFiles(args[0]);
                    {dumpFileName};

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

                    // Finish/Close arent needed strictly as the using statement does this automatically

                    // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                    // the created file would be invalid.
                    s.Finish();

                    // Close is important to wrap things up and unlock the file.
                    s.Close();

                    ConsoleEx.WriteLine("{0} repository dumped and zipped to {1}", GetRepoName(), zipFileName);
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
            ConsoleEx.WriteLine("SVN Backup 1.00 for .NET");
            ConsoleEx.WriteLine("http://www.sourceforge.net/projects/ConsoleFx");
            ConsoleEx.WriteLine("Copyright (c) 2009 Vijay Santhanam");
            ConsoleEx.WriteLine();
            ConsoleEx.WriteLine("Backs up (or 'dumps') an SVN repository to a file using svnadmin");
            ConsoleEx.WriteLine();
            ConsoleEx.WriteLine("svnbackup <svn path> <repository path> <dump file> [-f] [-z]");
            ConsoleEx.WriteLine(); ;
            ConsoleEx.WriteLine("  -f     interpret dump file specified as formatted utilizing symbols stamp and/or name. Default: false");
            ConsoleEx.WriteLine("  -z     enables zipping of the dump file. Default: false");
            ConsoleEx.WriteLine();
            base.DisplayUsage();
        }
     

    }
}
