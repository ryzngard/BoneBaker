using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace BoneBaker
{
    /// <summary>
    /// Copied from https://raw.githubusercontent.com/dotnet/roslyn/48a80e0e3dc95bcd769d9f03586e8e95c58d546a/src/Razor/src/Shared/Microsoft.AspNetCore.Razor.Utilities.Shared/Assumed.cs
    /// </summary>
    public static class NullUtil
    {
        public static void NotNull<T>(
                [NotNull] this T? value,
                string? message = null,
                [CallerArgumentExpression(nameof(value))] string? valueExpression = null,
                [CallerFilePath] string? path = null,
                [CallerLineNumber] int line = 0)
                where T : class
        {
            if (value is null)
            {
                ThrowInvalidOperation(message ?? $"{valueExpression} expected to be not null", path, line);
            }
        }

        public static void NotNull<T>(
            [NotNull] this T? value,
            [InterpolatedStringHandlerArgument(nameof(value))] ThrowIfNullInterpolatedStringHandler<T> message,
            [CallerFilePath] string? path = null,
            [CallerLineNumber] int line = 0)
            where T : class
        {
            if (value is null)
            {
                ThrowInvalidOperation(message.GetFormattedText(), path, line);
            }
        }

        public static void NotNull<T>(
            [NotNull] this T? value,
            string? message = null,
            [CallerArgumentExpression(nameof(value))] string? valueExpression = null,
            [CallerFilePath] string? path = null,
            [CallerLineNumber] int line = 0)
            where T : struct
        {
            if (value is null)
            {
                ThrowInvalidOperation(message ?? $"{valueExpression} expected to be not null", path, line);
            }
        }

        public static void NotNull<T>(
            [NotNull] this T? value,
            [InterpolatedStringHandlerArgument(nameof(value))] ThrowIfNullInterpolatedStringHandler<T> message,
            [CallerFilePath] string? path = null,
            [CallerLineNumber] int line = 0)
            where T : struct
        {
            if (value is null)
            {
                ThrowInvalidOperation(message.GetFormattedText(), path, line);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void True(
            [DoesNotReturnIf(false)] bool condition,
            string? message = null,
            [CallerFilePath] string? path = null,
            [CallerLineNumber] int line = 0)
        {
            if (!condition)
            {
                ThrowInvalidOperation(message ?? "Expected to be true", path, line);
            }
        }

        [DebuggerHidden]
        [DoesNotReturn]
        private static void ThrowInvalidOperation(string message, string? path, int line)
            => throw new InvalidOperationException(message + Environment.NewLine + $"Path:{path}, Line:{line}");
    }

    [InterpolatedStringHandler]
    public readonly ref struct ThrowIfNullInterpolatedStringHandler<T>
    {
        private readonly PooledStringBuilderHelper _builder;

        public ThrowIfNullInterpolatedStringHandler(int literalLength, int formattedCount, T? value, out bool success)
        {
            success = value is null;
            _builder = new(literalLength, success);
        }

        public void AppendLiteral(string value)
            => _builder.AppendLiteral(value);

        public void AppendFormatted<TValue>(TValue value)
            => _builder.AppendFormatted(value);

        public void AppendFormatted<TValue>(TValue value, string format)
            where TValue : IFormattable
            => _builder.AppendFormatted(value, format);

        public string GetFormattedText()
            => _builder.GetFormattedText();

        private ref struct PooledStringBuilderHelper
        {
            private StringBuilder? _builder;

            public PooledStringBuilderHelper(int capacity, bool condition)
            {
                if (condition)
                {
                    _builder = new();
                    _builder.EnsureCapacity(capacity);
                }
            }

            public readonly void AppendLiteral(string value)
                => _builder!.Append(value);

            public readonly void AppendFormatted<T>(T value)
                => _builder!.Append(value?.ToString());

            public readonly void AppendFormatted<TValue>(TValue value, string format)
                where TValue : IFormattable
                => _builder!.Append(value?.ToString(format, formatProvider: null));

            public string GetFormattedText()
            {
                var builder = Interlocked.Exchange(ref _builder, null);

                if (builder is not null)
                {
                    var result = builder.ToString();
                    return result;
                }

                throw new InvalidOperationException();
            }
        }
    }
}