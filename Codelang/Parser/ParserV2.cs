using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codelang
{
    class ParserV2
    {
        public List<string> Diagnostics { get; set; }
        private int pos = 0;

        public ParserV2(List<Token> _tokens)
        {
            Tokens = _tokens;
        }

        private Token Peek(int offset)
        {
            int index = pos + offset;

            if (index >= Tokens.Count) return Tokens[Tokens.Count - 1];

            return Tokens[index];
        }

        private Token NextToken()
        {
            var current = Current;
            pos++;

            return current;
        }

        private Token Match(TokenTypes tokentype)
        {
            if (Current.TokenType == tokentype) return NextToken();

            return new Token(tokentype, null);
        }

        public List<SyntaxNode> Parse()
        {
            List<SyntaxNode> programTree = new List<SyntaxNode>();

            while(pos < Tokens.Count)
            {
                var z = ParseExpr();

                programTree.Add(z);

                if (z is EndStatementSyntax && ((EndStatementSyntax)z).value == "}")
                {
                    break;
                }

                if (z is EndStatementSyntax && ((EndStatementSyntax)z).value == "]") break;
            }

            programTree.RemoveAll((x) =>
            {
                return x is EndStatementSyntax;
            });

            return programTree;
        }

        public ExpressionSyntax ParseExpr()
        {
            var left = ParseFactor();

            while 
            ((Current.TokenType == TokenTypes.OPERATOR) && 
                (
                    Current.Value == "+" || Current.Value == "-"
                )
            )
            {
                var operatorToken = NextToken();
                var right = ParseFactor();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            if(left is NumberExpressionSyntax n)
            {
                if (n.NumberToken.Value == null)
                {
                    left = new EndStatementSyntax();
                    pos++;
                }
            }

            return left;
        }

        public ExpressionSyntax ParseFactor()
        {
            var left = ParsePrimaryExpression();

            while
            ((Current.TokenType == TokenTypes.OPERATOR) &&
                (
                    Current.Value == "*" || Current.Value == "/" ||
                    Current.Value == "==" || Current.Value == "!=" ||
                    Current.Value == ">" || Current.Value == "<" ||
                    Current.Value == ">=" || Current.Value == "<=" ||
                    Current.Value == "%"
                )
            )
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        private ExpressionSyntax ParseExpression()
        {
            return ParseExpr();
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            if(Current.TokenType == TokenTypes.LPAREN)
            {

                var left = NextToken();
                var expr = ParseExpression();
                var right = Match(TokenTypes.RPAREN);

                return new ParenthisizedExpressionSyntax(left, expr, right);
            }

            if (Current.TokenType == TokenTypes.KEYWORD)
            {
                if (Current.Value == "var")
                {
                    var varDecl = NextToken();
                    var name = NextToken();
                    var colon = NextToken();
                    var typeToken = ParseExpression();
                    var equalType = NextToken();
                    var value = ParseExpression();

                    return new DefineVariableSyntax(varDecl, name, colon, (DataType)typeToken, equalType, value);
                }
                else if (Current.Value == "func")
                {
                    var funcDecl = NextToken();
                    var lparen = NextToken();
                    var argsList = ParseExpression();
                    var rparen = NextToken();
                    var funcBlock = ParseExpression();

                    return new FunctionSyntax(funcDecl, lparen, (ArgListItemSyntax) argsList, rparen, (BlockSyntax)funcBlock);
                } 
                else if (Current.Value == "if")
                {
                    var ifDecl = NextToken();
                    var lparen = NextToken();
                    var controller = ParseExpression();
                    var rparen = NextToken();
                    var codeBlock = ParseExpression();

                    return new ControlFlowSyntax(ifDecl, lparen, controller, rparen, (BlockSyntax)codeBlock);
                }
                else if (Current.Value == "while")
                {
                    var whileDecl = NextToken();
                    var lparen = NextToken();
                    var controller = ParseExpression();
                    var rparen = NextToken();
                    var codeBlock = ParseExpression();

                    return new ControlFlowSyntax(whileDecl, lparen, controller, rparen, (BlockSyntax)codeBlock);
                }
            }

            if (Current.TokenType == TokenTypes.IDENTIFIER)
            {
                var name = NextToken();
                var reassign = Peek(0);

                if (name.Value == "int" || name.Value == "string" || name.Value == "bool" || name.Value == "void" || name.Value == "none")
                {
                    if (reassign.TokenType == TokenTypes.LBRACKET)
                    {
                        return new DataType(name, NextToken(), NextToken());
                    }

                    return new DataType(name);
                }

                if (reassign.TokenType == TokenTypes.EQUAL)
                {
                    pos++;
                    var value = ParseExpression();
                    return new AssignVariableSyntax(name, reassign, value);
                }

                else if (reassign.TokenType == TokenTypes.LPAREN)
                {
                    var lparen = NextToken();
                    var param = ParseExpression();
                    var rparen = NextToken();

                    return new CallFunctionSyntax(name, lparen, param, rparen);
                }

                else if (reassign.TokenType == TokenTypes.COLON)
                {
                    var colon = NextToken();
                    var typeToken = NextToken();

                    return new ArgListItemSyntax(name, colon, typeToken);
                }

                else if (reassign.TokenType == TokenTypes.LBRACKET)
                {
                    return new IndexListSyntax(new UseVariableSyntax(name), NextToken(), ParseExpression(), NextToken());
                }

                return new UseVariableSyntax(name);
            }

            if (Current.TokenType == TokenTypes.LBRACE)
            {
                var lbrace = NextToken();
                
                List<SyntaxNode> statements = Parse();

                var rbrace = new Token(TokenTypes.RBRACE, "}");

                return new BlockSyntax(lbrace, statements, rbrace);
            }

            if (Current.TokenType == TokenTypes.LBRACKET)
            {
                var lbracket = NextToken();

                List<SyntaxNode> statements = Parse();

                var rbracket = new Token(TokenTypes.RBRACE, "]");

                return new ListSyntax(lbracket, statements, rbracket);
            }

            if(Current.TokenType == TokenTypes.STRING)
            {
                return new StringExpressionSyntax(NextToken());
            }

            if(Current.TokenType == TokenTypes.BOOLEAN)
            {
                return new BooleanExpressionSyntax(NextToken());
            }

            if (Current.TokenType == TokenTypes.RBRACE)
            {
                pos++;
                return new EndStatementSyntax("}");
            }

            if (Current.TokenType == TokenTypes.RBRACKET)
            {
                pos++;
                return new EndStatementSyntax("]");
            }

            if (Current.TokenType == TokenTypes.COMMA)
            {
                pos++;
                return new EndStatementSyntax(",");
            }


            var numberToken = Match(TokenTypes.NUMBER);

            return new NumberExpressionSyntax(numberToken);
        }

        private Token Current => Peek(0);

        public List<Token> Tokens { get; private set; }
    }

    abstract class SyntaxNode
    {
        public abstract TokenTypes SyntaxType { get; }

        public abstract IEnumerable<SyntaxNode> GetChildren();
    }

    abstract class ExpressionSyntax : SyntaxNode
    {

    }

    sealed class BlockSyntax : ExpressionSyntax
    {
        public BlockSyntax(Token lBrace, List<SyntaxNode> statements, Token rBrace)
        {
            LBrace = lBrace;
            Statements = statements;
            RBrace = rBrace;
        }

        public override TokenTypes SyntaxType => TokenTypes.Block;

        public Token LBrace { get; }
        public List<SyntaxNode> Statements { get; }
        public Token RBrace { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return null;
        }
    }

    sealed class ListSyntax : ExpressionSyntax
    {
        public ListSyntax(Token lBracket, List<SyntaxNode> statements, Token rBracket)
        {
            LBracket = lBracket;
            Statements = statements;
            RBracket = rBracket;
        }

        public override TokenTypes SyntaxType => TokenTypes.ListBlock;

        public Token LBracket { get; }
        public List<SyntaxNode> Statements { get; }
        public Token RBracket { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return null;
        }
    }

    sealed class EndStatementSyntax : ExpressionSyntax
    {
        public string value;

        public EndStatementSyntax(string v = "")
        {
            value = v;
        }

        public override TokenTypes SyntaxType => TokenTypes.EndExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        { yield return null; }
    }

    sealed class NumberExpressionSyntax : ExpressionSyntax
    {
        public NumberExpressionSyntax(Token token)
        {
            NumberToken = token;
        }

        public override TokenTypes SyntaxType => TokenTypes.NumberExpression;
        public Token NumberToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NumberToken;
        }
    }

    sealed class StringExpressionSyntax : ExpressionSyntax
    {
        public StringExpressionSyntax(Token token)
        {
            StringToken = token;
        }

        public override TokenTypes SyntaxType => TokenTypes.StringExpression;
        public Token StringToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return StringToken;
        }
    }

    sealed class BooleanExpressionSyntax : ExpressionSyntax
    {
        public BooleanExpressionSyntax(Token token)
        {
            BooleanToken = token;
        }

        public override TokenTypes SyntaxType => TokenTypes.BooleanExpression;

        public Token BooleanToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            throw new NotImplementedException();
        }
    }

    sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax (ExpressionSyntax left, Token operatorToken, ExpressionSyntax right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }

        public ExpressionSyntax Left { get; private set; }
        public Token OperatorToken { get; private set; }
        public ExpressionSyntax Right { get; }

        public override TokenTypes SyntaxType => TokenTypes.BinaryExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }

    sealed class ParenthisizedExpressionSyntax : ExpressionSyntax
    {
        public ParenthisizedExpressionSyntax(Token lParen, ExpressionSyntax expression, Token rParen)
        {
            LParen = lParen;
            Expression = expression;
            RParen = rParen;
        }

        public Token RParen { get; }
        public ExpressionSyntax Expression { get; }
        public Token LParen { get; }

        public override TokenTypes SyntaxType => TokenTypes.ParenthisizedExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LParen;
            yield return Expression;
            yield return RParen;
        }
    }

    sealed class DataType : ExpressionSyntax
    {
        public override TokenTypes SyntaxType => TokenTypes.DataType;

        public Token TypeName { get; }
        public Token LBracket { get; }
        public Token RBracket { get; }

        public DataType(Token typeName, Token lBracket = null, Token rBracket = null)
        {
            TypeName = typeName;
            LBracket = lBracket;
            RBracket = rBracket;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return TypeName;
            yield return LBracket;
            yield return RBracket;
        }
    }

    sealed class DefineVariableSyntax : ExpressionSyntax
    {
        public DefineVariableSyntax (Token variableToken, Token nameToken, Token typeColon, DataType dataType, Token equalType, ExpressionSyntax valueExpression)
        {
            VariableToken = variableToken;
            TypeColon = typeColon;
            Datatype = dataType;
            NameToken = nameToken;
            EqualType = equalType;
            ValueExpression = valueExpression;
        }

        public override TokenTypes SyntaxType => TokenTypes.DefineVariableSyntax;

        public Token VariableToken { get; }
        public Token TypeColon { get; }
        public DataType Datatype { get; }
        public Token NameToken { get; }
        public Token EqualType { get; }
        public ExpressionSyntax ValueExpression { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NameToken;
            yield return Datatype;
            yield return ValueExpression;
        }
    }

    sealed class AssignVariableSyntax : ExpressionSyntax
    {
        public AssignVariableSyntax(Token nameToken, Token equalToken, ExpressionSyntax valueExpr)
        {
            NameToken = nameToken;
            EqualToken = equalToken;
            ValueExpression = valueExpr;
        }

        public override TokenTypes SyntaxType => TokenTypes.AssignVariableExpression;

        public Token NameToken { get; }
        public Token EqualToken { get; }
        public ExpressionSyntax ValueExpression { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NameToken;
            yield return ValueExpression;
        }
    }

    sealed class UseVariableSyntax : ExpressionSyntax
    {
        public UseVariableSyntax(Token nameToken)
        {
            NameToken = nameToken;
        }

        public Token NameToken { get; }

        public override TokenTypes SyntaxType => TokenTypes.UseVariableExpression;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NameToken;
        }
    }

    sealed class CallFunctionSyntax : ExpressionSyntax
    {
        public CallFunctionSyntax(Token nameToken, Token lParen, ExpressionSyntax paramList, Token rParen)
        {
            NameToken = nameToken;
            LParen = lParen;
            Param = paramList;
            RParen = rParen;
        }

        public override TokenTypes SyntaxType => TokenTypes.CallFunctionExpression;

        public Token NameToken { get; }
        public Token LParen { get; }
        public ExpressionSyntax Param { get; }
        public Token RParen { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return NameToken;
            yield return Param;
        }
    }

    sealed class ArgListSyntax : ExpressionSyntax
    {
        public override TokenTypes SyntaxType => TokenTypes.ArgListExpression;

        public List<SyntaxNode> ArgList { get; }

        public ArgListSyntax (List<SyntaxNode> argList)
        {
            ArgList = argList;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            throw new NotImplementedException();
        }
    }

    sealed class ArgListItemSyntax : ExpressionSyntax
    {
        public override TokenTypes SyntaxType => TokenTypes.ArgListItemExpression;

        public Token VariableName { get; }
        public Token Colon { get; }
        public Token TypeName { get; }

        public ArgListItemSyntax(Token variableName, Token colon, Token typeName)
        {
            VariableName = variableName;
            Colon = colon;
            TypeName = typeName;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return VariableName;
            yield return TypeName;
        }
    }

    sealed class FunctionSyntax : ExpressionSyntax
    {
        public override TokenTypes SyntaxType => TokenTypes.FunctionExpression;

        public Token FuncKeyword { get; }
        public Token LParen { get; }
        public ArgListItemSyntax ArgList { get; }
        public Token RParen { get; }
        public BlockSyntax FunctionBlock { get; }

        public FunctionSyntax(Token funcKeyword, Token lParen, ArgListItemSyntax argList, Token rParen, BlockSyntax functionBlock)
        {
            FuncKeyword = funcKeyword;
            LParen = lParen;
            ArgList = argList;
            RParen = rParen;
            FunctionBlock = functionBlock;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return FunctionBlock;
        }
    }

    sealed class ControlFlowSyntax : ExpressionSyntax
    {
        public override TokenTypes SyntaxType => TokenTypes.ControlFlowSyntax;

        public Token Keyword { get; }
        public Token LParen { get; }
        public ExpressionSyntax Controller { get; }
        public Token RParen { get; }
        public BlockSyntax StatementBlock { get; }

        public ControlFlowSyntax (Token keyword, Token lParen, ExpressionSyntax controller, Token rParen, BlockSyntax statementBlock)
        {
            Keyword = keyword;
            LParen = lParen;
            Controller = controller;
            RParen = rParen;
            StatementBlock = statementBlock;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Controller;
            yield return StatementBlock;
        }
    }

    sealed class IndexListSyntax : ExpressionSyntax
    {
        public override TokenTypes SyntaxType => TokenTypes.IndexListExpression;

        public ExpressionSyntax Expression { get; }
        public Token LBracket { get; }
        public ExpressionSyntax Index { get; }
        public Token RBracket { get; }

        public IndexListSyntax(ExpressionSyntax expression, Token lBracket, ExpressionSyntax index, Token rBracket)
        {
            Expression = expression;
            LBracket = lBracket;
            Index = index;
            RBracket = rBracket;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
            yield return Index;
        }
    }
}
