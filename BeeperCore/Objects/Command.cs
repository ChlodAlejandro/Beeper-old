using System;
using System.Net.Sockets;
using System.Threading;

namespace BeeperCore.Objects
{
    public class Command
    {
        
        public User Invoker { get; set; }
        public String Action { get; set; }
        public String[] Arguments { get; set; }
        public DateTime Sent { get; set; }
        public TcpClient InvokerClient { get; set; }

        public Command(User _Invoker, String _Action, String[] _Arguments, DateTime _Sent, TcpClient _InvokerClient)
        {
            Invoker = _Invoker;
            Action = _Action;
            Arguments = _Arguments;
            Sent = _Sent;
            InvokerClient = _InvokerClient;
        }
    }
}
