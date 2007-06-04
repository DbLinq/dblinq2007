////////////////////////////////////////////////////////////////////
//Initial author: Jiri George Moudry, 2006.
//License: LGPL. (Visit http://www.gnu.org)
//Commercial code may call into this library, if it's in a different module (DLL)
////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
//using System.Data.DLinq;
#if LINQ_PREVIEW_2006
//Visual Studio 2005 with Linq Preview May 2006 - can run on Win2000
using System.Expressions;
using System.Data.DLinq;
#else
//Visual Studio Orcas - requires WinXP
using System.Linq.Expressions;
using System.Data.Linq;
#endif

using DBLinq.util;
using DBLinq.vendor;

namespace DBLinq.Linq.clause
{
    public class ParseInputs
    {
        #region ParseInputs: couple fields needed during parsing, eg. paramMap
        public Dictionary<MemberExpression,string> memberExprNickames = new Dictionary<MemberExpression,string>();
        public readonly Dictionary<string,object> paramMap = new Dictionary<string,object>();
        public LambdaExpression groupByExpr;

        public ParseInputs(ParseResult prevResult)
        {
            if(prevResult==null)
                return;
            this.memberExprNickames = prevResult.memberExprNickames;
            this.paramMap = prevResult.paramMap;
        }

        public string NicknameRequest(Expression expr, AssociationAttribute assoc1)
        { 
            //TODO this needs fixing
            return "join";
        }
        /// <summary>
        /// given 'o.Customer', return previously assigned nickname 'o$' (or join$)
        /// </summary>
        public string NicknameRequest(MemberExpression memberExpr)
        {
            string nick;
            if(memberExprNickames.TryGetValue(memberExpr,out nick))
                return nick;
            return "join";
        }
        #endregion
    }

    /// <summary>
    /// ExpressionTreeParser parses expressions such as 
    /// 'c.Product.ProductID', 'c==x' or 'c.ToString()' into a SQL string.
    /// This is used for both Where and Select clauses.
    /// Output: sql clause, sql params, and sql joins
    /// </summary>
    public class ExpressionTreeParser
    {
        static Dictionary<string, string> s_csharpOperatorToSqlMap = new Dictionary<string, string>();
        //{
        //    {"op_Equality", " = "},
        //    {"op_Inequality", " != "},
        //    {"op_GreaterThan", " > "},
        //    {"op_LessThan", " > "},
        //};

        ParseResult _result;
        ParseInputs _inputs;

        static ExpressionTreeParser()
        {
            s_csharpOperatorToSqlMap["op_Equality"] = " = ";
            s_csharpOperatorToSqlMap["op_Inequality"] = " != ";
            s_csharpOperatorToSqlMap["op_GreaterThan"] = " > ";
            s_csharpOperatorToSqlMap["op_GreaterThanOrEqual"] = " >= ";
            s_csharpOperatorToSqlMap["op_LessThan"] = " < ";
            s_csharpOperatorToSqlMap["op_LessThanOrEqual"] = " <= ";
        }

        /// <summary>
        /// pass in bodies of Lambdas, not lambdas themselves
        /// </summary>
        /// <param name="ex">body of LambdaExpr</param>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static ParseResult Parse(Expression ex, ParseInputs inputs)
        {
            RecurData recur = new RecurData();
            ExpressionTreeParser parser = new ExpressionTreeParser();
            parser._inputs = inputs;
            parser._result = new ParseResult(inputs);
            parser.AnalyzeExpression(recur, ex);
            parser._result.EndField();
            return parser._result;
        }

        private void AnalyzeExpression(RecurData recurData, Expression expr)
        {
            recurData.depth++;
            switch(expr.NodeType)
            {
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    AnalyzeBinary(recurData, (BinaryExpression)expr);
                    return;
                case ExpressionType.Call:
                //case ExpressionType.MethodCallVirtual:
                    AnalyzeMethodCall(recurData, (MethodCallExpression)expr);
                    return;
                case ExpressionType.MemberAccess:
                    AnalyzeMember(recurData, (MemberExpression)expr);
                    return;
                case ExpressionType.Constant:
                    AnalyzeConstant(recurData, (ConstantExpression)expr);
                    return;
                case ExpressionType.Parameter:
                    AnalyzeParameter(recurData, (ParameterExpression)expr);
                    return;
                case ExpressionType.MemberInit:
                    AnalyzeMemberInit(recurData, (MemberInitExpression)expr);
                    return;
                case ExpressionType.Convert:
                case ExpressionType.Cast:
                    AnalyzeUnary(recurData, (UnaryExpression)expr);
                    return;
                default:
                    throw new ApplicationException("Analyze: L105 TODO: "+expr.NodeType);
            }
        }

