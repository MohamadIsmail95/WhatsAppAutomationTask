using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsAppServices.Models
{
    public class UnreadMessage
    {
        [JsonProperty("messages")]
        public List<MessageModel> Messages { get; set; }
    }
}
