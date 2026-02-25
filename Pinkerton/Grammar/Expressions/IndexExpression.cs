namespace PinkertonInterpreter.Grammar.Expressions
{
    internal record IndexExpression(Expression Target, Expression Index) : Expression;
}
