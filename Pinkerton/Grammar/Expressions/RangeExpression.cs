namespace PinkertonInterpreter.Grammar.Expressions
{
    internal record RangeExpression(Expression left, Expression right, Expression? step) : Expression;
}
