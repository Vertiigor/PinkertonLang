namespace PinkertonInterpreter.Grammar.Expressions
{
    internal record InExpression(Expression Left, Expression Right) : Expression;
}
