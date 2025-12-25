namespace PinkertonInterpreter.Grammar.Statements
{
    internal record IfStatement(Expression Condition, Statement ThenBranch, Statement? ElseBranch) : Statement;
}
