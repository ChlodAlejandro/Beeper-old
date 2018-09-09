using BeeperCore.Converters;
using BeeperCore.Objects;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BeeperClient_Console
{
    class Program
    {
        static User User = new User();
        Boolean Terminated = false;

        static void Main(string[] args)
        {
            InitializeUser();
            
            while (true)
            {
                // ACCEPT COMMANDS
                Console.Write("> ");
                String Line = Console.ReadLine();
                if (Line == "exit")
                {
                    break;
                }
                else if (Line == "connect")
                {
                    Connect();
                    Console.WriteLine("Connection trial sent.");
                } else if (Line == "jsontest")
                {
                    Console.WriteLine("Creating intro message...");
                    Message IntroductoryMessage = new Message(User, "INTRO", "Hello, Beeper World!", DateTime.Now);
                    Console.WriteLine("Message JSON: " + BeeperBuilder.BuildMessageJSON(IntroductoryMessage));
                }
                Console.WriteLine();
            }
        }

        public static void InitializeUser()
        {
            User.ID = Guid.NewGuid();
            Console.WriteLine("GUID > " + User.ID);
            Console.WriteLine("Username > ");
            User.Username = Console.ReadLine();
            Console.Write("Display Name > ");
            User.DisplayName = Console.ReadLine();
            // String -> SHA256
            // ABCDEF
            User.PasswordHash = "e9a92a2ed0d53732ac13b031a27b071814231c8633c9f41844ccba884d482b16";
            User.PasswordSalt = "G";
            User.Joined = DateTime.Now;
            /** SERVER ONLY **/ User.ColorHex = null;
        }

        private static Boolean Disconnected;

        public static void Connect()
        {
            Console.WriteLine("TCP Client started");
            TcpClient client = new TcpClient();

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 40613);
            Console.WriteLine("Server endpoint set.");

            Console.WriteLine("Connecting to endpoint...");
            try
            {
                client.Connect(serverEndPoint);
            } catch (Exception e)
            {
                Console.WriteLine("Cannot connect to endpoint: " + e.Message);
            }
            Console.WriteLine("Connected to endpoint.");

            Console.WriteLine("Looking for stream...");
            NetworkStream clientStream = client.GetStream();
            Console.WriteLine("Stream found.");

            Console.WriteLine("Building introductory message...");
            Message IntroductoryMessage = new Message();
            IntroductoryMessage.Sender = User;
            IntroductoryMessage.Sent = DateTime.Now;
            IntroductoryMessage.Type = "INTRO";
            IntroductoryMessage.Content = "Hello, Beeper Server!";
            Console.WriteLine("Introductory message built.");

            Console.WriteLine("Initializing encoder...");
            ASCIIEncoding encoder = new ASCIIEncoding();
            Console.WriteLine("Encoder initialized.");
            Console.WriteLine("Serializing Message.");
            Console.WriteLine("Encoding message...");
            byte[] buffer = encoder.GetBytes(BeeperBuilder.BuildMessageJSON(IntroductoryMessage));
            Console.WriteLine("Message encoded.");

            Console.WriteLine("Writing to stream...");
            clientStream.Write(buffer, 0, buffer.Length);
            Console.WriteLine("Written to buffer...");
            clientStream.Flush();
            Console.WriteLine("Written to stream.");
            Console.WriteLine("Starting stream reading...");

            Thread ReceivingThread = new Thread(new ParameterizedThreadStart(HandleServerConnection));
            ReceivingThread.Start(client);
            while (!Disconnected)
            {
                Console.Write("Server > ");
                String Data = Console.ReadLine();
                String MessageType = "MESSAGE_STANDARD";
                if (Data.Substring(0,2) == "::")
                {
                    MessageType = "COMMAND";
                    Data = Data.Remove(0, 2);
                }
                Console.WriteLine(BeeperBuilder.BuildMessageJSON(new Message(User, MessageType, Data, DateTime.Now)));
                byte[] sendbuffer = encoder.GetBytes(BeeperBuilder.BuildMessageJSON(new Message(User, MessageType, Data, DateTime.Now)));
                clientStream.Write(sendbuffer, 0, sendbuffer.Length);
                clientStream.Flush();
            }
        }

        public static void HandleServerConnection(object client)
        {
            TcpClient TCPClient = (TcpClient) client; 
            NetworkStream ClientDataStream = TCPClient.GetStream();
            UTF8Encoding encoder = new UTF8Encoding();
            int CurrentMessage_BytesRead;
            byte[] message = new byte[UInt16.MaxValue * 16];
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
                    Console.WriteLine("Connection closed.");
                    ClientDataStream.Close();
                    break;
                }
                // RECIEVED
                Message Received = BeeperBuilder.BuildMessage(encoder.GetString(message, 0, CurrentMessage_BytesRead));
                Console.WriteLine(BeeperBuilder.BuildMessageOutput(Received));
            }

            TCPClient.Close();
            Disconnected = true;
        }
    }
}
