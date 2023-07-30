using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codelang
{
    class Token : SyntaxNode
    {
        public TokenTypes TokenType { get; set; }
        public string Value { get; set; }


        public Token(TokenTypes _tokenType, string _value)
        {
            TokenType = _tokenType;
            Value = _value;
        }

        public override string ToString()
        {
            return $"{TokenType} -> {Value}";
        }

        public override TokenTypes SyntaxType => TokenType;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }
    }
    
    enum TokenTypes
    {
        IDENTIFIER,
        KEYWORD,
        NUMBER,
        STRING,
        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,
        SEMICOLON,
        WS,
        EQUAL,
        QUOTE,
        HASHTAG,
        COMMA,
        DOT,
        OPERATOR,
        COLON,
        UNKNOWN,
        BOOLEAN,
        RBRACKET,
        LBRACKET,
        NumberExpression,
        BinaryExpression,
        ParenthisizedExpression,
        DefineVariableSyntax,
        UseVariableExpression,
        AssignVariableExpression,
        StringExpression,
        CallFunctionExpression,
        ParamListExpression,
        FunctionExpression,
        EndExpression,
        Block,
        BooleanExpression,
        ArgListItemExpression,
        ArgListExpression,
        ControlFlowSyntax,
        ForLoopExpression,
        DataType,
        ListBlock,
        IndexListExpression
    }
}
