using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BeeperCore.Objects;
using BeeperCore;
using System;
using System.Collections.Generic;
using BeeperCore.Converters;
using Beeper_Server.Crash;
using Beeper_Server.Config;
using Beeper_Server.Server.Commands;

namespace Beeper_Server.Server
{
    internal class ServerCore
    {

        private TcpListener TCPListener;
        private Thread ListenThread;
        private Log l;
        private Boolean Terminated;
        private User ServerUser;
        private readonly Configuration Configuration = Program.ProgramConfig;
        public Dictionary<TcpClient, User> Clients = new Dictionary<TcpClient, User>();

        public void Start()
        {
            ServerUser = new User(Configuration.ID, "Server", Configuration.ServerName, "server", "server", DateTime.Now, Configuration.ColorHex);
            l = Program.GetLog();
            TCPListener = new TcpListener(IPAddress.Any, Program.ProgramConfig.Port);
            ListenThread = new Thread(new ThreadStart(AwaitClients));
            l.WriteLine(l.INFO, "Starting client listener...");
            ListenThread.Start();
        }

        private void AwaitClients()
        {
            try
            {
                TCPListener.Start();
                l.WriteLine(l.INFO, "Listening for clients...");
                while (!Terminated)
                {
                    //blocks until a client has connected to the server
                    l.WriteLine(l.INFO, "Awaiting new client...");
                    TcpClient IncomingClient = TCPListener.AcceptTcpClient();

                    l.WriteLine(l.INFO, "New client connected. Preparing communication...");

                    //create a thread to handle communication
                    //with connected client
                    Thread ClientThread = new Thread(new ParameterizedThreadStart(HandleClientConnection));
                    ClientThread.Start(IncomingClient);
                    l.WriteLine(l.INFO, "Communication thread started...");
                }
                TCPListener.Stop();
                l.WriteLine(l.INFO, "Communication thread stopped.");
            }
            catch
            {
                // SERVER SHUTTING DOWN
            }
        }

        private Message GetIntroMessage()
        {
            return new Message(ServerUser, "INTRO", Configuration.ConnectionString, DateTime.Now);
        }

        private void HandleClientConnection(object client)
        {
            TcpClient TCPClient = (TcpClient)client;
            NetworkStream ClientDataStream = TCPClient.GetStream();
            UTF8Encoding encoder = new UTF8Encoding();
            l.WriteLine(l.VERBOSE, "Client thread started for " + TCPClient.GetHashCode());
            byte[] message = new byte[UInt16.MaxValue * 16];

            l.WriteLine(l.INFO, "A client is connecting...");
            l.WriteLine(l.VERBOSE, "Awaiting introduction message of " + TCPClient.GetHashCode());
            Message IntroductionMessage = null;
            try { 
                byte[] Introduction_Bytes = new byte[ushort.MaxValue];
                int Introduction_BytesRead = ClientDataStream.Read(Introduction_Bytes, 0, ushort.MaxValue * 16);
                IntroductionMessage = BeeperBuilder.BuildMessage(encoder.GetString(Introduction_Bytes, 0, Introduction_BytesRead));
            } catch (Exception e)
            {
                l.WriteLine(l.ERROR, "An exception occured! Oh no!");
                throw new BeeperCrash(e.Message);
            }
            if (IntroductionMessage == null )
            {
                l.WriteLine(l.INFO, "Connection failed. Reason: User error");
                l.WriteLine(l.VERBOSE, "Introduction message invalid. Disconnecting " + TCPClient.GetHashCode() + "...");
                ClientDataStream.Close();
                TCPClient.Close();
                return;
            }
            else
            {
                if (IntroductionMessage.AreContentsNull())
                {
                    l.WriteLine(l.INFO, "Connection failed. Reason: User error");
                    l.WriteLine(l.VERBOSE, "Introduction message null. Disconnecting " + TCPClient.GetHashCode() + "...");
                    ClientDataStream.Close();
                    TCPClient.Close();
                    return;
                }
                else if (IntroductionMessage.Type != "INTRO")
                {
                    l.WriteLine(l.INFO, "Connection failed. Reason: User error");
                    l.WriteLine(l.VERBOSE, "Did not send introduction for first contact. Disconnecting " + TCPClient.GetHashCode() + "...");
                    ClientDataStream.Close();
                    TCPClient.Close();
                    return;
                }
                else
                {
                    l.WriteLine(l.VERBOSE, "User introduction is good. Connecting to " + TCPClient.GetHashCode());
                    Clients.Add(TCPClient, IntroductionMessage.Sender);
                    l.WriteLine(l.VERBOSE, "User connected: " + TCPClient.GetHashCode() + ". Switching from hashcode identification to username identification.");
                    l.WriteLine(l.INFO, "User " + IntroductionMessage.Sender.DisplayName + " (" + IntroductionMessage.Sender.Username + ") connected");
                    l.WriteLine(l.VERBOSE, IntroductionMessage.Sender.DisplayName + ". Sending verification response.");
                    byte[] buffer = encoder.GetBytes(BeeperBuilder.BuildMessageJSON(GetIntroMessage()));
                    ClientDataStream.Write(buffer, 0, buffer.Length);
                    l.WriteLine(l.VERBOSE, IntroductionMessage.Sender.DisplayName + ": Response sent.");
                    DistributeMessage(new Message(ServerUser, "MESSAGE_STANDARD", IntroductionMessage.Sender.DisplayName + " connected."));
                }
            }

            int CurrentMessage_BytesRead;
            while (true)
            {
                CurrentMessage_BytesRead = 0;

                try
                {
                    // READ INPUT
                    CurrentMessage_BytesRead = ClientDataStream.Read(message, 0, ushort.MaxValue * 16);
                }
                catch
                {
                    // EXCEPTION
                    break;
                }

                if (CurrentMessage_BytesRead == 0)
                {
                    DisconnectUser(TCPClient);
                }
                // RECIEVED
                String ReceivedJSON = "";
                try
                {
                    ReceivedJSON = encoder.GetString(message, 0, CurrentMessage_BytesRead);
                    l.WriteLine(l.DEBUG, ReceivedJSON);
                    Message Received = BeeperBuilder.BuildMessage(ReceivedJSON);
                    if (!Received.AreContentsNull())
                    {
                        if (Received.Type == "MESSAGE_STANDARD")
                        {
                            DistributeMessage(Received);
                            l.WriteLine(l.INFO, BeeperBuilder.BuildMessageOutput(Received));
                        }
                        else if (Received.Type == "DISCONNECT")
                        {
                            DisconnectUser(TCPClient);
                            return;
                        }
                        else if (Received.Type == "COMMAND")
                        {
                            int CommandResult = Commander.HandleCommand(new Command(Received.Sender, ((Received.Content.Split(' ').Length == 1) ? Received.Content : Received.Content.Split(' ')[0]), ((Received.Content.Split(' ').Length == 1) ? new String[0] : Received.Content.Split(' ')), Received.Sent, TCPClient));
                            switch (CommandResult)
                            {
                                case (int) CommandStatus.NonExistent:
                                    SendMessage(ClientDataStream, "Command is unknown or inaccessible.");
                                    break;
                                case (int) CommandStatus.ExecutionError:
                                    SendMessage(ClientDataStream, "The command handler had an issue executing the command.");
                                    break;
                                case (int)CommandStatus.ProcessingError:
                                    break;
                                case (int)CommandStatus.InvalidResult:
                                    break;
                                case (int) CommandStatus.KillThread:
                                    return;
                                default:
                                    // OK.
                                    break;
                            }
                        }
                    }
                } catch (Exception e)
                {
                    l.WriteLine(l.ERROR, "Hold your horses! The JSON is invalid! This message can't be displayed!");
                    l.WriteLine(l.ERROR, ReceivedJSON);
                    SendMessage(ClientDataStream, new Message(ServerUser, "ERROR", "Your message cannot be displayed! " + e.Message, DateTime.Now));
                }
            }

            TCPClient.Close();
        }

