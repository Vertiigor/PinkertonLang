using PinkertonInterpreter.Grammar;

namespace Interpreter.Grammar.Statements
{
    internal record BlockStatement(List<Statement> Statements) : Statement;
}
