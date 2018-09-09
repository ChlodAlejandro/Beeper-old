using BeeperCore.Objects;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace BeeperCore.Converters
{
    public class BeeperBuilder
    {
        static JsonSerializerSettings MessageFormatting = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
        };

        public static Message BuildMessage(User User, String Type, String Content, DateTime Sent)
        {
            Message Result = new Message(User, Type, Content, Sent);
            return Result;
        }

        public static Message BuildMessage(String MessageJSON)
        {
            Message Result = JsonConvert.DeserializeObject<Message>(MessageJSON, MessageFormatting);
            return Result;
        }

        public static String BuildMessageJSON(User User, String Type, String Content, DateTime Sent)
        {
            String Result = JsonConvert.SerializeObject(new Message(User, Type, Content, Sent), MessageFormatting);
            return Result;
        }

        public static String BuildMessageJSON(Message Message)
        {
            String Result = JsonConvert.SerializeObject(Message, MessageFormatting);
            return Result;
        }

        public static String BuildMessageOutput(Message Message)
        {
            String Result = "[" + Message.Sent.ToString("mm/dd/yyyy hh:mm:ss.ffff tt") + "]<" + Message.Sender.DisplayName + "> " + Message.Content;
            return Result;
        }

        public static User CleanseUserForDistribution(User serverUser)
        {
            serverUser.PasswordSalt = "redacted";
            return serverUser;
        }
    }
}