        public void DistributeMessage(Message Message)
        {
            foreach (TcpClient ReceivingClient in Clients.Keys)
            {
                if (!Terminated)
                {
                    try
                    {
                        NetworkStream ReceivingStream = ReceivingClient.GetStream();
                        UTF8Encoding encoder = new UTF8Encoding();
                        byte[] buffer = encoder.GetBytes(BeeperBuilder.BuildMessageJSON(Message));
                        ReceivingStream.Write(buffer, 0, buffer.Length);
                        ReceivingStream.Flush();
                    }
                    catch { }
                }
            }
        }

        public void DisconnectUser(TcpClient TCPClient)
        {
            try
            {
                DistributeMessage("User " + Clients[TCPClient].DisplayName + " disconnected.");
                Clients.Remove(TCPClient);
                TCPClient.GetStream().Close();
                TCPClient.Close();
            } catch
            {
                l.WriteLine(l.ERROR, "User " + Clients[TCPClient].DisplayName + " cannot be disconnected safely. Disposing...");
                Clients.Remove(TCPClient);
                TCPClient.Dispose();
            }
        }

        public void DistributeMessage(String Message)
        {
            foreach (TcpClient ReceivingClient in Clients.Keys)
            {
                if (!Terminated)
                {
                    try {
                        NetworkStream ReceivingStream = ReceivingClient.GetStream();
                        UTF8Encoding encoder = new UTF8Encoding();
                        byte[] buffer = encoder.GetBytes(BeeperBuilder.BuildMessageJSON(new Message(BeeperBuilder.CleanseUserForDistribution(ServerUser), "MESSAGE_STANDARD", Message, DateTime.Now)));
                        ReceivingStream.Write(buffer, 0, buffer.Length);
                        ReceivingStream.Flush();
                    }
                    catch { }
            }
            }
        }

        public void SendMessage(NetworkStream ReceivingStream, Message Message)
        {
            try { 
                UTF8Encoding encoder = new UTF8Encoding();
                byte[] buffer = encoder.GetBytes(BeeperBuilder.BuildMessageJSON(Message));
                ReceivingStream.Write(buffer, 0, buffer.Length);
                ReceivingStream.Flush();
            }
            catch {
                l.WriteLine(l.ERROR, "Failed to send message.");
            }
        }

        public void SendMessage(NetworkStream ReceivingStream, String Message)
        {
            try
            {
                UTF8Encoding encoder = new UTF8Encoding();
                byte[] buffer = encoder.GetBytes(BeeperBuilder.BuildMessageJSON(new Message(ServerUser, "MESSAGE_STANDARD", Message, DateTime.Now)));
                ReceivingStream.Write(buffer, 0, buffer.Length);
                ReceivingStream.Flush();
            }
            catch {
                l.WriteLine(l.ERROR, "Failed to send message.");
            }
}

    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  