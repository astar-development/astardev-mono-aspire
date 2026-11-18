using Microsoft.Extensions.Logging;

namespace Fab4Kids.Web.TestsUnit.Helpers;

/// <summary>Records each entry's values when it is logged: the logger's state object is pooled and cleared after the call returns, so reading it later finds nothing.</summary>
internal sealed class RecordingLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, EventId EventId, IReadOnlyDictionary<string, object?> Values)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var values = state is IEnumerable<KeyValuePair<string, object?>> pairs ? pairs.ToDictionary(pair => pair.Key, pair => pair.Value) : [];
        Entries.Add((logLevel, eventId, values));
    }
}

internal static class LoggedEvents
{
    public static ILogger<T> CreateEnabledLogger<T>() => new RecordingLogger<T>();

    private static List<(LogLevel Level, EventId EventId, IReadOnlyDictionary<string, object?> Values)> EntriesOf(ILogger logger) => ((dynamic)logger).Entries;

    public static IReadOnlyDictionary<string, object?> ValuesOf(this ILogger logger, string eventName)
        => EntriesOf(logger).Where(entry => entry.EventId.Name == eventName).Select(entry => entry.Values).ShouldHaveSingleItem();

    public static IReadOnlyList<string> AllLoggedValues(this ILogger logger)
        => [.. EntriesOf(logger).SelectMany(entry => entry.Values).Select(pair => pair.Value?.ToString() ?? string.Empty)];

    public static IReadOnlyList<LogLevel> LevelsOf(this ILogger logger, string eventName)
        => [.. EntriesOf(logger).Where(entry => entry.EventId.Name == eventName).Select(entry => entry.Level)];
}
