using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace YoloTrain.Config
{
    public class ObjectDataConfig
    {
        [JsonProperty("train_file")]
        public string Train { get; set; }

        [JsonProperty("valid_file")]
        public string Valid { get; set; }

        [JsonProperty("names_file")]
        public string Names { get; set; }

        [JsonProperty("backup_directory")]
        public string Backup { get; set; }

        public static void SaveFromTemplate(YoloProject project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (project.ObjData == null)
                throw new ArgumentException($"{nameof(project)}.{nameof(project.ObjData)} cannot be null", nameof(project));

            var objData = project.ObjData;

            int classes = GetClassCount(project);

            string objDataTemplate = File.ReadAllText(Path.Combine("darknet", "obj.data.template"));
            string objDataText = objDataTemplate
                .Replace("%classes%", classes.ToString())
                .Replace("%train%", objData.Train)
                .Replace("%valid%", objData.Valid)
                .Replace("%names%", objData.Names)
                .Replace("%backup%", objData.Backup);

            var baseDir = Path.GetDirectoryName(project.DarknetExecutableFilePath);

            // make sure backup folder exists
            var backupFolder = Path.Combine(baseDir, objData.Backup);
            Directory.CreateDirectory(backupFolder);

            // update train/valid files
            var imgsDirectory = Path.Combine(baseDir, project.ImagesDirectory);
            Directory.CreateDirectory(imgsDirectory);
            var txtFiles = Directory.GetFiles(imgsDirectory, "*.txt", SearchOption.AllDirectories);
            var sb = new StringBuilder();
            foreach (var txtFile in txtFiles)
            {
                var jpgFile = Path.Combine(Path.GetDirectoryName(txtFile), Path.GetFileNameWithoutExtension(txtFile)) + ".jpg";
                if (!File.Exists(jpgFile))
                    continue;

                sb.AppendLine(jpgFile.Substring(baseDir.Length + 1).Replace('\\', '/'));
            }
            File.WriteAllText(Path.Combine(baseDir, objData.Train), sb.ToString());
            // TODO (judwhite): separate validation file
            File.WriteAllText(Path.Combine(baseDir, objData.Valid), sb.ToString());

            File.WriteAllText(project.ObjectDataFilePath, objDataText);
        }

        public static int GetTrainImageCount(YoloProject project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (project.ObjData == null)
                throw new ArgumentException($"{nameof(project)}.{nameof(project.ObjData)} cannot be null", nameof(project));

            var objData = project.ObjData;

            var baseDir = Path.GetDirectoryName(project.DarknetExecutableFilePath);
            var trainFileName = Path.Combine(baseDir, objData.Train);
            if (!File.Exists(trainFileName))
                return 0;

            return File.ReadAllLines(trainFileName).Length;
        }

        public static int GetClassCount(YoloProject project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (project.ObjData == null)
                throw new ArgumentException($"{nameof(project)}.{nameof(project.ObjData)} cannot be null", nameof(project));

            var objData = project.ObjData;

            var baseDir = Path.GetDirectoryName(project.DarknetExecutableFilePath);
            var namesFileName = Path.Combine(baseDir, objData.Names);

            int classes = 0;
            if (File.Exists(namesFileName))
            {
                classes = File.ReadAllLines(namesFileName).Length;
            }

            return classes;
        }
    }
}
