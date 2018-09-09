using Beeper_Server.Config;
using BeeperCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Beeper_Server.Crash
{
    class BeeperCrash : Exception
    {

        static string CrashFolder = Path.Combine(ConfigurationManager.ConfigurationFolder, "Crashes");
        public String SpecificMessage { get; set; }
        Log l = Program.GetLog();

        public BeeperCrash()
        {
            l.ForceConsoleColor(ConsoleColor.Red);
            SpecificMessage = "N/A";
            if (Directory.Exists(ConfigurationManager.ConfigurationFolder))
            {
                l.WriteLine(l.DEBUG, "Configuration directory valid. Writing crash file to \"/Crashes/\"");
                if (Directory.Exists(CrashFolder))
                {
                    WriteDumpFile(SpecificMessage);
                }
                else
                {
                    Directory.CreateDirectory(CrashFolder);
                    WriteDumpFile(SpecificMessage);
                }
            }
            else
            {
                l.WriteLine(l.WARN, "Configuration directory invalid. Writing crash file to desktop...");
            }
        }

        public BeeperCrash(String message)
        {
            l.WriteLine(l.ERROR, message);
            SpecificMessage = message;
            if (Directory.Exists(ConfigurationManager.ConfigurationFolder))
            {
                l.WriteLine(l.DEBUG, "Configuration directory valid. Writing crash file to \"/Crashes/\"");
                if (Directory.Exists(CrashFolder))
                {
                    WriteDumpFile(message);
                }
                else
                {
                    Directory.CreateDirectory(CrashFolder);
                    WriteDumpFile(message);
                }
            } else
            {
                l.WriteLine(l.DEBUG, "Configuration directory invalid.");
            }
        }

        public void WriteDumpFile(String message)
        {
            string CrashDump = Path.Combine(CrashFolder, "crash-" + DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.ffff") + ".json");

            Boolean CrashExists;
            do
            {
                if (File.Exists(CrashDump))
                {
                    CrashExists = true;
                    CrashDump = Path.Combine(CrashFolder, "crash-" + DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.ffff") + ".json");
                } else {
                    CrashExists = false;
                    FileStream NewConfig = File.Create(CrashDump);
                    NewConfig.Close();
                }
            } while (CrashExists);
            l.WriteLine(l.DEBUG, "Writing crash dump...");
            File.WriteAllText(CrashDump, "Exception occured on " + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.ffff") + Environment.NewLine + "Message: " + Message + Environment.NewLine + "Specific Message: " + message + Environment.NewLine + ToString() + Environment.NewLine + StackTrace);
            l.WriteLine(l.ERROR, "Crash dump written.");
        }

    }
}
