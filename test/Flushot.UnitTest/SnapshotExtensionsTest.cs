using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Flushot.UnitTest
{
    public class SnapshotExtensionsTest
    {
        public SnapshotExtensionsTest(ITestOutputHelper output)
        {
        }

        [Fact]
        public void Should_match_matching_snapshot()
        {
            new Test("hej").Should().MatchSnapshot();
        }

        [Fact]
        public void Should_respect_equivalency_options()
        {
            new TestWithIgnoredProperty("hej", "notHere")
                .Should()
                .MatchSnapshot<TestWithIgnoredProperty>(
                    equivalencyConfig: cfg => cfg.Excluding(x => x.Ignored));
        }

        [Fact]
        public void Should_not_match_changed_snapshot()
        {
            Action act = () => new Test("hej").Should().MatchSnapshot();

            act.Should().Throw<XunitException>();
        }

        [Fact]
        public void Should_include_snapshot_path_in_because()
        {
            Action act = () => new Test("hej").Should().MatchSnapshot();

            var sep = Path.DirectorySeparatorChar;
            act.Should()
               .Throw<XunitException>()
               .WithMessage(
                   $"*_snapshots{sep}SnapshotExtensionsTest{sep}Should_include_snapshot_path_in_because.json*");
        }

        [Fact]
        public void Should_create_snapshot_if_missing()
        {
            var snapshotFilePath = Path.Combine(
                "..",
                "..",
                "..",
                "_snapshots",
                $"{nameof(SnapshotExtensionsTest)}",
                $"{nameof(Should_create_snapshot_if_missing)}.json");
            // Delete first to make sure it isn't left over from aborted run
            File.Delete(snapshotFilePath);
            try
            {
                new Test("property").Should().MatchSnapshot();
                File.Exists(snapshotFilePath).Should().BeTrue($"{snapshotFilePath} should exist");
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
            Action act = () => new NotDeserializableTest(
                    new[]
                    {
                        123
                    }).Should()
                      .MatchSnapshot();

            act.Should().Throw<XunitException>();
        }

        [Fact]
        public void Should_return_generic_typed_constraint()
        {
            new Test("test").Should()
                            .MatchSnapshot<Test>()
                            .Subject.Property.Should()
                            .Be("test");
        }

        [Fact]
        public void Should_deserialize_to_interface()
        {
            new TestWithInterface("test").Should()
                                         .MatchSnapshot<ITest>()
                                         .Subject.Property.Should()
                                         .Be("test");
        }

        [Fact]
        public void Should_allow_explicitly_named_snapshots()
        {
            new Test("hej").Should()
                           .MatchNamedSnapshot("ExplicitlyNamed/Explicitly_named_snapshot");
        }

        [Fact]
        public void Should_not_match_changed_explicitly_named_snapshot()
        {
            Action act = () =>
                new Test("hej").Should()
                               .MatchNamedSnapshot("ExplicitlyNamed/Changed_explicitly_named_snapshot");

            var sep = Path.DirectorySeparatorChar;
            act.Should()
               .Throw<XunitException>()
               .WithMessage(
                   $"*SnapshotExtensionsTest{sep}ExplicitlyNamed{sep}Changed_explicitly_named_snapshot.json*");
        }

        [Fact]
        public void Should_allow_explicitly_named_snapshots_with_constraint()
        {
            new Test("xyzzy").Should()
                             .MatchNamedSnapshot<Test>(
                                 "ExplicitlyNamed/Explicitly_named_snapshot_with_constraint")
                             .And.NotBeNull();
        }

        [Fact]
        public async Task Should_find_file_in_lambda_async()
        {
            Action act = () => new Test("hej").Should().MatchSnapshot();
            await Task.Run(
                () =>
                {
                    using var scope = new AssertionScope();
                    act.Invoke();
                    var messages = scope.Discard();
                    messages
                        .Should()
                        .SatisfyRespectively(
                            message => message
                                       .Should()
                                       .StartWith("JSON document has a different value at $.Property"),
                            message => message
                                       .Should()
                                       .StartWith("Expected member Property to be"));
                });
        }

        private class Test
        {
            public Test(string property)
            {
                Property = property;
            }

            public string Property { get; }
        }

        [JsonConverter(typeof(InterfaceConverter))]
        private interface ITest
        {
            string Property { get; }
        }

        [JsonConverter(typeof(NotDeserializableConverter))]
        private class TestWithInterface : ITest
        {
            public TestWithInterface(string property)
            {
                Property = property;
            }

            public string Property { get; }
        }

        private class CustomTestConverter : JsonConverter<Test>
        {
            public override void WriteJson(JsonWriter writer, Test? value, JsonSerializer serializer)
            {
                writer.WriteValue(value?.Property);
            }

            public override Test ReadJson(
                JsonReader reader,
                Type objectType,
                Test? existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
            {
                var readerValue = (string?)reader.Value ?? throw new NullReferenceException();
                return new Test(readerValue);
            }
        }

        private class NotDeserializableConverter : JsonConverter<TestWithInterface>
        {
            public override void WriteJson(
                JsonWriter writer,
                TestWithInterface? value,
                JsonSerializer serializer)
            {
                writer.WriteValue(value?.Property);
            }

            public override TestWithInterface ReadJson(
                JsonReader reader,
                Type objectType,
                TestWithInterface? existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
            {
                throw new Exception();
            }
        }

        private class InterfaceConverter : JsonConverter<ITest>
        {
            public override void WriteJson(JsonWriter writer, ITest? value, JsonSerializer serializer)
            {
                writer.WriteValue(value?.Property);
            }

            public override ITest ReadJson(
                JsonReader reader,
                Type objectType,
                ITest? existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
            {
                var readerValue = (string?)reader.Value ?? throw new NullReferenceException();
                return new TestWithInterface(readerValue);
            }
        }

        private class NotDeserializableTest
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            // ReSharper disable once MemberCanBePrivate.Local
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Property { get; }

            public NotDeserializableTest(int[] ints)
            {
                // ReSharper disable once ConstantConditionalAccessQualifier
                Property = Convert.ToString(ints?[0] ?? 0);
            }
        }

        public class TestWithIgnoredProperty
        {
            // Only here to test that it gets ignored with equivalency options...
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            // ReSharper disable once MemberCanBePrivate.Global
            public string NotIgnored { get; }

            [JsonIgnore]
            public string? Ignored { get; }

            [JsonConstructor]
            public TestWithIgnoredProperty(string notIgnored)
            {
                NotIgnored = notIgnored;
                Ignored = null;
            }

            public TestWithIgnoredProperty(string notIgnored, string ignored)
            {
                NotIgnored = notIgnored;
                Ignored = ignored;
            }
        }
    }
}
