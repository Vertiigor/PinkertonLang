namespace PinkertonInterpreter
{
    internal enum TokenType
    {
        // Single-character tokens
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE, LEFT_BRACKET, RIGHT_BRACKET,
        COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR, MOD, CONCAT,

        // One or two character tokens
        BANG, AINTSO,
        EQUAL, EQUAL_EQUAL,
        GREATER, GREATER_EQUAL,
        LESS, LESS_EQUAL,

        // Literals
        IDENTIFIER, STRING, INTEGER, FLOAT, DOUBLE, BOOL, CHAR, DATE, DECIMAL, BYTE, NUMBER,

        // Keywords
        VAR, AS, INT, DOUBLE_KW, STRING_KW, FLOAT_KW, BOOL_KW,
        IF, THEN, WHILE, FOR, ELSE, RANGE,
        FUNCTION, CHAR_KW,
        NULL, AND, OR, TRUE, FALSE, NOT,
        ARRAY,
        RETURN, DO, PROCEDURE, BREAK, CONTINUE,
        PRINT, PRINTLN, SELECT, IN, STEP,

        GROUP,

        // End of file
        EOF
    }

    internal class Token
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public object Literal { get; }
        public int Line { get; }

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override string ToString()
        {
            return $"Type: {Type}\t\t\tLexeme: {Lexeme}\t\t\tLiteral: {Literal}";
        }
    }
}
