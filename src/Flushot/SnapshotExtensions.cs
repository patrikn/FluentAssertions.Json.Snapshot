using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Primitives;
using Newtonsoft.Json;

namespace Flushot
{
    public static class SnapshotExtensions
    {
        private static readonly Regex FileExtensionRegex = new Regex("\\.[^.]*$");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void MatchSnapshot(this ObjectAssertions assertions,
                                         JsonSerializer? serializer = null,
                                         [CallerMemberName] string? snapshotFileName = null,
                                         [CallerFilePath] string? filePath = null)
        {
            MatchSnapshotInternal(assertions, assertions.Subject.GetType(), serializer, filePath, snapshotFileName);
        }

        public static void MatchNamedSnapshot(this ObjectAssertions assertions,
                                              string snapshotFileName,
                                              JsonSerializer? serializer = null,
                                              [CallerFilePath] string? filePath = null)
        {
            MatchSnapshotInternal(assertions, assertions.Subject.GetType(), serializer, filePath, snapshotFileName);
        }

        private static AndConstraint<ObjectAssertions> MatchSnapshotInternal(ObjectAssertions assertions, Type deserializationType,
            JsonSerializer? serializer, string? filePath, string? snapshotFileName)
        {
            var sourceFilePathNotNull = SourceFilePathWithoutExtension(filePath);
            var fileNameNotNull = snapshotFileName ?? throw new ArgumentNullException(nameof(snapshotFileName));
            var matcher = new SnapshotMatcher(new Snapshotter(sourceFilePathNotNull,
                                                              fileNameNotNull));

            return matcher.Match(assertions, deserializationType, serializer);
        }

        private static string SourceFilePathWithoutExtension(string? filePath)
        {
            var sourceFilePathNotNull = filePath ?? throw new ArgumentNullException(nameof(filePath));
            sourceFilePathNotNull = FileExtensionRegex.Replace(sourceFilePathNotNull, "");
            return sourceFilePathNotNull;
        }

        public static AndWhichConstraint<ObjectAssertions, T> MatchSnapshot<T>(
            this ObjectAssertions assertions,
            JsonSerializer? serializer = null,
            [CallerMemberName] string? snapshotFileName = null,
            [CallerFilePath] string? filePath = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            return MatchSnapshotInternal(assertions, typeof(T), serializer, filePath, snapshotFileName)
                .And.BeAssignableTo<T>();
        }

        public static AndWhichConstraint<ObjectAssertions, T> MatchNamedSnapshot<T>(
            this ObjectAssertions assertions,
            string snapshotFileName,
            JsonSerializer? serializer = null,
            [CallerFilePath] string? filePath = null)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            return MatchSnapshotInternal(assertions, typeof(T), serializer, filePath, snapshotFileName)
                .And.BeAssignableTo<T>();
        }
    }
}