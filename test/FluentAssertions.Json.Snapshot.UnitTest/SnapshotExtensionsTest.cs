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
            var snapshotFilePath = Path.Combine("..", "..", "..", "_snapshots",
                $"{nameof(SnapshotExtensionsTest)}", $"{nameof(Should_create_snapshot_if_missing)}.json");
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

        [Fact]
        public void Should_deserialize_snapshot()
        {
            Action act = () => new NotDeserializableTest(new[] {123}).Should().MatchSnapshot();

            act.Should().Throw<XunitException>();
        }

        [Fact]
        public void Should_return_generic_typed_constraint()
        {
            new Test("test").Should().MatchSnapshot<Test>()
                .Subject.Property.Should().Be("test");
        }

        [Fact]
        public void Should_deserialize_to_interface()
        {
            new TestWithInterface("test").Should().MatchSnapshot<ITest>()
                .Subject.Property.Should().Be("test");
        }

        public class Test
        {
            public Test(string property)
            {
                Property = property;
            }

            public string Property { get; }
        }

        [JsonConverter(typeof(InterfaceConverter))]
        public interface ITest
        {
            string Property { get; }
        }

        [JsonConverter(typeof(NotDeserializableConverter))]
        public class TestWithInterface : ITest
        {
            public TestWithInterface(string property)
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

            public override Test ReadJson(JsonReader reader, Type objectType, Test existingValue, bool hasExistingValue,
                JsonSerializer serializer)
            {
                var readerValue = (string?) reader.Value ?? throw new NullReferenceException();
                return new Test(readerValue);
            }
        }
        public class NotDeserializableConverter : JsonConverter<TestWithInterface>
        {
            public override void WriteJson(JsonWriter writer, TestWithInterface value, JsonSerializer serializer)
            {
                writer.WriteValue(value.Property);
            }

            public override TestWithInterface ReadJson(JsonReader reader, Type objectType, TestWithInterface existingValue, bool hasExistingValue,
                JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        public class InterfaceConverter : JsonConverter<ITest>
        {
            public override void WriteJson(JsonWriter writer, ITest value, JsonSerializer serializer)
            {
                writer.WriteValue(value.Property);
            }

            public override ITest ReadJson(JsonReader reader, Type objectType, ITest existingValue, bool hasExistingValue,
                JsonSerializer serializer)
            {
                var readerValue = (string?) reader.Value ?? throw new NullReferenceException();
                return new TestWithInterface(readerValue);
            }
        }

        public class NotDeserializableTest
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public string Property { get; }

            public NotDeserializableTest(int[] ints)
            {
                Property = Convert.ToString(ints?[0] ?? 0);
            }
        }
    }
}