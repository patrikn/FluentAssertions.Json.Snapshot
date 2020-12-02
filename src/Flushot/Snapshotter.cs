using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit.Sdk;

namespace Flushot
{
    public class Snapshotter
    {
        private readonly string _snapshotDirectory;
        internal string SnapshotPath { get; }

        internal Snapshotter(string path, string fileName)
        {
            var testDirectory = Path.GetDirectoryName(path) ?? throw new ArgumentException(nameof(path));
            var testClass = Path.GetFileName(path) ?? throw new ArgumentException(nameof(path));
            SnapshotPath =
                Path.GetFullPath(
                    Path.Combine(testDirectory, "_snapshots", testClass, $"{fileName}.json"));
            _snapshotDirectory = Path.GetDirectoryName(SnapshotPath)!;
        }

        public JToken? GetOrCreateSnapshot(object subject, JsonSerializer serializer)
        {
            if (!File.Exists(SnapshotPath))
            {
                CreateSnapshot(subject, serializer);
            }

            using var fileStream = File.OpenRead(SnapshotPath);
            var snapshot =
                serializer.Deserialize<JToken>(
                    new JsonTextReader(new StreamReader(fileStream, Encoding.UTF8)));

            return snapshot;
        }

        private void CreateSnapshot(object subject, JsonSerializer serializer)
        {
            var created = Directory.CreateDirectory(_snapshotDirectory);
            // I don't think this should ever happen unless someone deleted the directory from under
            // us, but I've seen weird intermittent failures so adding lots of checks seems like a good
            // idea.
            if (!created.Exists)
            {
                throw new XunitException($"Failed to create snapshot directory {_snapshotDirectory}");
            }

            using var output = File.OpenWrite(SnapshotPath);
            var writer = new StreamWriter(output, Encoding.UTF8);
            serializer.Serialize(new JsonTextWriter(writer), subject);
            writer.Flush();
            // Again this should never happen.
            if (!File.Exists(SnapshotPath))
            {
                throw new XunitException($"Failed to create snapshot file {SnapshotPath}");
            }
        }
    }
}
