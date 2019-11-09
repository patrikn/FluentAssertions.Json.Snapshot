using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FluentAssertions.Json.Snapshot
{
    public class SnapshotMatcher
    {
        private readonly Snapshotter _snapshotter;

        internal SnapshotMatcher(Snapshotter snapshotter)
        {
            _snapshotter = snapshotter;
        }

        public AndConstraint<JTokenAssertions> Match(ObjectAssertions assertions, JsonSerializer serializer, string filePath)
        {
            serializer ??= new JsonSerializer();
            
            var snapshot = _snapshotter.Snapshot(assertions.Subject, serializer);

            var actualJson = ToJTokenUsingSerializer(assertions, serializer);

            return actualJson.Should().BeEquivalentTo(snapshot, "snapshot doesn't match");
        }


        private JToken ToJTokenUsingSerializer(ObjectAssertions assertions, JsonSerializer serializer)
        {
            var buffer = new MemoryStream();
            var writer = new StreamWriter(buffer);
            serializer.Serialize(writer, assertions.Subject);
            writer.Flush();
            var bytes = buffer.ToArray();
            var input = new MemoryStream(bytes);

            var actualJson = serializer.Deserialize<JToken>(new JsonTextReader(new StreamReader(input, Encoding.UTF8)));
            return actualJson;
        }
    }
}