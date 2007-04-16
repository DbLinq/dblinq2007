////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Data.DLinq;
using System.Expressions;

namespace DBLinq.util
{
    /// <summary>
    /// given Expression (e.ID>1), produce summary XML.
    /// Note that there is a Microsoft sample that displays a GUI tree.
    /// </summary>
    public class DumpExpressionXml<T>
    {
        //List<string> whereClause;
        //List<string> whereParam;

        public string FormatExpression(int depth, Expression expr)
        {
            if(expr==null){ return "null"; }
            depth++;
            switch(expr.NodeType)
            {
                case ExpressionType.Lambda:
                    return FormatExpression_Lambda(depth, (LambdaExpression)expr);
                case ExpressionType.MethodCall:
                case ExpressionType.MethodCallVirtual: //occurs for string.StartsWith?!
                    return FormatExpression_Method(depth, (MethodCallExpression)expr);
                case ExpressionType.Constant:
                    return FormatExpression_Const(depth, (ConstantExpression)expr);
                case ExpressionType.MemberAccess:
                    return FormatExpression_Member(depth, (MemberExpression)expr);
                case ExpressionType.Cast:
                    return FormatExpression_Unary(depth, (UnaryExpression)expr);
                case ExpressionType.GT:
                case ExpressionType.LT:
                case ExpressionType.EQ:
                case ExpressionType.AndAlso:
                    return FormatExpression_Binary(depth, (BinaryExpression)expr);
                case ExpressionType.Parameter:
                    return FormatExpression_Parameter(depth, (ParameterExpression)expr);
                case ExpressionType.MemberInit:
                    return FormatExpression_MemberInit(depth, (MemberInitExpression)expr);
                case ExpressionType.New:
                    return FormatExpression_New(depth, (NewExpression)expr);
                default:
                    return "X27: Unprepared for expr "+expr.NodeType;
            }
        }

        private string FormatExpression_Method(int depth, MethodCallExpression expr)
        {
            string spacer = new string(' ',depth);

            List<string> paramStrings = new List<string>();
            foreach(Expression ex in expr.Parameters){
                string str1 = "  <MethodParam>"+FormatExpression(depth,ex)+"</MethodParam>\n";
                paramStrings.Add(str1);
            }
            string allParams = string.Join("\n", paramStrings.ToArray());
            string methodObj = "<MethodObj>"+FormatExpression(depth, expr.Object)+"</MethodObj>";
            string str = "<Method name=\""+expr.Method.Name+"\" >\n"+methodObj+"\n"+allParams+"\n</Method>";
            return spacer + str;
        }

        private string FormatExpression_MemberInit(int depth, MemberInitExpression expr)
        {
            string spacer = new string(' ',depth);
            string inner1 = FormatExpression(depth,expr.NewExpression);
            string str = spacer + "<MemberInit "+formatType(expr.Type)+" >\n" + inner1;
            foreach(Binding bind in expr.Bindings)
            {
                //string bindExpr = bind.b
                if(bind is MemberAssignment){
                    MemberAssignment bind2 = (MemberAssignment)bind;
                    str += spacer + "<MemberBinding>\n"
                        +FormatExpression(depth,bind2.Expression)
                        +bind2.BindingType+" "+bind.Member.Name+"</MemberBinding>\n";
                } else{
                str += spacer + "<MemberBinding>"+bind.BindingType+" "+bind.Member.Name+"</MemberBinding>\n";
                }
            }
            str += spacer + "\n</MemberInit>\n";
            return str;
        }

        private string FormatExpression_New(int depth, NewExpression expr)
        {
            string spacer = new string(' ',depth);
            string str = spacer + "<New ctor=\"xx\" >\n";
            foreach(Expression e in expr.Args)
            {
                str += "<NewArg>"+FormatExpression(depth,expr)+"</NewArg>";
            }
            str += "</New>\n";
            return str;
        }


        private string FormatExpression_Const(int depth, ConstantExpression expr)
        {
            string spacer = new string(' ',depth);
            Expression val2 = expr.Value as Expression;
            if(val2!=null)
                return spacer + "<Const>\n[["+FormatExpression(depth, val2)+"\n</Const>";
            if(expr.Value is ValueType)
                return spacer + "<Const int=\""+expr.Value+"\" />";
            return spacer + "<Const>\n[[Non-Expr:"+expr.Value+"\n"+spacer+"</Const>";
        }

        private string FormatExpression_Unary(int depth, UnaryExpression expr)
        {
            string spacer = new string(' ',depth);
            string a = spacer + " <Left>\n"+FormatExpression(depth, expr.Operand)+"</Left>";
            return spacer + "<Unary type=\""+expr.NodeType+"\">\n" + a + "\n"+spacer+"</Unary>";
        }

        private string FormatExpression_Binary(int depth, BinaryExpression expr)
        {
            string spacer = new string(' ',depth);
            string a = spacer + " <Left>\n"+FormatExpression(depth, expr.Left)+"</Left>";
            string b = spacer + " <Right>\n"+FormatExpression(depth, expr.Right)+"</Right>";
            return spacer + "<Binary type=\""+expr.NodeType+"\">\n" + a + "\n" + b + "\n"+spacer+"</Binary>";

            //string operatorStr = formatBinOperator(expr.NodeType);
            //string left  = "L75_left";
            //string right = "L76_left";
            //this.whereClause.Add( left+" "+operatorStr+" "+right);
        }

        /// <summary>
        /// given 'GT', return '>'
        /// </summary>
        string formatBinOperator(ExpressionType exprType)
        {
            switch(exprType){
                case ExpressionType.GT: return ">";
                case ExpressionType.LT: return "<";
                default:
                    return "TODO_formatOperator_"+exprType;
            }
        }

        private string FormatExpression_Member(int depth, MemberExpression expr)
        {
            string spacer = new string(' ',depth);
            string a = "A:"+FormatExpression(depth, expr.Expression);
            MemberInfo mi = expr.Member;
            return spacer + "<Member fieldInfo=\""+mi+"\">\n"+a+"\n"+spacer+"</Member>";
        }

        private string FormatExpression_Lambda(int depth, LambdaExpression expr)
        {
            string spacer = new string(' ',depth);
            string body = spacer + " <LambdaBody>\n"+FormatExpression(depth, expr.Body)+"\n"+spacer+" </LambdaBody>";
            string par = "";
            foreach(Expression e in expr.Parameters){
                par += "\n" + spacer + "<LambdaParam>\n"+FormatExpression(depth, e)+"</LambdaParam>";
            }
            return "\n" + spacer + "<Lambda>\n"+body+par+"\n"+spacer+"</Lambda>";
        }

        private string FormatExpression_Parameter(int depth, ParameterExpression expr)
        {
            string spacer = new string(' ',depth);
            return spacer + "<Param name=\""+expr.Name+"\" "+formatType(expr.Type)+"/>";
        }

        string formatType(Type t)
        {
            string typeStr = t.Name;
            if(typeStr.Length>20){ typeStr=typeStr.Substring(0,15); }
            typeStr = typeStr.Replace('<','{');
            typeStr = typeStr.Replace('>','}');
            return " type=\""+typeStr+"\"";
        }

    }
}
