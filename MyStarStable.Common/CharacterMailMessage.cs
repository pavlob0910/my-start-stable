using System;
using Newtonsoft.Json;

namespace MyStarStable.Common
{
    public class CharacterMailMessage
    {
        public enum MailType : int
        {
            User = 0,
            UserWithAttachment,
            System,
            SystemWithAttachment,
            Web,
            WebWithAttachment
        };

        [JsonProperty("mail_id")]
        public UInt64 MailId;
        
        [JsonProperty("recipient")]
        public UInt64 RecipientCharacterId;
        
        [JsonProperty("type")]
        public MailType Type;
        
        [JsonProperty("sender")]
        public UInt64 SenderCharacterId;
        
        [JsonProperty("sender_name")]
        public string SenderCharacterName;
        
        [JsonProperty("subject")] 
        public string Subject;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("sent_date")]
        public DateTime SentDate;

        [JsonProperty("read_date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ReadDate;

        [JsonProperty("has_attachment")]
        public bool HasAttachment;
    }
}
