using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FluentAssertions;
using FluentAssertions.Primitives;
using Newtonsoft.Json;

namespace Flushot
{
    public static class SnapshotExtensions
    {
        // Avoid inlining so we can find which method called us - would probably never happen but better safe than sorry
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void MatchSnapshot(this ObjectAssertions assertions,
            JsonSerializer? serializer = null, [CallerFilePath] string? filePath = null)
        {
            MatchSnapshotInternal(assertions, assertions.Subject.GetType(), serializer, filePath, null);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void MatchSnapshot(this ObjectAssertions assertions,
                                         string? snapshotFileName,
                                         JsonSerializer? serializer = null, [CallerFilePath] string? filePath = null)
        {
            MatchSnapshotInternal(assertions, assertions.Subject.GetType(), serializer, filePath, snapshotFileName);
        }

        private static AndConstraint<ObjectAssertions> MatchSnapshotInternal(ObjectAssertions assertions, Type deserializationType,
            JsonSerializer? serializer, string? filePath, string? snapshotFileName)
        {
            var filePathNotNull = filePath ?? throw new ArgumentNullException(nameof(filePath));

            SnapshotMatcher matcher;
            if (snapshotFileName != null)
            {
                matcher = new SnapshotMatcher(new Snapshotter(filePathNotNull, snapshotFileName));
            }
            else
            {
                matcher = new SnapshotMatcher(new Snapshotter(filePathNotNull, new StackTrace()));
            }

            return matcher.Match(assertions, deserializationType, serializer);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static AndWhichConstraint<ObjectAssertions, T> MatchSnapshot<T>(this ObjectAssertions assertions,
            JsonSerializer? serializer = null, [CallerFilePath] string? filePath = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            return MatchSnapshotInternal(assertions, typeof(T), serializer, filePath, null)
                .And.BeAssignableTo<T>();
        }

        public static AndWhichConstraint<ObjectAssertions, T> MatchSnapshot<T>(
            this ObjectAssertions assertions,
            string explicitlyNamedSnapshot,
            JsonSerializer? serializer = null,
            [CallerFilePath] string? filePath = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            return MatchSnapshotInternal(assertions, typeof(T), serializer, filePath, explicitlyNamedSnapshot)
                .And.BeAssignableTo<T>();
        }
    }
}