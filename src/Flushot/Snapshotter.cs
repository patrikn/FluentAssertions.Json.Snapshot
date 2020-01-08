using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Sdk;

namespace Flushot
{
    internal class Snapshotter
    {
        private readonly string _snapshotDirectory;
        internal string SnapshotPath { get; }

        internal Snapshotter(string path, string fileName)
        {
            var testDirectory = Path.GetDirectoryName(path) ?? throw new ArgumentException(nameof(path));
            SnapshotPath = Path.Combine(testDirectory, "_snapshots", $"{fileName}.json");
            _snapshotDirectory = Path.GetDirectoryName(SnapshotPath);
        }

        internal Snapshotter(string path, StackTrace stackTrace) : this(path, SnapshotNameForTest(stackTrace))
        {
        }

        private static string SnapshotNameForTest(StackTrace stackTrace)
        {
            var testFrame = (stackTrace.GetFrames() ?? throw new ArgumentNullException(nameof(stackTrace)))
                            .FirstOrDefault(frame => frame.GetMethod().GetCustomAttributes(typeof(FactAttribute)).Any());
            if (testFrame == null)
            {
                throw new XunitException("Snapshotting with implicit names only works in Xunit Fact methods");
            }

            var mth = testFrame.GetMethod();
            var declaringTypePath = PathForType(mth);

            var mthName = ReplaceInvalidChars(mth.Name);

            var fileName = Path.Combine($"{declaringTypePath}", $"{mthName}");
            return fileName;
        }

        private static string PathForType(MemberInfo mth)
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
            Directory.CreateDirectory(_snapshotDirectory);
            using var output = File.OpenWrite(SnapshotPath);
            var writer = new StreamWriter(output, Encoding.UTF8);
            serializer.Serialize(new JsonTextWriter(writer), subject);
            writer.Flush();
        }

        private static string ReplaceInvalidChars(string declaringTypeName)
        {
            return Regex.Replace(declaringTypeName, "[^\\w\\d-_]", "_");
        }
    }
}