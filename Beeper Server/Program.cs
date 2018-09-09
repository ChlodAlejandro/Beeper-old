using Beeper_Server.Config;
using Beeper_Server.Crash;
using Beeper_Server.Server;
using BeeperCore;
using System;
using System.Collections.Generic;
using System.IO;

namespace Beeper_Server
{
    class Program
    {
        private static Log l = new Log();
        public static Configuration ProgramConfig = new Configuration();
        public static ServerCore BeeperServerCore = new ServerCore();

        public static Log GetLog()
        {
            return l;
        }

        static void Main(string[] args)
        {
            try
            {

                HandleProgramArguments(args);
                Console.WriteLine("PROGRAM START - " + l.GetDateTimeNowString());
                Console.Title = "Beeper Server";
                l.WriteLine(l.DEBUG, "Debugger initialized.");
                l.WriteLine(l.VERBOSE, "Logger initialized.");

                l.WriteLine(l.INFO, "Starting up...");
                l.WriteLine(l.VERBOSE, "Reading configuration values...");
                ConfigurationManager cfg = new ConfigurationManager();
                cfg.InitializeConfiguration();
                l.WriteLine(l.DEBUG, "START CONFIGURATION FILE CONTENT OUTPUT:");
                if (cfg.Configuration == null)
                {
                    l.WriteLine(l.DEBUG, "ERROR: null");
                }
                else
                {
                    ProgramConfig = cfg.Configuration;
                    l.WriteLine(l.DEBUG, "ConfigurationVersion: " + cfg.Configuration.ConfigurationVersion);
                    l.WriteLine(l.DEBUG, "ServerName: " + cfg.Configuration.ServerName);
                    l.WriteLine(l.DEBUG, "ServerDesc: " + cfg.Configuration.ServerDesc);
                    l.WriteLine(l.DEBUG, "Port: " + cfg.Configuration.Port);
                    l.WriteLine(l.DEBUG, "MaxConnections: " + cfg.Configuration.MaxConnections);
                    l.WriteLine(l.DEBUG, "ConnectionString: " + cfg.Configuration.ConnectionString);
                    l.WriteLine(l.DEBUG, "ProfanityFilter: " + cfg.Configuration.ProfanityFilter);
                }
                l.WriteLine(l.DEBUG, "END CONFIGURATION FILE CONTENT OUTPUT");

                l.WriteLine(l.INFO, "Configuration successfully read.");
                Console.Title = ProgramConfig.ServerName + " | Beeper Server";

                l.WriteLine(l.INFO, "Registering server commands...");
                ServerCommands.RegisterCommands();
                l.WriteLine(l.INFO, "Server commands registered.");

                l.WriteLine(l.INFO, "Starting up TCP server...");
                BeeperServerCore.Start();
                while (true)
                {
                    // ACCEPT COMMANDS
                    Console.Write("> ");
                    if (Console.ReadLine() == "exit")
                    {
                        break;
                    }
                }
                l.WriteLine(l.INFO, "Closing TCP server...");
            }
            catch (BeeperCrash BeeperCrash)
            {
                l.WriteLine(l.ERROR, "Beeper Crash thrown! Dumping stacktrace...");
                l.WriteLine(l.ERROR, "Exception occured on " + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.ffff") + Environment.NewLine + "Message: " + BeeperCrash.Message + Environment.NewLine + "Specific Message: " + BeeperCrash.SpecificMessage + Environment.NewLine + BeeperCrash.ToString() + Environment.NewLine + BeeperCrash.StackTrace);
                Console.Write("Holding due to crash. Press any key to exit...");
                Console.ReadKey(false);
                Console.WriteLine("PROGRAM END - " + l.GetDateTimeNowString());
            }
            catch (Exception BeeperCrash)
            {
                l.WriteLine(l.ERROR, "Unknown exception thrown! Dumping stacktrace...");
                l.WriteLine(l.ERROR, "Exception occured on " + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.ffff") + Environment.NewLine + "Message: " + BeeperCrash.Message + Environment.NewLine + BeeperCrash.ToString() + Environment.NewLine + BeeperCrash.StackTrace);
                DumpCrash(BeeperCrash);
            } finally
            {
                Console.WriteLine("PROGRAM END - " + l.GetDateTimeNowString());
                Console.ReadKey(false);
            }
            return;
        }

        private static void DumpCrash(Exception BeeperCrash)
        {
            string CrashFolder = Path.Combine(ConfigurationManager.ConfigurationFolder, "Crashes");
            string CrashDump = Path.Combine(CrashFolder, "crash-" + DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.ffff") + ".json");
            if (Directory.Exists(ConfigurationManager.ConfigurationFolder))
            {
                l.WriteLine(l.INFO, "Configuration directory valid. Writing crash file to \"/Crashes/\"");
                if (!Directory.Exists(CrashFolder))
                {
                    Directory.CreateDirectory(CrashFolder);
                }
            }
            else
            {
                l.WriteLine(l.WARN, "Configuration directory invalid. Writing crash file to desktop...");
            }
            Boolean CrashExists;
            do
            {
                if (File.Exists(CrashDump))
                {
                    CrashExists = true;
                    CrashDump = Path.Combine(CrashFolder, "crash-" + DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss.ffff") + ".json");
                }
                else
                {
                    CrashExists = false;
                    FileStream NewConfig = File.Create(CrashDump);
                    NewConfig.Close();
                }
            } while (CrashExists);
            l.WriteLine(l.INFO, "Writing crash dump...");
            File.WriteAllText(CrashDump, "Exception occured on " + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.ffff") + Environment.NewLine + "Message: " + BeeperCrash.Message + Environment.NewLine + BeeperCrash.ToString() + Environment.NewLine + BeeperCrash.StackTrace);
            l.WriteLine(l.INFO, "Crash dump written.");
        }

        private static void HandleProgramArguments(string[] args)
        {
            List<String> arguments = new List<String>();
            foreach (string arg in args)
            {
                arguments.Add(arg);
            }

            if (arguments.Contains("-v") || arguments.Contains("--verbose"))
            {
                l.VERBOSEON = true;
            }

            if (arguments.Contains("-d") || arguments.Contains("--debug"))
            {
                l.DEBUGON = true;
                l.VERBOSEON = true;
            }
        }
    }
}
