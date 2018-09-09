using System;
using System.Collections.Generic;
using System.Text;

namespace Beeper_Server.Crash
{
    class UnavailablePortException : BeeperCrash
    {
        public UnavailablePortException() : base() { }
        public UnavailablePortException(String message) : base(message) { }
    }
}
