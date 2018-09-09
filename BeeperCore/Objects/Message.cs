using Newtonsoft.Json;
using System;

namespace BeeperCore.Objects
{
    public class Message
    {

        [JsonProperty]
        public User Sender { get; set; }
        [JsonProperty]
        public String Type { get; set; }
        [JsonProperty]
        public String Content { get; set; }
        [JsonProperty]
        public DateTime Sent { get; set; }

        public Message()
        {
            Sender = null;
            Type = null;
            Content = null;
            Sent = new DateTime(0);
        }

        public Message(User sender, string type, string content, DateTime sent)
        {
            Sender = sender;
            Type = type;
            Content = content;
            Sent = sent;
        }

        public Message(User sender, string type, string content)
        {
            Sender = sender;
            Type = type;
            Content = content;
            Sent = DateTime.Now;
        }

        public Boolean AreContentsNull()
        {
            if (Sender == null) return true;
            if (Sender.AreContentsNull()) return true;
            if (String.IsNullOrWhiteSpace(Type)) return true;
            if (String.IsNullOrWhiteSpace(Content)) return true;
            if (Sent == null) return true;
            return false;
        }

    }
}