        private void AnalyzeConstant(RecurData recurData, ConstantExpression expr)
        {
            object val = expr.Value;
            if(expr.Type==typeof(string))
            {
                //pass as named parameter:
                string paramName = _result.storeParam((string)val);
                _result.AppendString(paramName);
                return;
            }
            if(expr.Type==typeof(DateTime) || expr.Type==typeof(DateTime?))
            {
                //this is where DateTime.Now gets given to us as a const DateTime
                if(val==null)
                {
                    _result.AppendString("NULL"); //for Nullable DateTime only
                    return;
                }

                DateTime dt = (DateTime)val;
                _result.AppendString("'");
                //TODO: how to format the datetime string?
                //on a UK machine, this format seems to work: '2007-12-03 08:25:00'
                //_result.AppendString(dt.ToString("yyyy-MM-dd HH:mm:ss"));
                string dateFormat = dt.TimeOfDay.TotalHours == 0
                    ? "yyyy-MMM-dd"
                    : "yyyy-MMM-dd HH:mm:ss";
                _result.AppendString(dt.ToString(dateFormat)); //MS SQL requires '2007-Apr-03' format
                _result.AppendString("'");
                return;
            }
            _result.AppendString(val.ToString());
        }

        private void AnalyzeMemberInit(RecurData recurData, MemberInitExpression expr)
        {
            //_result.AppendString("Init");
            int fieldCount = 0;
            recurData.selectAllFields = true;
            foreach (MemberBinding bind in expr.Bindings)
            {
                if(bind.BindingType!=MemberBindingType.Assignment)
                    throw new ArgumentException("AnalyzeMemberInit - only prepared for MemberAssign, not "+bind.BindingType);

                if(fieldCount++ > 0){ this._result.AppendString(","); }
                MemberAssignment memberAssign = (MemberAssignment)bind;
                //AnalyzeExpression(recurData,memberAssign.Expression);

                Expression MAExpr = memberAssign.Expression;

                string nick = null;
                ColumnAttribute[] colAttribs = null;

                switch(MAExpr.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        {
                            MemberExpression memberExpr = MAExpr as MemberExpression;
                            colAttribs = AttribHelper.GetColumnAttribs(MAExpr.Type);
                            if(colAttribs.Length==0)
                            {
                                if(GroupHelper.IsGrouping(memberExpr))
                                {
                                    //eg. {g.Key}
                                    //replace {g.Key} with groupByExpr={o.Customer}
                                    Expression replaceExpr = _inputs.groupByExpr.Body;
                                    AnalyzeExpression(recurData,replaceExpr);
                                } else {
                                    //it's a primitive field (eg. p.ProductID), not a column type
                                    AnalyzeExpression(recurData, memberExpr);
                                }
                                continue;
                            }
                            nick = _inputs.NicknameRequest(memberExpr);
                            nick = VarName.GetSqlName(nick);
                            break;
                        }
                    case ExpressionType.Parameter:
                        {
                            ParameterExpression paramExpr = MAExpr.XParam();
                            nick = VarName.GetSqlName(paramExpr.Name);
                            colAttribs = AttribHelper.GetColumnAttribs(MAExpr.Type);
                            break;
                        }
                    default:
                        AnalyzeExpression(recurData, MAExpr);
                        continue; //unknown path
                }

                int loopIndex=0;
                foreach(ColumnAttribute colAtt in colAttribs)
                {
                    string part = nick+"."+colAtt.Name; //eg. '$o.OrderID'
                    if(loopIndex++>0){ _result.EndField(); }
                    _result.AppendString(part);
                }

            }
            //recurData.selectAllFields = false;
        }

        private void AnalyzeParameter(RecurData recurData, ParameterExpression expr)
        {
            string sqlParamName = VarName.GetSqlName(expr.Name);
            _result.AppendString(sqlParamName); //"e$"
        }

