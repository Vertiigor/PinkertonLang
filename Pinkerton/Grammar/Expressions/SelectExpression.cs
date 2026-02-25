namespace PinkertonInterpreter.Grammar.Expressions
{
    internal record SelectExpression(Expression Condition, Expression ThenExpression, Expression ElseExpression) : Expression;
}
