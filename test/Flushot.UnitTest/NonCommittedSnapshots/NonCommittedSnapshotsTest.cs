using System;
using System.IO;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace FluentAssertions.Json.Snapshot.UnitTest.NonCommittedSnapshots
{
    public class NonCommittedSnapshotsTest : IDisposable
    {
        private readonly ITestOutputHelper _outputHelper;

        public NonCommittedSnapshotsTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            // Make sure we don't have one left over from earlier aborted test run
            DeleteSnapshotDirectory();
        }

        [Fact]
        public void Should_work_if_directory_does_not_exist()
        {
            new Test().Should().MatchSnapshot();
        }

        public class NestedClass
        {
            [Fact]
            public void Should_create_directory_for_nested_class()
            {
                new Test().Should().MatchSnapshot();
                var expectedSnapshotPath = Path.Combine(
                    GetSnapshotDirectory(),
                    nameof(NonCommittedSnapshotsTest),
                    nameof(NestedClass),
                    $"{nameof(Should_create_directory_for_nested_class)}.json");
                File.Exists(expectedSnapshotPath).Should().BeTrue();
            }

            public NestedClass(ITestOutputHelper outputHelper) : base(outputHelper)
            {
            }
        }

        public class Test
        {
            public int Property = 1;
        }

        public void Dispose()
        {
            DeleteSnapshotDirectory();
        }

        private void DeleteSnapshotDirectory()
        {
            var snapshotDirectory = GetSnapshotDirectory();
            if (Directory.Exists(snapshotDirectory))
            {
                try
                {
                    Directory.Delete(snapshotDirectory, true);
                }
                catch (Exception e)
                {
                    _outputHelper.WriteLine(
                        $"Failed to delete snapshot directory {snapshotDirectory}: {e.Message}");
                }
            }
        }

        private static string GetSnapshotDirectory([CallerFilePath] string? path = null)
        {
            return Path.Combine(Path.GetDirectoryName(path), "_snapshots");
        }
    }
}
