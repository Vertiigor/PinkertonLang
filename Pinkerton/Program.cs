using PinkertonInterpreter.Grammar;
using System.Text;

namespace PinkertonInterpreter
{
    internal class Program
    {
        private static Interpreter interpreter = new Interpreter();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("    ____  _       __             __                      ___ ____ \r\n   / __ \\(_)___  / /_____  _____/ /_____  ____     _   _<  // __ \\\r\n  / /_/ / / __ \\/ //_/ _ \\/ ___/ __/ __ \\/ __ \\   | | / / // / / /\r\n / ____/ / / / / ,< /  __/ /  / /_/ /_/ / / / /   | |/ / // /_/ / \r\n/_/   /_/_/ /_/_/|_|\\___/_/   \\__/\\____/_/ /_/    |___/_(_)____/  \r\n                                                                  ");
            Console.ResetColor();

            string input;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Pinkerton> ");
                Console.ResetColor();
                input = Console.ReadLine();
                if (input?.ToLower() == "exit")
                {
                    break;
                }
                if (input?.ToLower() == "clear")
                {
                    Console.Clear();
                    continue;
                }
                if (input?.Split(' ')[0].ToLower() == "run") // run file.pink
                {
                    string fileName = input.Split(' ')[1].Trim();
                    if (File.Exists(fileName))
                    {
                        Run(fileName);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"File not found: {fileName}");
                        Console.ResetColor();
                    }
                }
                if (input?.Split(' ')[0].ToLower() == "show")
                {
                    string fileName = input.Split(' ')[1].Trim();
                    byte[] bytes = File.ReadAllBytes(fileName);
                    string fileContent = Encoding.UTF8.GetString(bytes);
                    var lines = fileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    var lineNum = 1;

                    Console.ForegroundColor = ConsoleColor.White;
                    foreach (var line in lines)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"{lineNum}.\t");
                        Console.ResetColor();
                        Console.WriteLine($"{line}");
                        lineNum++;
                    }
                    Console.ResetColor();
                }
                if (input?.ToLower() == "list")
                {
                    var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.pink");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Available .pink files:");
                    Console.ResetColor();

                    foreach (var file in files)
                    {
                        Console.WriteLine(Path.GetFileName(file));
                    }

                    continue;
                }
                if (input?.ToLower() == "prompt")
                {
                    RunPrompt();
                }
            }
        }

        private static void Run(string filePath)
        {
            // Load the file and execute the program
            byte[] bytes = File.ReadAllBytes(filePath);

            // Convert the byte array to a string using Encoding.UTF8.GetString
            string fileContent = Encoding.UTF8.GetString(bytes);

            Execute(fileContent);
        }

        private static void Execute(string content)
        {
            Scanner scanner = new Scanner(content);
            List<Token> tokens = scanner.ScanTokens();

            Parser parser = new Parser(tokens);

            List<Statement> statements = parser.Parse();

            if (statements == null || statements.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Parsing failed.");
                Console.ResetColor();
                return;
            }

            foreach (var stmt in statements)
            {
                try
                {
                    interpreter.Execute(stmt);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Runtime error: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }


        public static void Error(Token token, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (token.Type == TokenType.EOF)
            {
                report(token.Line, " at end", message);
            }
            else
            {
                report(token.Line, " at '" + token.Lexeme + "'", message);
            }
            Console.ResetColor();
        }

        private static void report(int line, string where, string message)
        {
            Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
        }

        private static void RunPrompt()
        {
            int lineNumber = 1;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{lineNumber}. ");
                Console.ResetColor();

                string input = Console.ReadLine();

                lineNumber++;

                if (input?.ToLower() == "exit")
                {
                    return;
                }
                if (input?.ToLower() == "clear")
                {
                    Console.Clear();
                    continue;
                }

                Execute(input);
            }
        }
    }
}
