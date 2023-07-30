using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Codelang
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                var filename = Directory.GetCurrentDirectory() + @"./tests/ex3/main.clg";
                //RunCommandLine();
                RunFile(filename);
            }
            else
            {
                RunFile(args[0]);
            }

            //RunCommandLine();
        }

        static void RunFile(string filename)
        {
            // Temporary file to test

            // Read the file if it exists
            if (File.Exists(filename))
            {
                try
                {
                    var filecontents = File.ReadAllText(filename).ToString();

                    /*var watch = new System.Diagnostics.Stopwatch();

                    watch.Start();*/
                    // Tokenize
                    Lexer lexer = new Lexer(filecontents);
                    var tokens = lexer.CreateTokens();

                    Evaluator e = new Evaluator();

                    ParserV2 parser = new ParserV2(tokens);
                    var programTree = parser.Parse();

                    //PrettyPrintTree(programTree);

                   /*Console.WriteLine(
                        JsonConvert.SerializeObject(
                            programTree, 
                            Formatting.Indented,
                            new JsonConverter[] { new StringEnumConverter() }
                        )
                    );*/

                    e.Run(programTree);
                    /*watch.Stop();

                    Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");*/
                }
                catch (Exception e)
                {
                    new ErrorHandler($"LanguageError: {e}");
                }
            }
            else
            {
                // Throw Error if the file does'nt exist
                new ErrorHandler($"Error: {filename} does not exist.");
            }
        }

        private static void PrettyPrintTree(List<SyntaxNode> programTree)
        {
            throw new NotImplementedException();
        }

        static void RunCommandLine()
        {
            bool showTree = false;
            Console.WriteLine("Codelang Version 1.0.0\nCommands: !quit, !cls");
            //Interpreter interpreter = new Interpreter(null);
            Evaluator e = new Evaluator();

            while (true)
            {
                Console.Write("> ");
                var cmd = Console.ReadLine();

                if (cmd == "") continue;
                if (cmd.ToLower() == "!quit") break;
                if (cmd.ToLower() == "!cls") { Console.Clear(); continue; }
                if (cmd.ToLower() == "!showtree") { showTree = !showTree; continue; }

                // Tokenize
                Lexer lexer = new Lexer(cmd);
                var tokens = lexer.CreateTokens();
                //var astNodes = new Parser(tokens).GenerateAST();


                /*var jsonString = JsonConvert.SerializeObject(
                        astNodes, Formatting.Indented,
                        new JsonConverter[] { new StringEnumConverter() });

                Console.WriteLine(jsonString);

                interpreter.ASTNodes = astNodes;
                interpreter.Run();*/

                ParserV2 parser = new ParserV2(tokens);
                var expr = parser.ParseExpr();

                var color = Console.ForegroundColor;

                Console.ForegroundColor = ConsoleColor.DarkGray;

                //if(showTree) PrettyPrint(expr);

                Console.ForegroundColor = color;

                /*var res = e.Evaluate(expr);

                if(res.GetType() != typeof(EvalType))
                {
                    Console.WriteLine(res);
                }*/
            }
        }
    }

    class ErrorHandler
    {
        public ErrorHandler(string errString, bool severity=true)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errString);
            Console.ResetColor();
            if(severity)
            {
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}
