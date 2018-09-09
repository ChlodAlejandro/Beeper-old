using BeeperCore.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beeper_Server.Server.Commands
{
    class Commander
    {

        private static Dictionary<String, Func<Command, Int32>> Commands = new Dictionary<String, Func<Command, Int32>>();

        public static int HandleCommand(Command Command)
        {
            if (Commands.ContainsKey(Command.Action))
            {
                try
                {
                    return (int)Commands[Command.Action].DynamicInvoke(Command);
                }
                catch
                {
                    return 2;
                }
            }
            else return 0;
        }

        public static void RegisterCommand(String CommandName, Func<Command, Int32> CommandHandler)
        {
            Commands.Add(CommandName, CommandHandler);
        }

    }

    enum CommandStatus
    {

        OK, NonExistent, ExecutionError, ProcessingError, InvalidResult, KillThread

    }
}
