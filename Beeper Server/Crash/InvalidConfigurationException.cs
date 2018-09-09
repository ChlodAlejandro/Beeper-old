using System;
using System.Collections.Generic;
using System.Text;

namespace Beeper_Server.Crash
{
    class InvalidConfigurationException : BeeperCrash
    {
        public InvalidConfigurationException() : base() { }
        public InvalidConfigurationException(String message) : base(message) { }
    }
}
