namespace PinkertonInterpreter.Grammar.Statements
{
    internal record ForLoopStatement(Statement Iterator,
                                 Expression Start,
                                 Expression End,
                                 Statement Body) : Statement;
}
