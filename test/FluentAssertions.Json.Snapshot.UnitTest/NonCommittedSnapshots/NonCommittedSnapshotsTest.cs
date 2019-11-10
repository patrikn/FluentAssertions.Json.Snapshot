using System;
using System.IO;
using System.Runtime.CompilerServices;
using Xunit;

namespace FluentAssertions.Json.Snapshot.UnitTest.NonCommittedSnapshots
{
    public class NonCommittedSnapshotsTest : IDisposable
    {
        public NonCommittedSnapshotsTest()
        {
            // Make sure we don't have one left over from earlier aborted test run
            DeleteSnapshotDirectory();
        }

        [Fact]
        public void Should_work_if_directory_does_not_exist()
        {
            new Test().Should().MatchSnapshot();
        }

        public class NestedClass : NonCommittedSnapshotsTest
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
            if (Directory.Exists(GetSnapshotDirectory()))
            {
                Directory.Delete(GetSnapshotDirectory(), true);
            }
        }

        private static string GetSnapshotDirectory([CallerFilePath] string? path = null)
        {
            return Path.Combine(Path.GetDirectoryName(path), "_snapshots");
        }
    }
}