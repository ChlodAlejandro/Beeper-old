using System;
using System.Net.NetworkInformation;

namespace Beeper_Server.Server
{
    class NetworkTools
    {

        public static Boolean IsPortOpen(UInt16 Port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == Port)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
