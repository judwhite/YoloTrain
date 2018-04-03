using Newtonsoft.Json;

namespace YoloTrain.Config
{
    public class YoloProject
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("yolo_version")]
        public string YoloVersion { get; set; }

        [JsonProperty("darknet_path")]
        public string DarknetExecutableFilePath { get; set; }

        [JsonProperty("yolo_config_path")]
        public string YoloConfigFilePath { get; set; }

        [JsonProperty("images_directory")]
        public string ImagesDirectory { get; set; }

        [JsonProperty("obj_data_path")]
        public string ObjectDataFilePath { get; set; }

        [JsonProperty("last_image")]
        public string LastImageFilePath { get; set; }
    }
}
