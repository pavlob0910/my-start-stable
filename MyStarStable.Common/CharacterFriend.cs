using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MyStarStable.Common
{
    public class CharacterFriend
    {
        [JsonProperty("id")]
        public UInt64 CharacterId;

        [JsonProperty("first_name")]
        public string FirstName;

        [JsonProperty("last_name")]
        public string LastName;
    }

    public class CharacterFriendComparer : IComparer<CharacterFriend>
    {
        public int Compare(CharacterFriend x, CharacterFriend y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    int retval = x.FirstName.CompareTo(y.FirstName);

                    if (retval != 0)
                    {
                        return retval;
                    }
                    else
                    {
                        return x.LastName.CompareTo(y.LastName);
                    }
                }
            }
        }
    }
}
