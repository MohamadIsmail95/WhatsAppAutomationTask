using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsAppServices.Models
{
    public class MessageModel
    {
        [JsonProperty("message")]
        public string Body { get; set; }
        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }
        [JsonProperty("contact")]
        public ContactModel Contact { get; set; }
    }
}
