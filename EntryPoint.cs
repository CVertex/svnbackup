using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleFx;
namespace SvnBackup {
    public static class EntryPoint {

        public static int Main(string[] args) {
            try
            {
                return CommandLine.Run<Program>(args);
            } catch (Exception ex)
            {
                Console.WriteLine("Exception in SvnBackup:");
                Console.WriteLine(ex.ToString());
                return -1;

            } finally
            {
#if DEBUG
                Console.WriteLine("Press [ENTER] to exit");
                Console.ReadLine();
#endif                
            }
        }
    }
}
