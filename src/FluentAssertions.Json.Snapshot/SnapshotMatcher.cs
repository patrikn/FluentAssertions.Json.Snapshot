using System;
using System.IO;
using System.Text;
using FluentAssertions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FluentAssertions.Json.Snapshot
{
    internal class SnapshotMatcher
    {
        private readonly Snapshotter _snapshotter;

        internal SnapshotMatcher(Snapshotter snapshotter)
        {
            _snapshotter = snapshotter;
        }

        public AndConstraint<ObjectAssertions> Match(ObjectAssertions assertions, Type deserializationType,
            JsonSerializer serializer)
        {
            serializer ??= new JsonSerializer();

            var subject = assertions.Subject;
            var snapshot = _snapshotter.Snapshot(subject, serializer);

            var actualJson = ToJTokenUsingSerializer(assertions, serializer);

            actualJson.Should().BeEquivalentTo(snapshot, "snapshot doesn't match");

            var deserializedSnapshot = snapshot.ToObject(deserializationType, serializer);
            deserializedSnapshot.Should()
                .BeOfType(subject.GetType())
                .And.BeEquivalentTo(subject);

            return deserializedSnapshot.Should().BeAssignableTo(deserializationType);
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