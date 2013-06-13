using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace TestControlTool.Web.Models
{
    [JsonObject]
    public class SaveTaskModel
    {
        [JsonProperty("model")]
        public TaskModel Model { get; set; }

        [JsonProperty("tasks")]
        public Dictionary<string, string> Tasks { get; set; }

        [JsonProperty("tests")]
        public Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>> Tests { get; set; }
    }
}