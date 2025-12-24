using PinkertonInterpreter.Grammar;
using PinkertonInterpreter.Grammar.Expressions;

namespace PinkertonInterpreter
{
    internal class AstPrinter
    {
        public static string Print(Expression expr) => expr switch
        {
            Binary(var left, var op, var right) =>
                Parenthesize(op, left, right),

            Grouping(var inner) =>
                Parenthesize(new Token(TokenType.GROUP, "group", string.Empty, 0), inner),

            Literal(var value) =>
                value?.ToString() ?? "nil",

            Unary(var op, var right) =>
                Parenthesize(op, right),

            _ => throw new Exception("Unknown expression")
        };

        private static string Parenthesize(Token name, params Expression[] exprs)
        {
            var parts = exprs.Select(Print);

            return $"({name.Lexeme} {string.Join(" ", parts)})";
        }
    }
}
