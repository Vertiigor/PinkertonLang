using PinkertonInterpreter.Grammar;
using System.Text;

namespace PinkertonInterpreter
{
    internal class Program
    {
        private static Interpreter interpreter = new Interpreter();

        static void Main(string[] args)
        {
            Run("file.pink");

            if (args.Length == 0)
            {
                RunPromt();
                return;
            }

            if (args.Length > 1)
            {
                Console.WriteLine("Usage: flow {file.fl}");
                return;
            }

            // Ensure the first argument is a valid file path
            string filePath = args[0];

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return;
            }
            else
            {
                Run(filePath);
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
            // 1️⃣ Сканируем
            Scanner scanner = new Scanner(content);
            List<Token> tokens = scanner.ScanTokens();

            //// 2️⃣ (Опционально) печатаем токены для отладки
            //foreach (var token in tokens)
            //{
            //    Console.ForegroundColor = ConsoleColor.DarkGray;
            //    Console.WriteLine(token);
            //    Console.ResetColor();
            //}

            // 3️⃣ Парсим
            Parser parser = new Parser(tokens);

            // Теперь Parse возвращает List<Statement>
            List<Statement> statements = parser.Parse();

            if (statements == null || statements.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Parsing failed.");
                Console.ResetColor();
                return;
            }

            // 4️⃣ Выполняем каждый Statement
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

            // hadError = true;
        }

        private static void RunPromt()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Pinkerton v0.1");
            Console.ResetColor();

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
