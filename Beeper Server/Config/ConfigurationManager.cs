using Beeper_Server.Crash;
using Beeper_Server.Server;
using BeeperCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Beeper_Server.Config
{
    class ConfigurationManager
    {
        public static string ConfigurationFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Beeper", "Server");
        public static string ConfigurationFile { get; } = Path.Combine(ConfigurationFolder, "config-main.json");
        public Configuration Configuration { get; set; } = new Configuration();
        Log l = null;

        public void InitializeConfiguration()
        {
            l = Program.GetLog();
            l.WriteLine(l.VERBOSE, "Initializing configuration...");
            l.WriteLine(l.VERBOSE, "Initializing configuration folder...");
            InitializeConfigurationFolder();
            l.WriteLine(l.VERBOSE, "Initializing configuration file...");
            InitializeConfigurationFile();
        }

        public void InitializeConfigurationFolder()
        {
            if (!Directory.Exists(ConfigurationFolder))
            {
                l.WriteLine(l.VERBOSE, "Configuration folder not detected. Initializing assuming first time run...");
                Directory.CreateDirectory(ConfigurationFolder);
                l.WriteLine(l.VERBOSE, "Configuration folder created.");
            }
            else
            {
                l.WriteLine(l.VERBOSE, "Configuration folder detected. Verifying contents...");
                VerifyConfigurationFolder();
            }
        }

        public void InitializeConfigurationFolder(bool verify)
        {
            if (!Directory.Exists(ConfigurationFolder))
            {
                l.WriteLine(l.VERBOSE, "Configuration folder not detected. Initializing assuming first time run...");
                Directory.CreateDirectory(ConfigurationFolder);
                l.WriteLine(l.VERBOSE, "Configuration folder created.");
            }
            else
            {
                l.WriteLine(l.VERBOSE, "Configuration folder detected.");
                if (verify) VerifyConfigurationFolder();
            }
        }

        private void InitializeConfigurationFile()
        {
            if (!File.Exists(ConfigurationFile))
            {
                l.WriteLine(l.VERBOSE, "Configuration file not detected. Initializing assuming first time run...");
                l.WriteLine(l.VERBOSE, "Creating configuration file...");
                InitializeConfigurationFolder(false);
                FileStream NewConfig = File.Create(ConfigurationFile);
                NewConfig.Close();
                l.WriteLine(l.VERBOSE, "Configuration file created.");
                l.WriteLine(l.VERBOSE, "Writing configuration file...");
                File.WriteAllText(ConfigurationFile, JsonConvert.SerializeObject(Configuration.GetDefaultConfiguration(), Formatting.Indented));
                l.WriteLine(l.VERBOSE, "Configuration file written.");
                l.WriteLine(l.VERBOSE, "Configuration file created.");
                ReadConfiguration();
            }
            else
            {
                ReadConfiguration();
            }
            
        }

        private void InitializeConfigurationFile(Configuration ConfigurationTemplate)
        {
            if (!File.Exists(ConfigurationFile))
            {
                l.WriteLine(l.VERBOSE, "Configuration file not detected. Initializing assuming first time run...");
                l.WriteLine(l.VERBOSE, "Creating configuration file...");
                InitializeConfigurationFolder(false);
                FileStream NewConfig = File.Create(ConfigurationFile);
                NewConfig.Close();
                l.WriteLine(l.VERBOSE, "Configuration file created.");
                l.WriteLine(l.VERBOSE, "Writing configuration file...");
                File.WriteAllText(ConfigurationFile, JsonConvert.SerializeObject(ConfigurationTemplate, Formatting.Indented));
                l.WriteLine(l.VERBOSE, "Configuration file created.");
                ReadConfiguration();
            }
            else
            {
                ReadConfiguration();
            }
        }

        private void ReadConfiguration()
        {
            l.WriteLine(l.VERBOSE, "Configuration file detected. Reading contents...");
            Configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigurationFile));
            if (Configuration == null)
            {
                l.WriteLine(l.VERBOSE, "Erred configuration file! Regenerating from default...");
                File.Delete(ConfigurationFile);
                InitializeConfigurationFile();
            }
            else
            {
                bool ConfigOK = false;
                if (Configuration.ConfigurationVersion != Configuration.GetDefaultConfiguration().ConfigurationVersion)
                {
                    l.WriteLine(l.ERROR, "Configuration version mismatch! Reinitializing configuration...");
                    File.Move(ConfigurationFile, Path.Combine(ConfigurationFile, ".bak"));
                    l.WriteLine(l.ERROR, "Old configuration is now labeled as " + Path.Combine(ConfigurationFile, ".bak") + ".");
                    InitializeConfiguration();
                    ConfigOK = true;
                }
                else if (String.IsNullOrWhiteSpace(Configuration.ServerName))
                {
                    throw new InvalidConfigurationException("Server name is invalid!");
                }
                else if(String.IsNullOrWhiteSpace(Configuration.ServerDesc))
                {
                    throw new InvalidConfigurationException("Server description is invalid!");
                }
                else if (Configuration.ID == null)
                {
                    throw new InvalidConfigurationException("Server GUID is invalid!");
                }
                else if (Configuration.Port < 1)
                {
                    throw new InvalidConfigurationException("Port must be an integer above zero (and below 65535)! (this may happen if your number of max connections is too high!)");
                }
                else if (!NetworkTools.IsPortOpen(Configuration.Port))
                {
                    throw new UnavailablePortException("Port is already being used! (NOTE: 1 to 1024 are usually reserved for the operating system.)");
                }
                else if (Configuration.MaxConnections < 1)
                {
                    throw new InvalidConfigurationException("Maximum connections must be an integer above zero (and below 2147483647)! (this may happen if your number of max connections is too high!)");
                }
                else if(String.IsNullOrWhiteSpace(Configuration.ConnectionString))
                {
                    throw new InvalidConfigurationException("Server connection string is invalid!");
                }
                else if (String.IsNullOrWhiteSpace(Configuration.ColorHex))
                {
                    throw new InvalidConfigurationException("Server theme color hex is invalid!");
                }
                else
                {
                    if (Configuration.Port < 1024) l.WriteLine(l.WARN, "The port to be occupied looks unsafe! 1 to 1024 are usually reserved for the operating system.");
                    if (Configuration.MaxConnections > short.MaxValue) l.WriteLine(l.WARN, "Your maximum connections seem to be high. Beware of CPU burnout!");
                    ConfigOK = true;
                }
                l.WriteLine(l.VERBOSE, "Configuration file read. Result: " + (ConfigOK ? "complete and usable." : "erred. Check if your values are correct."));
                if (!ConfigOK) throw new InvalidConfigurationException();
            }
        }

        public void VerifyConfigurationFolder()
        {
            l.WriteLine(l.DEBUG, "Locating configuration folder...");
            List<String> FilesToVerify = new List<String>();
            FilesToVerify.Add(ConfigurationFolder);
            FilesToVerify.Add(ConfigurationFile);
            Dictionary<String, Boolean> VerifyFiles = DetectObjects(FilesToVerify);
            foreach (Boolean b in VerifyFiles.Values)
            {
                if (!b)
                {
                    l.WriteLine(l.VERBOSE, "Vital files not detected. Resetting configurations...");
                    l.WriteLine(l.INFO, "The configuration folder is incomplete. Resetting... (All settings will be deleted!)");
                    Directory.Delete(ConfigurationFolder);
                }
            }
        }

        public Dictionary<String, Boolean> DetectObjects(List<String> paths)
        {
            Dictionary<String, Boolean> FilesExist = new Dictionary<String, Boolean>();
            int CurrentFile = 0;
            int PathCount = paths.Count;
            foreach (String path in paths)
            {
                CurrentFile++;
                if (File.Exists(path) || Directory.Exists(path))
                {
                    l.WriteLine(l.DEBUG, "Object: \"" + path + "\" exists [" + CurrentFile + "/" + PathCount + "]");
                    FilesExist.Add(path, true);
                }
                else
                {
                    l.WriteLine(l.DEBUG, "Object: \"" + path + "\" does not exist [" + CurrentFile + "/" + PathCount + "]");
                    FilesExist.Add(path, false);
                }
            }
            return FilesExist;
        }
    }
}
