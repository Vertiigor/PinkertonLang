namespace PinkertonInterpreter.Grammar.Statements
{
    internal record PrintStatement(Expression value) : Statement;
    internal record PrintLnStatement(Expression value) : Statement;
}