namespace Monkey.Tests;

using Xunit;

using Monkey.Lexing;
using Monkey.Parsing;

public class ParserTest
{
    [Fact]
    public void ParsingLetStatements()
    {
        string input = @"
            let x = 5;
            let y = 10;
            let foobar = 838383;
        ";

        List<string> expectedIdentifiers = new() {
            "x", "y", "foobar"
        };

        Lexer lexer = new(input);
        List<Token> tokens = lexer.TokenizeProgram();
        Parser parser = new(tokens);

        Ast program = parser.ParseProgram();
        Assert.NotNull(program);
        Assert.False(
            parser.Errors.Any(), 
            string.Join("\n".PadRight(4), parser.Errors)
        );
        Assert.Equal(3, program.Statements.Count());
        
        for (int i = 0; i < expectedIdentifiers.Count(); i++)
        {
            IStatement statement = program.Statements[i];
            string expectedIdentifier = expectedIdentifiers[i];

            LetStatement letStatement = Assert.IsType<LetStatement>(statement);
            Assert.IsType<LetToken>(letStatement.Token);
            Assert.IsType<IdentToken>(letStatement.Name.Token);
            Assert.Equal(expectedIdentifier, letStatement.Name.Value);
            Assert.Equal(expectedIdentifier, letStatement.Name.TokenLiteral());

        }
    }

    [Fact]
    public void ParsingReturnStatements()
    {
        string input = @"
            return 5;
            return 10;
            return 993322;
        ";

        Lexer lexer = new(input);
        List<Token> tokens = lexer.TokenizeProgram();
        Parser parser = new(tokens);

        Ast program = parser.ParseProgram();
        Assert.NotNull(program);
        Assert.False(
            parser.Errors.Any(), 
            string.Join("\n".PadRight(4), parser.Errors)
        );
        Assert.Equal(3, program.Statements.Count());
        
        foreach (IStatement statement in program.Statements)
        {
            ReturnStatement returnStatement = Assert.IsType<ReturnStatement>(statement);
            Assert.IsType<ReturnToken>(returnStatement.Token);
        }
    }

    [Fact]
    public void ParsingIdentifierExpression()
    {
        string input = "foobar";

        Lexer lexer = new(input);
        List<Token> tokens = lexer.TokenizeProgram();
        Parser parser = new(tokens);
        Ast program = parser.ParseProgram();
        
        Assert.NotNull(program);
        Assert.False(
            parser.Errors.Any(), 
            string.Join("\n".PadRight(4), parser.Errors)
        );
        Assert.Single(program.Statements);
        
        IStatement statement = program.Statements[0];
        ExpressionStatement expressionStatement = 
            Assert.IsType<ExpressionStatement>(statement);

        IdentifierExpression identifierExpression = 
            Assert.IsType<IdentifierExpression>(expressionStatement.Expression);

        Assert.Equal("foobar", identifierExpression.Value);
        Assert.Equal("foobar", identifierExpression.TokenLiteral());
    }

    [Fact]
    public void ParsingIntegerExpression()
    {
        string input = "5;";

        Lexer lexer = new(input);
        List<Token> tokens = lexer.TokenizeProgram();
        Parser parser = new(tokens);
        Ast program = parser.ParseProgram();
        
        Assert.NotNull(program);
        Assert.False(
            parser.Errors.Any(), 
            string.Join("\n".PadRight(4), parser.Errors)
        );
        Assert.Single(program.Statements);
        
        IStatement statement = program.Statements[0];
        ExpressionStatement expressionStatement = 
            Assert.IsType<ExpressionStatement>(statement);

        IntegerExpression integerExpression = 
            Assert.IsType<IntegerExpression>(expressionStatement.Expression);

        Assert.Equal(5, integerExpression.Value);
        Assert.Equal("5", integerExpression.TokenLiteral());
    }

    [Fact]
    public void ParsingPrefixExpression()
    {
        List<(string, string, long)> prefixTests = new()
        {
            ("!5;", "!", 5),
            ("-15;", "-", 15),
        };

        foreach ((string input, string op, long value) in prefixTests)
        {
            Lexer lexer = new(input);
            List<Token> tokens = lexer.TokenizeProgram();
            Parser parser = new(tokens);
            Ast program = parser.ParseProgram();
            
            Assert.NotNull(program);
            Assert.False(
                parser.Errors.Any(), 
                string.Join("\n".PadRight(4), parser.Errors)
            );
            Assert.Single(program.Statements);
            
            IStatement statement = program.Statements[0];
            ExpressionStatement expressionStatement = 
                Assert.IsType<ExpressionStatement>(statement);

            PrefixExpression prefixExpression = 
                Assert.IsType<PrefixExpression>(expressionStatement.Expression);

            Assert.Equal(op, prefixExpression.Operator);

            IntegerExpression integerExpression = 
                Assert.IsType<IntegerExpression>(prefixExpression.Right);

            Assert.Equal(value, integerExpression.Value);
            Assert.Equal(value.ToString(), integerExpression.TokenLiteral());
        }
    }
}
