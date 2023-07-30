using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codelang
{
    class Evaluator
    {
        private ExpressionSyntax _root;
        private List<Variable> Variables;

        public Evaluator()
        {
            Runtime runtime = new Runtime();

            Variables = new List<Variable>() 
            { 
                new Variable("writeln", "void", new Func<object, EvalType>(runtime.writeln)),
                new Variable("boolstr", "string", new Func<int, string>(runtime.boolstr)),
                new Variable("readln", "string", new Func<object, string>(runtime.readln)),
                new Variable("liststr", "string", new Func<ListSyntax, string>(runtime.liststr))
            };
        }

        public EvalType Run(List<SyntaxNode> statements)
        {
            foreach (var statement in statements)
            {
                Evaluate(statement as ExpressionSyntax);
            }
            return EvalType.Void;
        }

        private object Evaluate(ExpressionSyntax root)
        {
            _root = root;
            return EvaluateExpression(_root);
        }

        private object EvaluateExpression(ExpressionSyntax root)
        {
            if (root is NumberExpressionSyntax n)
            {
                return int.Parse(n.NumberToken.Value.ToString());
            }

            else if (root is BinaryExpressionSyntax b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                switch (b.OperatorToken.Value)
                {
                    case "+":
                        if (left.GetType().Equals(right.GetType()))
                            if (left.GetType() == typeof(int))
                                return (int)left + (int)right;
                            else if (left.GetType() == typeof(string))
                                return (string)left + (string)right;
                            new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot be added to {right}<{CreateTypeName(right)}>", false);
                        new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot be added to {right}<{CreateTypeName(right)}>", false);
                        break;
                    case "-":
                        return (int)left - (int)right;
                    case "/":
                        return (int)left / (int)right;
                    case "*":
                        return (int)left * (int)right;
                    case "==":
                        if (left.GetType().Equals(right.GetType()))
                        {
                            if (left.GetType() == typeof(int))
                                return (int)left == (int)right ? 1 : 0;
                            else if (right.GetType() == typeof(string))
                                return (string)left == (string)right ? 1 : 0;
                        }

                        new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot be compared to {right}<{CreateTypeName(right)}>", false);
                        break;
                    case "!=":
                        if(left.GetType().Equals(right.GetType()))
                        {
                            if (left.GetType() == typeof(int))
                                return (int)left != (int)right ? 1 : 0;
                            else if (right.GetType() == typeof(string))
                                return (string)left != (string)right ? 1 : 0;
                        }

                        new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot be compared to {right}<{CreateTypeName(right)}>", false);
                        break;
                    case ">":
                        if (left.GetType().Equals(right.GetType()))
                        {
                            if (left.GetType() == typeof(int))
                                return (int)left > (int)right ? 1 : 0;

                            new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot be compared to {right}<{CreateTypeName(right)}> using the '>' operator", false);
                            break;
                        }

                        new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot be compared to {right}<{CreateTypeName(right)}>", false);
                        break;
                    case "<":
                        if (left.GetType().Equals(right.GetType()))
                        {
                            if (left.GetType() == typeof(int))
                                return (int)left < (int)right ? 1 : 0;

                            new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot be compared to {right}<{CreateTypeName(right)}> using the '<' operator", false);
                            break;
                        }

                        new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot be compared to {right}<{CreateTypeName(right)}>", false);
                        break;
                    case ">=":
                        if (left.GetType().Equals(right.GetType()))
                        {
                            if (left.GetType() == typeof(int))
                                return (int)left >= (int)right ? 1 : 0;

                            new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot be compared to {right}<{CreateTypeName(right)}> using the '>=' operator", false);
                            break;
                        }

                        new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot be compared to {right}<{CreateTypeName(right)}>", false);
                        break;
                    case "<=":
                        if (left.GetType().Equals(right.GetType()))
                        {
                            if (left.GetType() == typeof(int))
                                return (int)left <= (int)right ? 1 : 0;

                            new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot be compared to {right}<{CreateTypeName(right)}> using the '<=' operator", false);
                            break;
                        }

                        new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot be compared to {right}<{CreateTypeName(right)}>", false);
                        break;

                    case "%":
                        if (left.GetType().Equals(right.GetType()))
                        {
                            if (left.GetType() == typeof(int))
                                return (int)left % (int)right;

                            new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot use '%' on {right}<{CreateTypeName(right)}>", false);
                            break;
                        }

                        new ErrorHandler($"TypeError: {left}<{CreateTypeName(left)}> cannot use '%' on {right}<{CreateTypeName(right)}>", false);
                        break;

                    default:
                        new ErrorHandler($"ParseError: Unexpected binary operator {b.OperatorToken.Value}", false);
                        break;
                }
            }

            else if(root is ParenthisizedExpressionSyntax p)
            {
                return EvaluateExpression(p.Expression);
            }

            else if(root is DefineVariableSyntax dv)
            {
                var result = EvaluateExpression(dv.ValueExpression);
                var newVar = new Variable(dv.NameToken.Value, dv.Datatype.TypeName.Value, result);

                if (!Variables.Exists(x => x.Name == dv.NameToken.Value))
                    Variables.Add(newVar);

                return result;
            }

            else if(root is UseVariableSyntax uv)
            {
                /*Console.WriteLine(JsonConvert.SerializeObject(
                        Variables, Formatting.Indented,
                        new JsonConverter[] { new StringEnumConverter() }));*/
                if (Variables.Exists(x => x.Name == uv.NameToken.Value))
                    return Variables.Where(x => x.Name == uv.NameToken.Value).ToList()[0].Value;

                new ErrorHandler($"VariableNotFoundError: Unknown variable {uv.NameToken.Value}", false);
                return null;
            }

            else if(root is AssignVariableSyntax av)
            {
                var result = EvaluateExpression(av.ValueExpression);

                if (Variables.Exists(x => x.Name == av.NameToken.Value))
                {
                    foreach (var variable in Variables)
                    {
                        if(variable.Name == av.NameToken.Value)
                        {
                            if(variable.VarType == "int")
                            {
                                if(result.GetType() == typeof(int))
                                {
                                    variable.Value = result;
                                }
                                else
                                {
                                    new ErrorHandler($"TypeError: Cannot assign {CreateTypeName(result)} to {variable.Name}<int>.", false);
                                    return null;
                                }
                            }
                            else if(variable.VarType == "string")
                            {
                                if(result.GetType() == typeof(string))
                                {
                                    variable.Value = result;
                                }
                                else
                                {
                                    new ErrorHandler($"TypeError: Cannot assign {CreateTypeName(result)} to {variable.Name}<string>.", false);
                                    return null;
                                }
                            }
                            else if (variable.VarType == "bool")
                            {
                                if (result.GetType() == typeof(bool))
                                {
                                    variable.Value = result;
                                }
                                else
                                {
                                    new ErrorHandler($"TypeError: Cannot assign {CreateTypeName(result)} to {variable.Name}<bool>.", false);
                                    return null;
                                }
                            }

                            else
                            {
                                new ErrorHandler($"TypeError: Unknown type {CreateTypeName(result)}", false);
                                return null;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    new ErrorHandler($"VariableNotFoundError: Unknown variable {av.NameToken.Value}", false);
                    return null;
                }

                return result;
            }

            else if(root is StringExpressionSyntax s)
            {
                var res = s.StringToken.Value;

                if(Variables != null)
                {
                    foreach (var item in Variables)
                    {
                        res = res.Replace("${"+item.Name+"}", item.Value.ToString());
                    }
                }

                return res;
            }

            else if(root is BooleanExpressionSyntax bs)
            {
                return bool.Parse(bs.BooleanToken.Value) ? 1 : 0;
            }

            else if(root is CallFunctionSyntax cf)
            {
                var name = cf.NameToken.Value;
                var param = EvaluateExpression(cf.Param);

                var variable = Variables.Find((x) => x.Name == name).Value;

                if (variable is Delegate)
                {
                    return ((Delegate)variable).DynamicInvoke(param);
                } 
                else
                {
                    return Run((List<SyntaxNode>)variable);
                }
            }

            else if (root is FunctionSyntax fs)
            {
                var argsList = fs.ArgList;
                var statementList = fs.FunctionBlock.Statements;

                return statementList;
            }

            else if (root is ControlFlowSyntax cfs)
            {
                var type = cfs.Keyword.Value;
                var controller = cfs.Controller;
                var statementList = cfs.StatementBlock.Statements;

                var controllerResult = EvaluateExpression(controller);

                switch (type)
                {
                    case "if":
                        if ((int)controllerResult == 1)
                        {
                            return Run(statementList);
                        } else
                        {
                            return type;
                        }
                    case "while":
                        while((int)controllerResult == 1)
                        {
                            Run(statementList);
                            controllerResult = (int)EvaluateExpression(controller);
                        }
                        return type;

                    default: break;
                }
            }

            else if (root is ListSyntax listSyntax)
            {
                return listSyntax;
            }

            else if (root is IndexListSyntax ils)
            {
                var v = EvaluateExpression(ils.Expression);
                var i = EvaluateExpression(ils.Index);

                if (i.GetType() == typeof(int))
                {
                    return EvaluateExpression((ExpressionSyntax)((v as ListSyntax).Statements[(int)i]));
                } else
                {
                    return new ErrorHandler($"SyntaxError: Can not index {(ils.Expression as UseVariableSyntax).NameToken} by {ils.Index} due to type system.");
                }
            }
                 
            return -1;
        }

        public string CreateTypeName(object v)
        {
            var res = v.GetType().Name;

            return res.ToLower();
        }
    }

    class Variable
    {
        public Variable(string name, string type, object value)
        {
            Name = name;
            VarType = type;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Name}<{VarType}> = {Value}";
        }

        public string Name { get; }
        public string VarType { get; }
        public object Value { get; set; }
    }

    class Runtime
    {
        public EvalType writeln(object a)
        {
            Console.WriteLine(a);

            return EvalType.Void;
        }

        public string readln(object a)
        {
            Console.Write(a);

            return Console.ReadLine();
        }

        internal string boolstr(int arg)
        {
            return arg == 1 ? "true" : "false";
        }

        internal string liststr(ListSyntax arg)
        {
            string res = "[";

            foreach (var item in arg.Statements)
            {
                if (item.SyntaxType == TokenTypes.NumberExpression)
                {
                    res += (item as NumberExpressionSyntax).NumberToken.Value;
                }

                else if (item.SyntaxType == TokenTypes.StringExpression)
                {
                    res += (item as StringExpressionSyntax).StringToken.Value;
                }

                else if (item.SyntaxType == TokenTypes.BooleanExpression)
                {
                    res += (item as BooleanExpressionSyntax).BooleanToken.Value;
                }

                else if (item.SyntaxType == TokenTypes.ListBlock)
                {
                    res += liststr(item as ListSyntax);
                }


                res += ", ";
            }

            res = res.Remove(res.Length - 2);
            res += "]";

            return res;
        }
    }

    enum EvalType
    {
        Void,
    }
}
