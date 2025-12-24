namespace PinkertonInterpreter.Grammar.Expressions
{
    internal record Binary(Expression Left, Token Operator, Expression Right) : Expression;
}
