namespace Exercise.Ex3
{
    public enum LogLevel
    {
        INFO,
        WARN,
        ERROR,
        DEBUG
    }

    public class LogEntry
    {
        public DateTime Date { get; set; }
        public string Time { get; set; } = string.Empty;
        public LogLevel Level { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Original { get; set; } = string.Empty;
        public bool IsValid { get; set; } = true;
    }

    public static class LogLevelMapper
    {
        private static readonly Dictionary<string, LogLevel> _map = new(StringComparer.OrdinalIgnoreCase)
        {
            ["INFO"] = LogLevel.INFO,
            ["INFORMATION"] = LogLevel.INFO,
            ["WARN"] = LogLevel.WARN,
            ["WARNING"] = LogLevel.WARN,
            ["ERROR"] = LogLevel.ERROR,
            ["DEBUG"] = LogLevel.DEBUG
        };

        public static LogLevel Map(string level) =>
            _map.TryGetValue(level, out var mapped) ? mapped : LogLevel.INFO;
    }
}
