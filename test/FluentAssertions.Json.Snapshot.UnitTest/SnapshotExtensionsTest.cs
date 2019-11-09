using System;
using System.IO;
using Newtonsoft.Json;
using Xunit;
using Xunit.Sdk;

namespace FluentAssertions.Json.Snapshot.UnitTest
{
    public class SnapshotExtensionsTest
    {
        [Fact]
        public void Should_match_matching_snapshot()
        {
            new Test("hej").Should().MatchSnapshot();
        }

        [Fact]
        public void Should_not_match_changed_snapshot()
        {
            Action act = () => new Test("hej").Should().MatchSnapshot();

            act.Should().Throw<XunitException>();
        }

        [Fact]
        public void Should_create_snapshot_if_missing()
        {
            var snapshotFilePath = Path.Combine("..", "..", "..",
                $"{nameof(SnapshotExtensionsTest)}.{nameof(Should_create_snapshot_if_missing)}.json");
            // Delete first to make sure it isn't left over from aborted run
            File.Delete(snapshotFilePath);
            try
            {
                new Test("property").Should().MatchSnapshot();
                File.Exists(snapshotFilePath).Should().BeTrue();
            }
            finally
            {
                File.Delete(snapshotFilePath);
            }
        }

        [Fact]
        public void Should_not_match_extra_fields()
        {
            Action act = () => new Test("propertyValue").Should().MatchSnapshot();

            act.Should().Throw<XunitException>();
        }

        [Fact]
        public void Should_match_existing_snapshot_with_custom_serializer()
        {
            var customSerializer = new JsonSerializer();
            
            customSerializer.Converters.Add(new CustomTestConverter());
            
            new Test("withCustomSerializer").Should().MatchSnapshot(customSerializer);
        }

        [Fact]
        public void Should_generate_snapshot_with_custom_serializer()
        {
            var customSerializer = new JsonSerializer();
            
            customSerializer.Converters.Add(new CustomTestConverter());
            
            new Test("withCustomSerializer").Should().MatchSnapshot(customSerializer);
        }

        public class Test
        {
            public Test(string property)
            {
                Property = property;
            }

            public string Property { get; }
        }
        
        public class CustomTestConverter : JsonConverter<Test>
        {
            public override void WriteJson(JsonWriter writer, Test value, JsonSerializer serializer)
            {
                writer.WriteValue(value.Property);
            }

            public override Test ReadJson(JsonReader reader, Type objectType, Test existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return new Test(reader.ReadAsString());
            }
        }

    }
}