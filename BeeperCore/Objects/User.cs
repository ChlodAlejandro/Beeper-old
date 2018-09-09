using Newtonsoft.Json;
using System;

namespace BeeperCore.Objects
{
    public class User
    {

        [JsonProperty]
        public Guid ID { get; set; }
        [JsonProperty]
        public String Username { get; set; }
        [JsonProperty]
        public String DisplayName { get; set; }
        [JsonProperty]
        public String PasswordHash { get; set; }
        [JsonProperty]
        public String PasswordSalt { get; set; }
        [JsonProperty]
        public DateTime Joined { get; set; }
        [JsonProperty]
        public String ColorHex { get; set; }

        public User()
        {
            ID = new Guid();
            Username = null;
            DisplayName = null;
            PasswordHash = null;
            PasswordSalt = null;
            Joined = new DateTime(0);
            ColorHex = null;
        }

        public User(Guid _ID, String _Username, String _DisplayName, String _PasswordHash, String _PasswordSalt, DateTime _Joined)
        {
            ID = _ID;
            Username = _Username;
            DisplayName = _DisplayName;
            PasswordHash = _PasswordHash;
            PasswordSalt = _PasswordSalt;
            Joined = _Joined;
            ColorHex = null;
        }

        public User(Guid _ID, String _Username, String _DisplayName, String _PasswordHash, String _PasswordSalt, DateTime _Joined, String _ColorHex)
        {

            ID = _ID;
            Username = _Username;
            DisplayName = _DisplayName;
            PasswordHash = _PasswordHash;
            PasswordSalt = _PasswordSalt;
            Joined = _Joined;
            ColorHex = _ColorHex;
        }

        public Boolean AreContentsNull()
        {
            if (ID == null) return true;
            if (String.IsNullOrWhiteSpace(Username)) return true;
            if (String.IsNullOrWhiteSpace(DisplayName)) return true;
            if (String.IsNullOrWhiteSpace(PasswordHash)) return true;
            if (String.IsNullOrWhiteSpace(PasswordSalt)) return true;
            if (Joined == null) return true;
            return false;
        }

    }
}
