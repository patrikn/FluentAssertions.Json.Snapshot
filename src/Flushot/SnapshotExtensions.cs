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
            var filePathNotNull = filePath ?? throw new ArgumentNullException(nameof(filePath));
            // ReSharper disable once ExplicitCallerInfoArgument
            MatchSnapshotInternal(assertions, assertions.Subject.GetType(), serializer, filePathNotNull, new StackTrace());
        }

        private static AndConstraint<ObjectAssertions> MatchSnapshotInternal(ObjectAssertions assertions, Type deserializationType,
            JsonSerializer? serializer, string filePath, StackTrace stackTrace)
        {
            return new SnapshotMatcher(new Snapshotter(filePath, stackTrace))
                .Match(assertions, deserializationType, serializer);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static AndWhichConstraint<ObjectAssertions, T> MatchSnapshot<T>(this ObjectAssertions assertions,
            JsonSerializer? serializer = null, [CallerFilePath] string? filePath = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            var filePathNotNull = filePath ?? throw new ArgumentNullException(nameof(filePath));
            return MatchSnapshotInternal(assertions, typeof(T), serializer, filePathNotNull, new StackTrace())
                .And.BeAssignableTo<T>();
        }
    }
}