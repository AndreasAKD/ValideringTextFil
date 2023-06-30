using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

class Program

{
    public enum HeaderEnum
    {
        BO,
        SI,
        AK,
        IK,
        PU,
        KO,
        SC,
        TO,
        UE,
        PR,
        KA
    }

    static void Main()
    {
        bool allLinesValid = ValidateAllLines();
        if (allLinesValid)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All lines are valid.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Some lines are invalid.");
            Console.ResetColor();
        }
    }

    static bool ValidateAllLines()
    {
        string filePath = "C:\\Users\\AndreasDahlgren\\source\\repos\\NCValidatorVersion2\\NCValidatorVersion2\\Example.txt";
        int lineNumber = 1;
        bool allLinesValid = true; 

        using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                bool isValid = ValidateLine(line);
                if (!isValid)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Invalid Line: Line Number: {lineNumber}, Content: {line}");
                    Console.ResetColor();
                    allLinesValid = false; 
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"VALID Line: Line Number: {lineNumber}, Content: {line}");
                    Console.ResetColor();
                }
                lineNumber++;
            }
        }

        return allLinesValid; 
    }

    static bool ValidateLine(string line)
    {

        bool isValidPattern1 = ValidateStartAndEnd(line);
        bool isValidPattern2 = ValidateHeaderEnums(line);
        bool isValidPattern3 = ValidateBlockValuesAfterHeader(line);
        bool isValidPattern4 = ValidateWhiteLines(line);
        bool isValidPattern5 = ValidateHeaderBlocs(line);
        bool isValidPattern6 = ValidateComments(line);
        bool isValidPattern7 = ValidateProfile(line);
        bool isValidPattern8 = ContainsNumericValue(line);
        bool isValidPattern9 = ValidateLineWithDataStringsOnly(line);
        bool isValidPattern10 = ValidateDstvInclusion(line);
        bool isValidPattern11 = ValidateOtherLines(line);


        if (isValidPattern1 || isValidPattern2 || isValidPattern3 || isValidPattern4 || isValidPattern5 || isValidPattern6 || isValidPattern7 || isValidPattern8 || isValidPattern9 || isValidPattern10 || isValidPattern11)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    static bool startsWithWhiteSpaceLetterAndTab(string line)
    {
        string pattern = @"^\s[-+a-zA-Z]\t";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(line);
    }

    static bool ValidateStartAndEnd(string line)
    {
        if (line.StartsWith("ST") || line.StartsWith("EN"))
        {
            return true;
        }
        return false;
    }

    static bool ValidateOtherLines(string line)
    {
        string pattern = @"^\s[A-Za-z](?!\t)\w*";

        if (Regex.IsMatch(line, pattern) && !ValidateProfile(line))
        {
            return true; 
        }
        else
        {
            return false; 
        }
    }

    static bool ContainsNumericValue(string line)
    {
        string[] values = line.Split(',');

        // Kontrollerar om det endast finns ett värde
        if (values.Length == 1)
        {
            double numericValue;
            // Försöker checka värdet till om det är en double
            return double.TryParse(values[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out numericValue);
        }

        return false;
    }

    static bool ValidateComments(string line)
    {
        if (line.StartsWith("**"))
        {
            return true;
        }
        return false;
    }

    static bool ValidateWhiteLines(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return true;
        }
        return false;
    }

    static bool ValidateHeaderEnums(string line)
    {
        HeaderEnum[] enumValues = (HeaderEnum[])Enum.GetValues(typeof(HeaderEnum));

        foreach (HeaderEnum enumValue in enumValues)
        {
            if (line.StartsWith(enumValue.ToString()))
            {
                return true;
            }
        }
        return false;
    }

    static bool ValidateBlockValuesAfterHeader(string line)
    {
        if (startsWithWhiteSpaceLetterAndTab(line))
        {
            string cleanLine = Regex.Replace(line, @"^\s*\D+", "");

            if (ValidateLineWithDataStrings(cleanLine))
            {
                return true; 
            }
        }
        return false;
    }

    static bool ValidateHeaderBlocs(string line)
    {
        string pattern = @"^[EBSAIPK][0-9]";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(line);
    }

    static bool ValidateProfile(string line)
    {
        string pattern = @"\b(I|L|U|B|RU|RO|M|C|T|SO)\b";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(line);
    }

    static bool ValidateDstvInclusion(string line)
    {
        string pattern = @"\bDSTV\b";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(line);
    }

    static bool ValidateLineWithDataStrings(string line)
    {
        // Delar upp raden i element baserat på tab-tecken ('\t')
        string[] elements = line.Split('\t');

        int dataStringCount = 0;
        bool firstElementSkipped = false;

        // Itererar genom varje element i raden
        foreach (string element in elements)
        {
            // Kontrollerar om elementet är en tom sträng eller om det innehåller whitespace
            if (!string.IsNullOrEmpty(element.Trim()) || !firstElementSkipped)
            {
                // Kontrollerar om det är det första elementet och hoppar över inledande blanksteg eller tomma element
                if (!firstElementSkipped)
                {
                    // Hoppa över inledande blanksteg eller tomma element
                    if (string.IsNullOrWhiteSpace(element))
                    {
                        continue;
                    }
                    firstElementSkipped = true;
                }
                dataStringCount++;
            }
        }
        // Kontrollerar om antalet datasträngar är minst 3
        if (dataStringCount >= 3)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    static bool ValidateLineWithDataStringsOnly(string line)
    {
        if (line.StartsWith("\t"))
        {        
            // Delar upp raden i element baserat på tab-tecken ('\t')
            string[] elements = line.Split('\t');

            // Räknar antalet dataelement efter det första elementet som inte är blanksteg eller tomma strängar
            int dataElementCount = elements.Skip(1).Count(element => !string.IsNullOrWhiteSpace(element));

            // Kontrollerar om antalet dataelement är minst 3
            if (dataElementCount >= 3)
            {
                return true; 
            }
        }
        return false; 
    }
}
