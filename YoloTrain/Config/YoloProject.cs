using Newtonsoft.Json;

namespace YoloTrain.Config
{
    public class YoloProject
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("yolo_version")]
        public int YoloVersion { get; set; }

        [JsonProperty("darknet_path")]
        public string DarknetExecutableFilePath { get; set; }

        [JsonProperty("train_yolo_cfg_path")]
        public string TrainYoloConfigFilePath { get; set; }

        [JsonProperty("test_yolo_cfg_path")]
        public string TestYoloConfigFilePath { get; set; }

        [JsonProperty("obj_data_path")]
        public string ObjectDataFilePath { get; set; }

        [JsonProperty("last_image")]
        public string LastImageFilePath { get; set; }
    }
}
