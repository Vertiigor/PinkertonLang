using PinkertonInterpreter.Grammar;

namespace PinkertonInterpreter.Grammar.Statements
{
    internal record WhileLoopStatement(Expression Condition, Statement Body) : Statement;
}
