namespace PinkertonInterpreter.Grammar.Statements
{
    internal record FunctionStatement(
    Token Name,
    List<Token> Parameters,
    List<Statement> Body
) : Statement;
}
