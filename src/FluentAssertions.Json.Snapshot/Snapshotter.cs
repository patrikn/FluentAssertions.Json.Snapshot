using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FluentAssertions.Json.Snapshot
{
    internal class Snapshotter
    {
        private readonly string _snapshotPath;

        public Snapshotter(string path, StackTrace stackTrace)
        {
            var fileName = SnapshotNameForTest(stackTrace);
            _snapshotPath =
                $"{Path.GetDirectoryName(path)}{Path.DirectorySeparatorChar}{fileName}.json";
        }

        private string SnapshotNameForTest(StackTrace stackTrace)
        {
            var mth = stackTrace.GetFrame(1).GetMethod();
            var declaringType = mth.DeclaringType;
            while (declaringType != null && declaringType.IsNested)
            {
                declaringType = declaringType.DeclaringType;
            }

            var declaringTypeName = ReplaceInvalidChars(declaringType?.Name ?? "__global__");
            var mthName = ReplaceInvalidChars(mth.Name);

            var fileName = $"{declaringTypeName}.{mthName}";
            return fileName;
        }

        public JToken Snapshot(object subject, JsonSerializer serializer)
        {
            if (!File.Exists(_snapshotPath))
            {
                using var output = File.OpenWrite(_snapshotPath);
                var writer = new StreamWriter(output, Encoding.UTF8);
                serializer.Serialize(new JsonTextWriter(writer),
                    subject);
                writer.Flush();
            }
            
            using var fileStream = File.OpenRead(_snapshotPath);
            var snapshot =
                serializer.Deserialize<JToken>(
                    new JsonTextReader(new StreamReader(fileStream, Encoding.UTF8)));

            return snapshot;
        }
 
        private string ReplaceInvalidChars(string declaringTypeName)
        {
            return Path.GetInvalidFileNameChars()
                .Aggregate(declaringTypeName, (name, c) => name.Replace(c, '_'));
        }   }
}