using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace YoloTrain.Config
{
    public class YoloTrainConfig
    {
        [JsonProperty("batchsize")]
        public int BatchSize { get; set; }

        [JsonProperty("subdivisions")]
        public int Subdivisions { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("random")]
        public bool Random { get; set; }

        [JsonProperty("max")]
        public int Max { get; set; }

        public static void SaveFromTemplate(YoloProject project, string anchors)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (project.ObjData == null)
                throw new ArgumentException($"{nameof(project)}.{nameof(project.TrainConfig)} cannot be null", nameof(project));

            var cfg = project.TrainConfig;

            var baseDir = Path.GetDirectoryName(project.DarknetExecutableFilePath);

            if (string.IsNullOrWhiteSpace(anchors) && ObjectDataConfig.GetTrainImageCount(project) > 0)
            {
                try
                {
                    // ensure we don't read stale values
                    var anchorsFileName = Path.Combine(baseDir, "anchors.txt");
                    if (File.Exists(anchorsFileName))
                        File.Delete(anchorsFileName);

                    // calculate anchors
                    var pinfo = new ProcessStartInfo();
                    pinfo.FileName = project.DarknetExecutableFilePath;
                    pinfo.Arguments = $"detector calc_anchors {project.ObjectDataFilePath} -num_of_clusters 9 -width {cfg.Width} -heigh {cfg.Height}";
                    pinfo.WorkingDirectory = baseDir;
                    pinfo.ErrorDialog = true;
                    pinfo.UseShellExecute = false;
                    Process.Start(pinfo).WaitForExit();

                    // read from anchors.txt
                    var parts = File.ReadAllText(anchorsFileName)
                                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    // convert doubles to ints
                    var sb = new StringBuilder();
                    foreach (var part in parts)
                    {
                        if (sb.Length != 0)
                            sb.Append(",  ");

                        var coords = part.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < coords.Length; i++)
                        {
                            if (i != 0)
                                sb.Append(',');
                            var coord = double.Parse(coords[i]);
                            sb.Append((int)Math.Round(coord, MidpointRounding.AwayFromZero));
                        }
                    }

                    anchors = sb.ToString();
                }
                catch
                {
                }
            }

            if (string.IsNullOrWhiteSpace(anchors))
            {
                anchors = "10,13,  16,30,  33,23,  30,61,  62,45,  59,119,  116,90,  156,198,  373,326";
            }

            int classes = ObjectDataConfig.GetClassCount(project);
            int filters = (classes + 5) * 3;

            string yoloTemplate = File.ReadAllText(Path.Combine("darknet", "yolov3.cfg.template"));
            string yoloCfg = yoloTemplate
                .Replace("%batch%", cfg.BatchSize.ToString())
                .Replace("%subdivisions%", cfg.Subdivisions.ToString())
                .Replace("%width%", cfg.Width.ToString())
                .Replace("%height%", cfg.Height.ToString())
                .Replace("%filters%", filters.ToString())
                .Replace("%classes%", classes.ToString())
                .Replace("%anchors%", anchors)
                .Replace("%random%", cfg.Random ? "1" : "0")
                .Replace("%max%", cfg.Max.ToString());

            Directory.CreateDirectory(Path.GetDirectoryName(project.YoloConfigFilePath));
            File.WriteAllText(project.YoloConfigFilePath, yoloCfg);
        }
    }
}
