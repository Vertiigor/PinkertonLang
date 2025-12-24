namespace PinkertonInterpreter.Grammar.Expressions
{
    internal record Unary(Token Operator, Expression Right) : Expression
    {
        public override string ToString()
        {
            return $"Unary(Operator: {Operator}, Right: {Right})";
        }
    }
}
