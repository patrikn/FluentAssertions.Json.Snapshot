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

        public class Test
        {
            public int Property = 1;
        }

        public void Dispose()
        {
            DeleteSnapshotDirectory();
        }

        private void DeleteSnapshotDirectory([CallerFilePath] string path = null)
        {
            Directory.Delete(Path.Combine(Path.GetDirectoryName(path), "_snapshots"), true);
        }
    }
}