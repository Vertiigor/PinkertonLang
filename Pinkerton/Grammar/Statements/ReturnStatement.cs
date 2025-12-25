namespace PinkertonInterpreter.Grammar.Statements
{
    internal record ReturnStatement(Expression Value) : Statement;

    public class ReturnException : Exception
    {
        public object? Value { get; }

        public ReturnException(object? value)
        {
            Value = value;
        }
    }

}
