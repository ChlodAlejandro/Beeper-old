using Beeper_Server.Server.Commands;
using BeeperCore.Objects;

namespace Beeper_Server
{
    public class ServerCommands
    {

        public static void RegisterCommands()
        {
            Commander.RegisterCommand("disconnect", 
                delegate (Command Command)
                {
                    Program.BeeperServerCore.DisconnectUser(Command.InvokerClient);
                    return 0;
                }
            );
        }
        
    }
}