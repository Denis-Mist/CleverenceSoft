namespace Exercise.Ex3
{
    public class LogProcessor
    {
        public (int processed, int errors) ProcessFile(string inputPath, string outputPath, string problemsPath)
        {
            var processed = 0;
            var errors = 0;
            
            using var reader = new StreamReader(inputPath);
            using var outputWriter = new StreamWriter(outputPath);
            using var problemsWriter = new StreamWriter(problemsPath);
            
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                var parser = ParserFactory.GetParser(line);
                var entry = parser.Parse(line);
                
                if (entry.IsValid)
                {
                    WriteFormattedEntry(entry, outputWriter);
                    processed++;
                }
                else
                {
                    problemsWriter.WriteLine(line);
                    errors++;
                }
            }
            
            return (processed, errors);
        }
        
        private void WriteFormattedEntry(LogEntry entry, StreamWriter writer)
        {
            writer.WriteLine(entry.Date.ToString("dd-MM-yyyy"));
            writer.WriteLine($"{entry.Time}\t{entry.Level}\t{entry.Method}");
            writer.WriteLine(entry.Message);
        }
    }
}
