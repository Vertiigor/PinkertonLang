namespace PinkertonInterpreter.Grammar.Expressions
{
    internal record Literal(object Value) : Expression
    {
        public override string ToString()
        {
            return $"Literal(Value: {Value})";
        }
    }
}
