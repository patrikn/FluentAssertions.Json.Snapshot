using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FluentAssertions.Primitives;
using Newtonsoft.Json;

namespace FluentAssertions.Json.Snapshot
{
    public static class SnapshotExtensions
    {
        // Avoid inlining so we can find which method called us - would probably never happen but better safe than sorry
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void MatchSnapshot(this ObjectAssertions assertions,
            JsonSerializer serializer = null, [CallerFilePath] string filePath = null)
        {
            new SnapshotMatcher(new Snapshotter(filePath, new StackTrace()))
                .Match(assertions, serializer, filePath);
        }

    }
}