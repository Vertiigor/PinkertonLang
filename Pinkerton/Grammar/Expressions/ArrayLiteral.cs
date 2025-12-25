namespace PinkertonInterpreter.Grammar.Expressions
{
    internal record ArrayLiteral(List<Expression> Elements) : Expression;
}
