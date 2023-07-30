using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codelang
{
	class Lexer
	{
		// Properties
		public string Code;
		public const string ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
		public const string DIGITS = "0123456789";
		// Creating the Keywords
		public string[] Keywords = { "var", "func", "if", "while", "for" };
		// boolean to get unknown chars
		private bool Taken;

		public Lexer(string _code)
		{
			// initialize the class
			Code = _code;
		}

		public List<Token> CreateTokens()
		{
			// Create variables
			List<Token> tokens = new List<Token>();
			Token currToken = new Token(TokenTypes.WS, "");
			Token prevToken = new Token(TokenTypes.WS, "");
			int pos = 0;

			// Check each character in the code
			while (pos < Code.Length)
			{
				char c = Code[pos];

				// A lot of repetitive code but i'll try to fix it later
				currToken.TokenType = CheckChar(currToken.TokenType, DIGITS.Contains(c.ToString()), TokenTypes.NUMBER);
				currToken.TokenType = CheckChar(currToken.TokenType, ALPHABET.Contains(c.ToString()), TokenTypes.IDENTIFIER);
				currToken.TokenType = CheckChar(currToken.TokenType, c == ' ' || c == '\n' || c == '\t' || c == '\r', TokenTypes.WS);
				currToken.TokenType = CheckChar(currToken.TokenType, c == ';', TokenTypes.SEMICOLON);
				currToken.TokenType = CheckChar(currToken.TokenType, c == '(', TokenTypes.LPAREN);
				currToken.TokenType = CheckChar(currToken.TokenType, c == ')', TokenTypes.RPAREN);
				currToken.TokenType = CheckChar(currToken.TokenType, c == '{', TokenTypes.LBRACE);
				currToken.TokenType = CheckChar(currToken.TokenType, c == '}', TokenTypes.RBRACE);
				currToken.TokenType = CheckChar(currToken.TokenType, c == '[', TokenTypes.LBRACKET);
				currToken.TokenType = CheckChar(currToken.TokenType, c == ']', TokenTypes.RBRACKET);
				currToken.TokenType = CheckChar(currToken.TokenType, c == '"', TokenTypes.QUOTE);
				currToken.TokenType = CheckChar(currToken.TokenType, c == ':', TokenTypes.COLON);
				currToken.TokenType = CheckChar(currToken.TokenType, c == '#', TokenTypes.HASHTAG);
				currToken.TokenType = CheckChar(currToken.TokenType, c == '.', TokenTypes.DOT);
				currToken.TokenType = CheckChar(currToken.TokenType, c == ',', TokenTypes.COMMA);
				currToken.TokenType = CheckChar(currToken.TokenType, 
					c == '+' || 
					c == '-' || 
					c == '/' || 
					c == '*' || 
					c == '^' ||
					c == '>' ||
					c == '<' ||
					c == '=' ||
					c == '!' ||
					c == '%' 
				, TokenTypes.OPERATOR);

				if(!Taken)
                {
					currToken.TokenType = TokenTypes.UNKNOWN;
                }

				// Check if we are adding onto the previous token or adding it to the list
				if (currToken.TokenType == prevToken.TokenType)
				{
					if(currToken.TokenType == TokenTypes.QUOTE) 
					{
						tokens.Add(new Token(TokenTypes.STRING, ""));
						currToken = new Token(TokenTypes.WS, "");
					}
					else
                    {
						currToken.Value += c;
					}
				}
				else
				{
					tokens.Add(prevToken);
					currToken.Value = c.ToString();
				}

				// Setting the previous token to the current one
				// note: do not use "prevToken = currToken" because it generates an error
				prevToken = new Token(currToken.TokenType, currToken.Value);
				Taken = false;
				// Continuing the loop
				pos++;
			}
			// Adding the final token
			tokens.Add(currToken);
			tokens.Add(new Token(TokenTypes.SEMICOLON, ";"));

			// Make Strings, Comments, and floats
			tokens = GenStrings(tokens);
			tokens = GenCommments(tokens);
			tokens = GenFloats(tokens);
			tokens = GenOperators(tokens);
			tokens = GenParens(tokens);

			// Filter
			List<Token> ftokens = new List<Token>();
			foreach (Token token in tokens)
			{
				if (token.TokenType == TokenTypes.UNKNOWN) new ErrorHandler($"Error: Unknown token ({token.ToString()}) at CHAR({pos})");
				// Remove all "WS" tokens
				if (
					token.TokenType != TokenTypes.WS
				)
				{
					// Change some identifiers to keywords
					foreach (string keyword in Keywords)
					{
						if (token.Value == keyword)
							token.TokenType = TokenTypes.KEYWORD;
					}

					if (token.Value == "true" || token.Value == "false")
                    {
						token.TokenType = TokenTypes.BOOLEAN;
					}

					ftokens.Add(token);
				}
			}

			return ftokens;
		}

        private List<Token> GenParens(List<Token> tokens)
        {
			List<Token> newTokens = new List<Token>();

			int i = 0;

			while (i < tokens.Count)
            {
				if(tokens[i].TokenType == TokenTypes.LPAREN)
                {
					var amount = tokens[i].Value.Length;

					for (int j = 0; j < amount; j++)
                    {
						newTokens.Add(new Token(TokenTypes.LPAREN, "("));
                    }
                }
				else if (tokens[i].TokenType == TokenTypes.RPAREN)
				{
					var amount = tokens[i].Value.Length;

					for (int j = 0; j < amount; j++)
					{
						newTokens.Add(new Token(TokenTypes.RPAREN, ")"));
					}
				}
				else
                {
					newTokens.Add(tokens[i]);
                }

				i++;
			}

			return newTokens;
        }

        private List<Token> GenOperators(List<Token> tokens)
        {
			List<Token> newTokens = new List<Token>();
			int i = 0;

            while (i < tokens.Count)
            {
				Token token = tokens[i];

				if(token.TokenType == TokenTypes.OPERATOR)
                {
					if (token.Value == "=")
                    {
						token.TokenType = TokenTypes.EQUAL;
                    }
                }

				i++;

				newTokens.Add(token);
            }

			return newTokens;
        }

        private List<Token> GenFloats(List<Token> tokens)
        {
			List<Token> newTokens = new List<Token>();
			int i = 0;

            while (i < tokens.Count)
            {
				Token token = tokens[i];

				if(token.TokenType == TokenTypes.NUMBER)
                {
					if(i + 1 < tokens.Count)
                    {
						if (tokens[i + 1].TokenType == TokenTypes.DOT)
						{
							token = new Token(TokenTypes.NUMBER, $"{tokens[i].Value}.{tokens[i + 2].Value}");
							i += 3;
						}
						else
						{
							i++;
						}
					}
                }
				else
                {
					i++;
                }

				newTokens.Add(token);
            }

			return newTokens;
        }

        private List<Token> GenStrings(List<Token> tokens)
        {
			// Variables
			bool makingString = false;
			string strValue = "";
			List<Token> newTokens = new List<Token>();

			// Loop through tokens
			for (var i = 0; i<tokens.Count; i++)
			{
				Token token = tokens[i];

				// switch the boolean if we encounter a '"'
				if (token.TokenType == TokenTypes.QUOTE)
				{
					switch (makingString)
					{
						case false:
							makingString = true;
							if(tokens[i + 1].TokenType == TokenTypes.QUOTE)
                            {
								makingString = false;
								newTokens.Add(new Token(TokenTypes.STRING, ""));
							}
							break;
						default:
							// Adding the string to list of tokens
							makingString = false;
                            newTokens.Add(new Token(TokenTypes.STRING, strValue));
							strValue = "";
							break;
					}
				}
				else { }

				// Adding the tokens to the string if we are making a string
				if (makingString && token.TokenType != TokenTypes.QUOTE)
				{
					strValue += token.Value;
				}
				// Adding the other tokens
				else if (!makingString && token.TokenType != TokenTypes.QUOTE)
				{
					newTokens.Add(token);
				}
			}

			return newTokens;
		}

		private List<Token> GenCommments(List<Token> tokens)
		{
			// Making the variables
			bool makingComment = false;
			List<Token> newTokens = new List<Token>();

			// Looping through the tokens
			foreach (Token token in tokens)
			{
				// Switching the boolean if we encounter a hashtag
				if (token.TokenType == TokenTypes.HASHTAG)
				{
					makingComment = !makingComment;
				}

				if (!makingComment && token.TokenType != TokenTypes.HASHTAG)
				{
					newTokens.Add(token);
				}
			}

			return newTokens;
		}

		private TokenTypes CheckChar(TokenTypes currentType, bool condition, TokenTypes type)
        {
			if (condition)
			{
				Taken = true;
				return type;
			}
			return currentType;
        }
	}
}