        private void AnalyzeMember(RecurData recurData, MemberExpression expr)
        {
            if(GroupHelper.IsGrouping(expr))
            {
                //eg. {g.Key.Length}
                //replace {g.Key.Length} with groupByExpr={o.Customer.Length}
                Expression replaceExpr = _inputs.groupByExpr.Body;
                if (replaceExpr.NodeType == ExpressionType.MemberInit)
                {
                    //we are grouping by multiple columns
                    //eg. new ComboGroupBy {col1 = o.Product.ProductID, col2 = o.Product.ProductName}
                    AnalyzeExpression(recurData, replaceExpr);
                    return;
                }
                System.Reflection.PropertyInfo pinfo = null;
                Expression replaceMemberExpr = Expression.Property(replaceExpr,pinfo);
                expr = replaceMemberExpr as MemberExpression;
            }

            MemberExpression memberInner = expr.Expression.XMember();
            if(memberInner!=null)
            {
                //process 'o.Customer.City'
                JoinBuilder.AddJoin2(expr, _inputs, _result);
                return;
            }

            AssociationAttribute assoc;
            if ( AttribHelper.IsAssociation(expr,out assoc) )
            {
                //process 'o.Customer'
                JoinBuilder.AddJoin2(expr, _inputs, _result);
                return;
            }

            int pos1 = _result.MarkSbPosition();

            AnalyzeExpression(recurData, expr.Expression);

            string varName = _result.Substring(pos1);
            //_result.Revert(pos1);

            _result.tablesUsed[expr.Expression.Type] = varName;
            _result.AppendString(".");
            _result.AppendString(expr.Member.Name);
        }

        //Dictionary<string,string> csharpOperatorToSqlMap = new Dictionary<string,string>
        //{
        //    {"op_Equality", " = "},
        //    {"op_Inequality", " != "},
        //    {"op_GreaterThan", " > "},
        //    {"op_LessThan", " > "},
        //};

        internal void AnalyzeMethodCall(RecurData recurData, MethodCallExpression expr)
        {
            string methodName = expr.Method.Name;

            string sqlOperatorName;
            if (s_csharpOperatorToSqlMap.TryGetValue(methodName, out sqlOperatorName))
            {
                //map "op_Inequality" to " != "
                AnalyzeExpression(recurData, expr.Arguments[0]);
                _result.AppendString(sqlOperatorName);
                AnalyzeExpression(recurData, expr.Arguments[1]);
                return;
            }

            //special handling
            switch(expr.Method.Name)
            {
                //case "op_Equality":
                //    AnalyzeExpression(recurData, expr.Parameters[0]);
                //    _result.AppendString(" = ");
                //    AnalyzeExpression(recurData, expr.Parameters[1]);
                //    return;
                //case "op_Inequality":
                //    AnalyzeExpression(recurData, expr.Parameters[0]);
                //    _result.AppendString(" != ");
                //    AnalyzeExpression(recurData, expr.Parameters[1]);
                //    return;
                //case "op_GreaterThan":
                //    AnalyzeExpression(recurData, expr.Parameters[0]);
                //    _result.AppendString(" > ");
                //    AnalyzeExpression(recurData, expr.Parameters[1]);
                //    return;

                case "StartsWith":
                case "EndWith":
                case "Contains":
                    {
                        //turn "e.Name.StartsWith("X")" -> "e.Name LIKE 'X%'
                        //turn "e.Name.Contains("X")" -> "e.Name LIKE '%X%'
                        AnalyzeExpression(recurData, expr.Object);
                        _result.AppendString(" LIKE ");
                        AnalyzeExpression(recurData, expr.Arguments[0]);
                        string paramName = _result.lastParamName;
                        string lastParam = _result.paramMap[paramName] as string;
                        if(lastParam !=null)
                        {
                            //modify parameter from X to X%
                            string modParam = "";
                            switch(expr.Method.Name)
                            { 
                                case "StartsWith":  modParam = lastParam + "%"; break;
                                case "EndWith":     modParam = "%" + lastParam; break;
                                case "Contains":    modParam = "%" + lastParam + "%"; break;
                            }
                            _result.paramMap[paramName] = modParam;
                        }
                    }
                    return;
                case "Sum":
                    {
                        //extract 'OrderID' from '{g.Sum(o => Convert(o.OrderID))}'
                        Expression sumExpr1 = expr.Arguments[1].XLambda().Body;
                        MemberExpression sumExpr2 = null;
                        switch(sumExpr1.NodeType)
                        {
                            case ExpressionType.Cast:
                                sumExpr2 = sumExpr1.XCastOperand().XMember(); break;
                            case ExpressionType.MemberAccess:
                                sumExpr2 = sumExpr1.XMember(); break;
                            case ExpressionType.Convert:
                                //eg. {g.Sum(o => Convert(o.OrderID))} (from G08_OrderSumByCustomerID)
                                sumExpr2 = sumExpr1.XUnary().Operand.XMember();
                                break;
                            default:
                                throw new ArgumentException("L277 Sum(lambda): unprepared for lambda expr "+sumExpr1.NodeType);
                        }
                        _result.AppendString("SUM("+sumExpr2.Member.Name+")");
                        return;
                    }
                case "Count":
                    {
                        //given expr='{g.Count()}', produce Count expression
                        _result.AppendString("COUNT(*)");
                        return;
                    }
                case "Concat":
                    {
                        //this path is taken in LinqPreview2006.
                        //In OrcasBeta1, we get operator+
                        List<string> strings = new List<string>();
                        int posInitial = _result.MarkSbPosition();
                        foreach (Expression concatPart in expr.Arguments)
                        {
                            int pos2A = _result.MarkSbPosition();

                            //strip bogus UnaryExpression containing MemberExpression, if any
                            Expression concatPart2 = concatPart.XCastOperand() ?? concatPart;

                            AnalyzeExpression(recurData, concatPart2);
                            string substr = _result.Substring(pos2A);
                            strings.Add(substr);
                        }
                        _result.Revert(posInitial);
                        string sqlConcatStr = Vendor.Concat(strings);
                        _result.AppendString(sqlConcatStr);
                        return;
                    }
                case "ToLower":
                case "ToUpper":
                    {
                        string sqlFctName = expr.Method.Name == "ToLower" ? "LOWER(" : "UPPER(";
                        _result.AppendString(sqlFctName);
                        AnalyzeExpression(recurData, expr.Object);
                        _result.AppendString(")");
                        return;
                    }
                case "FromOADate":
                    {
                        //convert double to DateTime
                        _result.AppendString("CAST(");
                        AnalyzeExpression(recurData, expr.Arguments[0]); //it's a static fct - don't pass expr.Object
                        _result.AppendString(" as smalldatetime)");
                        return;
                    }

                default:
                    //detailed error will be thrown below
                    break;
            }
            //TODO: throw for any other method - database probably cannot handle such call
            string msg2 ="L274: Unprepared to map method "+methodName+" ("+expr+") to SQL";
            Console.WriteLine(msg2);
            throw new ApplicationException(msg2);
            //_result.AppendString(expr.Method.Name);
        }

