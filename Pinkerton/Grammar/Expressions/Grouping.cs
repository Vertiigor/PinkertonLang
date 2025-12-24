namespace PinkertonInterpreter.Grammar.Expressions
{
    internal record Grouping(Expression Inner) : Expression
    {
        public override string ToString()
        {
            return $"Grouping(Expression: {Inner})";
        }
    }
}
