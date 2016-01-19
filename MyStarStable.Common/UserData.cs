using System;
using Newtonsoft.Json;

namespace MyStarStable.Common
{
    public class UserData
    {
        [JsonProperty("character")]
        public CharacterData Character;
    }

    public class CharacterData
    {
        [JsonProperty("id")]
        public UInt64 Id;

        [JsonProperty("first_name")]
        public string FirstName;

        [JsonProperty("last_name")]
        public string LastName;

        [JsonProperty("club")]
        public ClubData Club;

        [JsonProperty("server")]
        public ServerData Server;

        public bool HasClub()
        {
            return (Club != null);
        }

        public bool CanCreateClubEvents()
        {
            return (HasClub() && (Club.Role >= ClubData.ClubRole.Leader));
        }

        public int? GetClubId()
        {
            return (HasClub() ? Club.Id : (int?)null);
        }
    }

    public class ServerData
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;
    }

    public class ClubData
    {
        public enum ClubRole : int
        {
            Role1 = 0,
            Role2 = 1,
            Role3 = 2,
            Role4 = 3,
            Role5 = 4,
            Role6 = 5,
            Role7 = 6,
            Leader = 7,
            Owner = 8,
        }

        [JsonProperty("id")]
        public int Id;

        [JsonProperty("role")]
        public ClubRole Role;

        [JsonProperty("first_name")]
        public string FirstName;

        [JsonProperty("last_name")]
        public string LastName;
    }
}