        private void AnalyzeUnary(RecurData recurData, UnaryExpression expr)
        {
            if (expr.NodeType == ExpressionType.Convert)
            {
                AnalyzeExpression(recurData, expr.Operand);
                return;
            }

            AnalyzeExpression(recurData, expr.Operand);

            string operatorStr = "UNOP:"+expr.NodeType.ToString(); //formatBinaryOperator(expr.NodeType);
            _result.AppendString(" "+ operatorStr + " ");
        }

        private void AnalyzeBinary(RecurData recurData, BinaryExpression expr)
        {
            if (expr.NodeType == ExpressionType.Add && expr.Type==typeof(string))
            {
                //in LinqPreview2006, this used to be MethodCall "Concat"
                List<string> strings = new List<string>();
                int posInitial = _result.MarkSbPosition();
                List<Expression> operands = new List<Expression>() { expr.Left, expr.Right };
                foreach (Expression concatPart in operands)
                {
                    int pos2A = _result.MarkSbPosition();

                    //strip bogus UnaryExpression containing MemberExpression, if any
                    Expression concatPart2 = concatPart.XCastOperand() ?? concatPart;

                    AnalyzeExpression(recurData, concatPart2);
                    string substr = _result.Substring(pos2A);
                    strings.Add(substr);
                }
                _result.Revert(posInitial);
                string sqlConcatStr = Vendor.Concat(strings);
                _result.AppendString(sqlConcatStr);
                return;
            }

            int precedence = Operators.GetPrecedence(expr.NodeType);
            bool needsBrackets = (recurData.operatorPrecedence > precedence);
            recurData.operatorPrecedence = precedence; //nested methods will see different precedence
            
            if(needsBrackets)
            {
                _result.AppendString("(");
            }

            AnalyzeExpression(recurData, expr.Left);

            string operatorStr = Operators.FormatBinaryOperator(expr.NodeType);
            _result.AppendString(" "+ operatorStr + " ");

            AnalyzeExpression(recurData, expr.Right);
            if(needsBrackets)
            {
                _result.AppendString(")");
            }
        }
        public struct RecurData
        {
            public int depth;
            public int operatorPrecedence;
            public bool selectAllFields;
        }
    }
}
