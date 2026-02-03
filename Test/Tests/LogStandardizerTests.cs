using Exercise.Ex3;

namespace LogStandardizer.Tests
{
    public class LogLevelMapperTests
    {
        [Theory]
        [InlineData("INFO", LogLevel.INFO)]
        [InlineData("INFORMATION", LogLevel.INFO)]
        [InlineData("WARN", LogLevel.WARN)]
        [InlineData("WARNING", LogLevel.WARN)]
        [InlineData("ERROR", LogLevel.ERROR)]
        [InlineData("DEBUG", LogLevel.DEBUG)]
        [InlineData("unknown", LogLevel.INFO)]
        public void Map_ReturnsCorrectLevel(string input, LogLevel expected)
        {
            var result = LogLevelMapper.Map(input);
            Assert.Equal(expected, result);
        }
    }

    public class Format1ParserTests
    {
        private readonly Format1Parser _parser = new();

        [Theory]
        [InlineData("10.03.2025 15:14:49.523 INFORMATION Версия программы: '3.4.0.48729'",
            "2025-03-10", "15:14:49.523", LogLevel.INFO, "DEFAULT", "Версия программы: '3.4.0.48729'")]
        [InlineData("31-12-2025 23:59:59.999 WARNING Something happened",
            "2025-12-31", "23:59:59.999", LogLevel.WARN, "DEFAULT", "Something happened")]
        [InlineData("01.01.2025 00:00:00.000 DEBUG Debug message",
            "2025-01-01", "00:00:00.000", LogLevel.DEBUG, "DEFAULT", "Debug message")]
        public void Parse_ValidFormat1Line_ReturnsCorrectEntry(
            string line, string expectedDate, string expectedTime,
            LogLevel expectedLevel, string expectedMethod, string expectedMessage)
        {
            var result = _parser.Parse(line);

            Assert.True(result.IsValid, "Entry should be valid");
            Assert.Equal(DateTime.Parse(expectedDate), result.Date);
            Assert.Equal(expectedTime, result.Time);
            Assert.Equal(expectedLevel, result.Level);
            Assert.Equal(expectedMethod, result.Method);
            Assert.Equal(expectedMessage, result.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Invalid line")]
        [InlineData("10.03.2025")]
        [InlineData("10.03.2025 15:14:49.523")]
        [InlineData("10.03.2025 15:14:49.523 INFO")]
        public void Parse_InvalidFormat1Line_ReturnsInvalidEntry(string line)
        {
            var result = _parser.Parse(line);

            Assert.False(result.IsValid, "Entry should be invalid");
            Assert.Equal(line, result.Original);
        }
    }

    public class Format2ParserTests
    {
        private readonly Format2Parser _parser = new();

        [Theory]
        [InlineData(
            "2025-03-10 15:14:51.5882| INFO|11|MobileComputer.GetDeviceId| Код устройства: '@MINDEO-M40-D-410244015546'",
            "2025-03-10", "15:14:51.5882", LogLevel.INFO, "MobileComputer.GetDeviceId",
            "Код устройства: '@MINDEO-M40-D-410244015546'")]
        [InlineData(
            "2025-12-31 23:59:59.9999|WARNING|999|Some.Namespace.Method|Warning message",
            "2025-12-31", "23:59:59.9999", LogLevel.WARN, "Some.Namespace.Method",
            "Warning message")]
        [InlineData(
            "2025-01-01 00:00:00.0|ERROR|0|ErrorHandler.Log|Fatal error occurred",
            "2025-01-01", "00:00:00.0", LogLevel.ERROR, "ErrorHandler.Log",
            "Fatal error occurred")]
        public void Parse_ValidFormat2Line_ReturnsCorrectEntry(
            string line, string expectedDate, string expectedTime,
            LogLevel expectedLevel, string expectedMethod, string expectedMessage)
        {
            var result = _parser.Parse(line);

            Assert.True(result.IsValid, "Entry should be valid");
            Assert.Equal(DateTime.Parse(expectedDate), result.Date);
            Assert.Equal(expectedTime, result.Time);
            Assert.Equal(expectedLevel, result.Level);
            Assert.Equal(expectedMethod, result.Method);
            Assert.Equal(expectedMessage, result.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Invalid|Line")]
        [InlineData("2025-03-10|INFO|11|Method")]
        [InlineData("2025-03-10 15:14:51.5882")]
        [InlineData("Invalid date 15:14:51.5882|INFO|11|Method|Message")]
        public void Parse_InvalidFormat2Line_ReturnsInvalidEntry(string line)
        {
            var result = _parser.Parse(line);

            Assert.False(result.IsValid, "Entry should be invalid");
            Assert.Equal(line, result.Original);
        }
    }

    public class ParserFactoryTests
    {
        public static TheoryData<string, Type> ParserTestData => new()
        {
            { "2025-03-10 15:14:51.5882| INFO|11|Method|Message", typeof(Format2Parser) },
            { "|INFO|11|Method|Message", typeof(Format2Parser) },
            { "10.03.2025 15:14:49.523 INFORMATION Message", typeof(Format1Parser) },
            { "Just a normal log message without pipes", typeof(Format1Parser) }
        };

        [Theory]
        [MemberData(nameof(ParserTestData))]
        public void GetParser_ReturnsCorrectParserType(string line, Type expectedParserType)
        {
            var parser = ParserFactory.GetParser(line);
            Assert.IsType(expectedParserType, parser);
        }
    }

    public class LogProcessorTests 
    {
        private const string TEST_INPUT_FILE = "test_input.txt";
        private const string TEST_OUTPUT_FILE = "test_output.txt";
        private const string TEST_PROBLEMS_FILE = "test_problems.txt";

        private readonly LogProcessor _processor = new();
        private readonly string _testDirectory;

        public LogProcessorTests()
        {
            _testDirectory = GetTestDirectory();
            CleanupTestFiles();
        }

        private static string GetTestDirectory()
        {
            var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(assemblyLocation) ?? Directory.GetCurrentDirectory();
        }

        private string GetTestFilePath(string fileName)
        {
            return Path.Combine(_testDirectory, fileName);
        }

        private void CleanupTestFiles()
        {
            var files = new[] { TEST_INPUT_FILE, TEST_OUTPUT_FILE, TEST_PROBLEMS_FILE };
            foreach (var file in files)
            {
                var filePath = GetTestFilePath(file);
                if (File.Exists(filePath))
                {
                    try { File.Delete(filePath); } catch { }
                }
            }
        }

        public static TheoryData<string[], int, int, string[], string[]> ProcessFileTestData => new()
        {
            // Format 1 lines
            {
                new[] { "10.03.2025 15:14:49.523 INFORMATION Message 1" },
                1, 0,
                new[] { "10-03-2025", "15:14:49.523\tINFO\tDEFAULT", "Message 1" },
                Array.Empty<string>()
            },
            
            // Format 2 lines
            {
                new[] { "2025-03-10 15:14:51.5882|INFO|11|Method1|Message 2" },
                1, 0,
                new[] { "10-03-2025", "15:14:51.5882\tINFO\tMethod1", "Message 2" },
                Array.Empty<string>()
            },
            
            // Mixed valid lines
            {
                new[]
                {
                    "10.03.2025 15:14:49.523 INFORMATION Message 1",
                    "2025-03-10 15:14:51.5882|INFO|11|Method1|Message 2",
                    "31-12-2025 23:59:59.999 WARNING Message 3"
                },
                3, 0,
                new[]
                {
                    "10-03-2025", "15:14:49.523\tINFO\tDEFAULT", "Message 1",
                    "10-03-2025", "15:14:51.5882\tINFO\tMethod1", "Message 2",
                    "31-12-2025", "23:59:59.999\tWARN\tDEFAULT", "Message 3"
                },
                Array.Empty<string>()
            },
            
            // With invalid lines
            {
                new[]
                {
                    "10.03.2025 15:14:49.523 INFORMATION Valid",
                    "Invalid line 1",
                    "2025-03-10 15:14:51.5882|INFO|11|Method|Valid 2",
                    "Invalid line 2"
                },
                2, 2,
                new[]
                {
                    "10-03-2025", "15:14:49.523\tINFO\tDEFAULT", "Valid",
                    "10-03-2025", "15:14:51.5882\tINFO\tMethod", "Valid 2"
                },
                new[] { "Invalid line 1", "Invalid line 2" }
            },
            
            // Empty lines
            {
                new[] { "", "  ", "\t", "10.03.2025 15:14:49.523 INFORMATION Message" },
                1, 0,
                new[] { "10-03-2025", "15:14:49.523\tINFO\tDEFAULT", "Message" },
                Array.Empty<string>()
            }
        };

        [Theory]
        [MemberData(nameof(ProcessFileTestData))]
        public void ProcessFile_ProcessesCorrectly(
            string[] inputLines, int expectedProcessed, int expectedErrors,
            string[] expectedOutputLines, string[] expectedProblemLines)
        {
            var inputPath = GetTestFilePath(TEST_INPUT_FILE);
            var outputPath = GetTestFilePath(TEST_OUTPUT_FILE);
            var problemsPath = GetTestFilePath(TEST_PROBLEMS_FILE);

            File.WriteAllLines(inputPath, inputLines);

            var result = _processor.ProcessFile(inputPath, outputPath, problemsPath);

            Assert.Equal(expectedProcessed, result.processed);
            Assert.Equal(expectedErrors, result.errors);

            if (expectedProcessed > 0)
            {
                Assert.True(File.Exists(outputPath), "Output file should exist");
                var outputLines = File.ReadAllLines(outputPath);
                Assert.Equal(expectedOutputLines, outputLines);
            }

            if (expectedErrors > 0)
            {
                Assert.True(File.Exists(problemsPath), "Problems file should exist");
                var problemLines = File.ReadAllLines(problemsPath);
                Assert.Equal(expectedProblemLines, problemLines);
            }
            else if (File.Exists(problemsPath))
            {
                var problemLines = File.ReadAllLines(problemsPath);
                Assert.Empty(problemLines);
            }
        }
    }
}