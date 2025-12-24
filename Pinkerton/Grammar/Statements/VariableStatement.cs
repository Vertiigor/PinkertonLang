namespace PinkertonInterpreter.Grammar.Statements
{
    internal record VariableStatement(Token Name, Expression? Initializer) : Statement;
}
