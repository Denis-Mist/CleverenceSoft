namespace Exercise.Ex3
{
    public interface ILogParser
    {
        LogEntry Parse(string line);
    }

    public abstract class BaseParser : ILogParser
    {
        public abstract LogEntry Parse(string line);

        protected static LogEntry InvalidEntry(string line) =>
            new() { Original = line, IsValid = false };

        protected static bool TryParseDate(string dateStr, string[] formats, out DateTime date)
        {
            return DateTime.TryParseExact(dateStr, formats, null,
                System.Globalization.DateTimeStyles.None, out date);
        }

        protected static bool TryParseDate(string dateStr, out DateTime date)
        {
            return DateTime.TryParse(dateStr, out date);
        }

        protected static void SetCommonProperties(LogEntry entry, DateTime date,
            string time, string levelStr)
        {
            entry.Date = date;
            entry.Time = time;
            entry.Level = LogLevelMapper.Map(levelStr);
            entry.IsValid = true;
        }
    }

    public class Format1Parser : BaseParser
    {
        private static readonly string[] _dateFormats = { "dd.MM.yyyy", "dd-MM-yyyy" };
        private const string DefaultMethod = "DEFAULT";

        public override LogEntry Parse(string line)
        {
            var entry = new LogEntry { Original = line };

            try
            {
                var parts = line.Split(' ', 4);
                if (parts.Length < 4) return InvalidEntry(line);

                if (!TryParseDate(parts[0], _dateFormats, out var date))
                    return InvalidEntry(line);

                SetCommonProperties(entry, date, parts[1], parts[2]);
                entry.Message = parts[3];
                entry.Method = DefaultMethod;
            }
            catch
            {
                return InvalidEntry(line);
            }

            return entry;
        }
    }

    public class Format2Parser : BaseParser
    {
        private const char Delimiter = '|';
        private const int ExpectedPartsCount = 5;

        public override LogEntry Parse(string line)
        {
            var entry = new LogEntry { Original = line };

            try
            {
                var pipeParts = line.Split(Delimiter, ExpectedPartsCount);
                if (pipeParts.Length < ExpectedPartsCount) return InvalidEntry(line);

                var dateTimePart = pipeParts[0].Trim();
                var spaceIndex = dateTimePart.IndexOf(' ');

                if (spaceIndex <= 0) return InvalidEntry(line);

                var dateStr = dateTimePart[..spaceIndex];
                var timeStr = dateTimePart[(spaceIndex + 1)..];

                if (!TryParseDate(dateStr, out var date))
                    return InvalidEntry(line);

                SetCommonProperties(entry, date, timeStr, pipeParts[1].Trim());
                entry.Method = pipeParts[3].Trim();
                entry.Message = pipeParts[4].Trim();
            }
            catch
            {
                return InvalidEntry(line);
            }

            return entry;
        }
    }

    public static class ParserFactory
    {
        public static ILogParser GetParser(string line)
        {
            return line.Contains('|') ? new Format2Parser() : new Format1Parser();
        }
    }
}