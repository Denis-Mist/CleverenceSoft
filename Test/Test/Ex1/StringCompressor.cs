using System.Text;

public static class StringCompression
{
    public static string Compress(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();
        int count = 1;

        for (int i = 1; i <= input.Length; i++)
        {
            if (i < input.Length && input[i] == input[i - 1])
            {
                count++;
            }
            else
            {
                result.Append(input[i - 1]);

                if (count > 1)
                    result.Append(count);

                count = 1;
            }
        }

        return result.ToString();
    }

    public static string Decompress(string compressed)
    {
        if (string.IsNullOrEmpty(compressed))
            return compressed;

        var result = new StringBuilder();
        int i = 0;

        while (i < compressed.Length)
        {
            char currentChar = compressed[i];
            i++;

            var numberBuilder = new StringBuilder();
            while (i < compressed.Length && char.IsDigit(compressed[i]))
            {
                numberBuilder.Append(compressed[i]);
                i++;
            }

            if (numberBuilder.Length > 0)
            {
                int count = int.Parse(numberBuilder.ToString());
                result.Append(currentChar, count);
            }
            else
            {
                result.Append(currentChar);
            }
        }

        return result.ToString();
    }
}