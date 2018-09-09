using System;
using System.Collections.Generic;
using System.Text;

namespace Beeper_Server.Config
{
    class Configuration
    {
        // configuration information
        public Int16 ConfigurationVersion;

        // server information
        public String ServerName;
        public String ServerDesc;
        public Guid ID;

        // server interaction
        public UInt16 Port;
        public Int64 MaxConnections;

        // user interaction
        public String ConnectionString;
        public Boolean ProfanityFilter;
        public String ColorHex;

        public static Configuration GetDefaultConfiguration()
        {
            Configuration DefaultConfiguration = new Configuration();

            DefaultConfiguration.ConfigurationVersion = 2;

            DefaultConfiguration.ServerName = "Beeper Server";
            DefaultConfiguration.ServerDesc = "A brand new Beeper server";
            DefaultConfiguration.ID = new Guid();

            DefaultConfiguration.Port = 40613;
            DefaultConfiguration.MaxConnections = 10;

            DefaultConfiguration.ConnectionString = "Welcome to a brand new Beeper server!";
            DefaultConfiguration.ProfanityFilter = false;
            DefaultConfiguration.ColorHex = "007cc2";

            return DefaultConfiguration;
        }

        public static Configuration RebuildBrokenConfiguration(Configuration PreviousConfiguration)
        {
            Configuration NewConfiguration = GetDefaultConfiguration();

            if (String.IsNullOrWhiteSpace(PreviousConfiguration.ServerName)) NewConfiguration.ServerName = "Beeper Server";
            if (String.IsNullOrWhiteSpace(PreviousConfiguration.ServerDesc)) NewConfiguration.ServerDesc = "A brand new Beeper server";
            if (PreviousConfiguration.ID != null) NewConfiguration.ID = new Guid();

            if (PreviousConfiguration.Port <= 0) NewConfiguration.Port = 40613;
            if (PreviousConfiguration.Port <= 0) NewConfiguration.MaxConnections = 10;

            if (String.IsNullOrWhiteSpace(PreviousConfiguration.ConnectionString)) NewConfiguration.ConnectionString = "Welcome to a brand new Beeper server!";
            if (String.IsNullOrWhiteSpace(PreviousConfiguration.ColorHex)) NewConfiguration.ColorHex = "007cc2";
            return NewConfiguration;
        }

    }
}
