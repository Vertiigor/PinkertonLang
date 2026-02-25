using System.Globalization;

namespace PinkertonInterpreter
{
    internal class Scanner
    {
        private readonly Dictionary<string, TokenType> _keywords;
        private readonly List<Token> _tokens = new List<Token>();
        private readonly string _source;

        private int start = 0;
        private int current = 0;
        private int line = 1;

        private bool isAtEnd => current >= _source.Length;

        private char Peek => isAtEnd ? EOF : _source[current];
        private char PeekNext => (current + 1) >= _source.Length ? EOF : _source[current + 1];

        private const char EOF = '\0';

        public Scanner(string source)
        {
            _source = source;
            _tokens = new List<Token>();
            _keywords = new Dictionary<string, TokenType>
            {
                { "let", TokenType.VAR },
                { "int", TokenType.INT },
                { "double", TokenType.DOUBLE_KW },
                { "string", TokenType.STRING_KW },
                { "float", TokenType.FLOAT_KW },
                { "bool", TokenType.BOOL_KW },
                { "if", TokenType.IF },
                { "while", TokenType.WHILE },
                { "for", TokenType.FOR },
                { "else", TokenType.ELSE },
                { "function", TokenType.FUNCTION },
                { "char", TokenType.CHAR_KW },
                { "null", TokenType.NULL },
                { "and", TokenType.AND },
                { "or", TokenType.OR },
                { "true", TokenType.TRUE },
                { "false", TokenType.FALSE },
                { "not", TokenType.NOT },
                { "array", TokenType.ARRAY },
                { "return", TokenType.RETURN },
                { "do", TokenType.DO },
                { "procedure", TokenType.PROCEDURE },
                { "break", TokenType.BREAK },
                { "again", TokenType.CONTINUE },
                { "write", TokenType.PRINT },
                { "writeln", TokenType.PRINTLN },
                { "then", TokenType.THEN },
                { "isnt", TokenType.AINTSO  },
                { "group", TokenType.GROUP },
                { "begin", TokenType.LEFT_BRACE  },
                { "end", TokenType.RIGHT_BRACE },
                { "select", TokenType.SELECT },
                { "mod", TokenType.MOD },
                { "in", TokenType.IN   },
                { "step", TokenType.STEP },
                };
        }

        public List<Token> ScanTokens()
        {
            while (!isAtEnd)
            {
                start = current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, line));

            return _tokens;
        }

        private static bool IsIdentifierChar(char c)
        {
            return
                char.IsLetterOrDigit(c)
                || c == '_'
                || char.GetUnicodeCategory(c) == UnicodeCategory.OtherLetter;
        }


        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case '[': AddToken(TokenType.LEFT_BRACKET); break;
                case ']': AddToken(TokenType.RIGHT_BRACKET); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '/': AddToken(TokenType.SLASH); break;
                case '^': AddToken(TokenType.CONCAT); break;
                case '$': SkipLine(); break; // Skip comments starting with $
                case '\n': line++; break;
                case '!':
                    AddToken(Match('=') ? TokenType.AINTSO : TokenType.BANG);
                    break;
                case '=':
                    AddToken(TokenType.EQUAL_EQUAL);
                    break;
                case '<':
                    AddToken(Match('-') ? TokenType.EQUAL : (Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS));
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '.':
                    AddToken(Match('.') ? TokenType.RANGE : TokenType.DOT);
                    break;
                case '"': String(); break;
                case '\'':
                    CharLiteral();
                    break;
                case ':':
                    AddToken(Match('=') ? TokenType.EQUAL : TokenType.AS);
                    break;
                default:
                    if (char.IsDigit(c)) Number();
                    else if (IsIdentifierChar(c)) Identifier();
                    else if (char.IsWhiteSpace(c)) { /* Ignore whitespace */ }
                    else throw new Exception($"Unexpected character: {c}");
                    break;
            }
        }

        private void Identifier()
        {
            while (IsIdentifierChar(Peek))
                Advance();

            string text = _source.Substring(start, current - start);

            if (!_keywords.TryGetValue(text, out var type))
                type = TokenType.IDENTIFIER;

            AddToken(type);
        }


        private void Number()
        {
            while (char.IsDigit(Peek)) Advance();

            if (Peek == '.' && char.IsDigit(PeekNext))
            {
                Advance(); // Consume the '.'

                while (char.IsDigit(Peek)) Advance();
            }

            string value = _source.Substring(start, current - start);

            AddToken(TokenType.NUMBER, double.Parse(value));
        }

        private void String()
        {
            while (true)
            {
                if (isAtEnd) throw new Exception("Unterminated string.");

                char c = Advance();

                if (c == '"') break;
                if (c == '\n') line++;
            }
            string value = _source.Substring(start + 1, current - start - 2); // Exclude the quotes

            AddToken(TokenType.STRING, value);
        }

        private void CharLiteral()
        {
            if (isAtEnd)
                throw new Exception("Unterminated char literal.");

            char value = Advance();

            // поддержка escape-последовательностей
            if (value == '\\')
            {
                char esc = Advance();
                value = esc switch
                {
                    'n' => '\n',
                    't' => '\t',
                    'r' => '\r',
                    '\\' => '\\',
                    '\'' => '\'',
                    _ => throw new Exception($"Unknown escape: \\{esc}")
                };
            }

            if (Advance() != '\'')
                throw new Exception("Char literal must contain exactly one character.");

            AddToken(TokenType.CHAR, value);
        }


        private void SkipLine()
        {
            while (!isAtEnd && _source[current] != '\n')
            {
                current++;
            }

            line++;
        }

        private bool Match(char c)
        {
            if (isAtEnd || _source[current] != c) return false;

            current++;

            return true;
        }

        private void AddToken(TokenType tokenType)
        {
            AddToken(tokenType, null);
        }

        private void AddToken(TokenType tokenType, object value)
        {
            string text = _source.Substring(start, current - start);

            _tokens.Add(new Token(tokenType, text, value, line));
        }

        private char Advance()
        {
            if (isAtEnd) return EOF;
            char c = _source[current];

            current++;

            return c;
        }
    }
}
