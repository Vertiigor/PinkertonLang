using PinkertonInterpreter.Grammar;


namespace PinkertonInterpreter.Grammar.Expressions
{
    internal record InExpression(Expression Left, Expression Right) : Expression;
}
