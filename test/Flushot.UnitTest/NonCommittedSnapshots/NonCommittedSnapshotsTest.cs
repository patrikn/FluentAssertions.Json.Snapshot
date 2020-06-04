using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Flushot.UnitTest.NonCommittedSnapshots
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
            private readonly ITestOutputHelper _outputHelper;

            public NestedClass(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public void Should_create_directory_for_nested_class()
            {
                Snapshotter.Output = _outputHelper;
                new Test().Should().MatchSnapshot();
                var snapshotDirectory = GetSnapshotDirectory();
                var expectedSnapshotPath = Path.Combine(
                    snapshotDirectory,
                    nameof(NonCommittedSnapshotsTest),
                    nameof(NestedClass),
                    $"{nameof(Should_create_directory_for_nested_class)}.json");

                var tree = GenerateTree(snapshotDirectory, 0);
                _outputHelper.WriteLine(tree);
                string? dir = expectedSnapshotPath;
                if (!File.Exists(expectedSnapshotPath))
                {
                    _outputHelper.WriteLine($"{expectedSnapshotPath} {File.Exists(expectedSnapshotPath)}");
                    while ((dir = Path.GetDirectoryName(dir)) != null)
                    {
                        _outputHelper.WriteLine($"{dir}: {Directory.Exists(dir)}");
                    }
                }
                File.Exists(expectedSnapshotPath).Should().BeTrue($" {expectedSnapshotPath} should be created by the test.");
            }

            private string GenerateTree(string root, int depth)
            {
                var selfIndent = string.Concat(Enumerable.Repeat("| ", depth));
                var indent = selfIndent + "| ";
                var tree = $"{selfIndent}|-+ {Path.GetFileName(root)} ({Path.GetDirectoryName(root)}){Environment.NewLine}";
                foreach (var dir in Directory.EnumerateDirectories(root))
                {
                    tree += $"{GenerateTree(dir, depth + 1)}";
                }

                foreach (var file in Directory.EnumerateFiles(root))
                {
                    tree += $"{indent}|-> {Path.GetFileName(file)} ({Path.GetDirectoryName(file)}){Environment.NewLine}";
                }

                return tree;
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
            var pathNotNull = path ?? throw new ArgumentNullException(nameof(path));
            var directoryName = Path.GetDirectoryName(pathNotNull) ?? throw new DirectoryNotFoundException(pathNotNull);
            return Path.Combine(directoryName, "_snapshots");
        }
    }
}
