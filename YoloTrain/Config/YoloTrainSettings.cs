using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoloTrain.Config
{
    public class YoloTrainSettings
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("recent_projects")]
        public List<string> RecentProjects { get; set; }
    }
}
