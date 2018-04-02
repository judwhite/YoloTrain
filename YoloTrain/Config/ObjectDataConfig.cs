using System.Collections.Generic;

namespace YoloTrain.Config
{
    public class ObjectDataConfig
    {
        public int Classes { get; set; }
        public string Train { get; set; }
        public string Valid { get; set; }
        public string Names { get; set; }
        public string Backup { get; set; }
        public List<KeyValuePair<string, string>> UnknownKeyValues { get; set; }
    }
}
