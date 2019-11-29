using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flushot
{
    internal class Snapshotter
    {
        private readonly string _snapshotPath;
        private readonly string _snapshotDirectory;

        internal Snapshotter(string path, StackTrace stackTrace)
        {
            var fileName = SnapshotNameForTest(stackTrace);
            var testDirectory = Path.GetDirectoryName(path) ?? throw new ArgumentException(nameof(path));
            _snapshotPath = Path.Combine(testDirectory, "_snapshots", $"{fileName}.json");
            _snapshotDirectory = Path.GetDirectoryName(_snapshotPath);
        }

        private string SnapshotNameForTest(StackTrace stackTrace)
        {
            var mth = stackTrace.GetFrame(1).GetMethod();
            var declaringTypePath = PathForType(mth);

            var mthName = ReplaceInvalidChars(mth.Name);

            var fileName = Path.Combine($"{declaringTypePath}", $"{mthName}");
            return fileName;
        }

        private string PathForType(MemberInfo mth)
        {
            var declaringType = mth.DeclaringType;
            var declaringTypePath = ReplaceInvalidChars(declaringType?.Name ?? "__global__");

            while (declaringType != null && declaringType.IsNested)
            {
                declaringType = declaringType.DeclaringType;
                declaringTypePath = declaringType == null ?
                    declaringTypePath
                    : Path.Combine(ReplaceInvalidChars(declaringType.Name), declaringTypePath);
            }

            return declaringTypePath;
        }

        public JToken? Snapshot(object subject, JsonSerializer serializer)
        {
            if (!File.Exists(_snapshotPath))
            {
                CreateSnapshot(subject, serializer);
            }

            using var fileStream = File.OpenRead(_snapshotPath);
            var snapshot =
                serializer.Deserialize<JToken>(
                    new JsonTextReader(new StreamReader(fileStream, Encoding.UTF8)));

            return snapshot;
        }

        private void CreateSnapshot(object subject, JsonSerializer serializer)
        {
            Directory.CreateDirectory(_snapshotDirectory);
            using var output = File.OpenWrite(_snapshotPath);
            var writer = new StreamWriter(output, Encoding.UTF8);
            serializer.Serialize(new JsonTextWriter(writer), subject);
            writer.Flush();
        }

        private string ReplaceInvalidChars(string declaringTypeName)
        {
            return Regex.Replace(declaringTypeName, "[^\\w\\d-_]", "_");
        }
    }
}