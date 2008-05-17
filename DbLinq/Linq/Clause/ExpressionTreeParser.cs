#region MIT license
// 
// Copyright (c) 2007-2008 Jiri Moudry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DbLinq.Util;
using DbLinq.Util.ExprVisitor;
using DbLinq.Linq.Mapping;
using DbLinq.Vendor;

namespace DbLinq.Linq.Clause
{
    /// <summary>
    /// ExpressionTreeParser parses expressions such as 
    /// 'c.Product.ProductID', 'c==x' or 'c.ToString()' into a SQL string.
    /// This is used for both Where and Select clauses.
    /// Output: sql clause, sql params, and sql joins
    /// </summary>
    public partial class ExpressionTreeParser
    {
        static readonly Dictionary<string, string> s_csharpOperatorToSqlMap = new Dictionary<string, string>()
        {
            {"op_Equality", " = "},
            {"op_Inequality", " != "},
            {"op_GreaterThan", " > "},
            {"op_GreaterThanOrEqual", " >= "},
            {"op_LessThan", " > "},
            {"op_LessThanOrEqual", " <= "},
        };

        QueryProcessor _parent;
        ParseResult _result;
        //ParseInputs _inputs;
        bool _isInTransparentIdBlock;


        /// <summary>
        /// main entry point for recursive analysis of an expression tree.
        /// </summary>
        /// <returns>ParseResult containing params, sql string</returns>
        public static ParseResult Parse(IVendor vendor, QueryProcessor parent, Expression expr)
        {
            RecurData recurData = new RecurData { allowSelectAllFields = true };
            return Parse(recurData, vendor, parent, expr);
        }

        public static ParseResult Parse(RecurData recurData, IVendor vendor, QueryProcessor parent, Expression expr)
        {
            ExpressionTreeParser parser = new ExpressionTreeParser();
            parser._parent = parent;
            parser._result = new ParseResult(vendor);

            parser.AnalyzeExpression(recurData, expr); //recursion here

            parser._result.EndField();
            return parser._result;
        }


        private AnalysisResult AnalyzeExpression(RecurData recurData, Expression expr)
        {
            recurData.depth++;
            switch (expr.NodeType)
            {
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Coalesce:
                case ExpressionType.And:
                    return AnalyzeBinary(recurData, (BinaryExpression)expr);
                case ExpressionType.Call:
                    //case ExpressionType.MethodCallVirtual:
                    return AnalyzeMethodCall(recurData, (MethodCallExpression)expr, _parent._vars.Context);
                case ExpressionType.MemberAccess:
                    return AnalyzeMember(recurData, (MemberExpression)expr);
                case ExpressionType.Constant:
                    return AnalyzeConstant(recurData, (ConstantExpression)expr);
                case ExpressionType.Parameter:
                    return AnalyzeParameter(recurData, (ParameterExpression)expr);
                case ExpressionType.MemberInit:
                    return AnalyzeMemberInit(recurData, (MemberInitExpression)expr);
                case ExpressionType.Convert:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                    //case ExpressionType.Cast: //Cast disappeared in Bet2?!
                    return AnalyzeUnary(recurData, (UnaryExpression)expr);
                case ExpressionType.Invoke:
                    //case ExpressionType.Cast: //Cast disappeared in Bet2?!
                    return AnalyzeInvocation(recurData, (InvocationExpression)expr);
                case ExpressionType.New:
                    {
                        //new case in Beta2 - route into MemberInit
                        NewExpression newExpr = (NewExpression)expr;
                        return AnalyzeNew(recurData, newExpr);

                        //MemberBinding[] fakeBindings = new MemberBinding[0]; //newExpr.Arguments
                        //MemberInitExpression fakeMemberInit = Expression.MemberInit(newExpr, fakeBindings);
                        //AnalyzeMemberInit(recurData, fakeMemberInit);
                    }
                default:
                    throw new ApplicationException("L105 TODO add parsing of expression: " + expr.NodeType);
            }
        }

        private AnalysisResult AnalyzeInvocation(RecurData recurData, InvocationExpression expr)
        {
            if (expr.Arguments.Any(arg => arg.NodeType != ExpressionType.Parameter))
                throw new ArgumentException("L142 TODO: rewrite Invocation to replace Lambda args");
            
            //if we get here, all Invoke params are plain parameters, and we can ignore them
            if(expr.Expression.NodeType!=ExpressionType.Lambda)
                throw new ArgumentException("L146 Invocation: only prepared for a lambda");

            return AnalyzeExpression(recurData, expr.Expression.XLambda().Body);
        }

        private AnalysisResult AnalyzeConstant(RecurData recurData, ConstantExpression expr)
        {
            object val = expr.Value;

            if (expr.Type == typeof(string))
            {
                //pass as named parameter:
                //string paramName = _result.storeParam((string)val);
                string paramName = _parent.storeParam((string)val);
                _result.AppendString(paramName);
                return AnalysisResult.Proceed;
            }
            else if (val == null)
            {
                _result.AppendString("NULL"); //for int? or DateTime? only
                return AnalysisResult.Proceed;
            }
            else if (expr.Type == typeof(DateTime) || expr.Type == typeof(DateTime?))
            {
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
                return AnalysisResult.Proceed;
            }
            _result.AppendString(val.ToString());
            return AnalysisResult.Proceed;
        }

        private AnalysisResult AnalyzeMemberInit(RecurData recurData, MemberInitExpression expr)
        {
            //_result.AppendString("Init");
            int fieldCount = 0;
            //recurData.selectAllFields = true;
            foreach (MemberBinding bind in expr.Bindings)
            {
                if (bind.BindingType != MemberBindingType.Assignment)
                    throw new ArgumentException("AnalyzeMemberInit - only prepared for MemberAssign, not " + bind.BindingType);

                if (fieldCount++ > 0) { this._result.AppendString(","); }
                MemberAssignment memberAssign = (MemberAssignment)bind;
                //AnalyzeExpression(recurData,memberAssign.Expression);

                Expression MAExpr = memberAssign.Expression;

                string nick = null;
                ColumnAttribute[] colAttribs = null;

                switch (MAExpr.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        {
                            MemberExpression memberExpr = MAExpr as MemberExpression;
                            colAttribs = AttribHelper.GetColumnAttribs(MAExpr.Type);
                            if (colAttribs.Length == 0)
                            {
                                if (GroupHelper.IsGrouping(memberExpr))
                                {
                                    //eg. {g.Key}
                                    //replace {g.Key} with GroupByExpression={o.Customer}
                                    Expression replaceExpr = _parent._vars.GroupByExpression.Body;
                                    AnalyzeExpression(recurData, replaceExpr);
                                }
                                else
                                {
                                    //it's a primitive field (eg. p.ProductID), not a column type
                                    AnalyzeExpression(recurData, memberExpr);
                                }
                                continue;
                            }
                            nick = _parent.NicknameRequest(memberExpr);
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

                int loopIndex = 0;
                foreach (ColumnAttribute colAtt in colAttribs)
                {
                    string part = nick + "." + colAtt.Name; //eg. '$o.OrderID'
                    if (loopIndex++ > 0) { _result.EndField(); }
                    _result.AppendString(part);
                }

            }
            //recurData.selectAllFields = false;
            return AnalysisResult.Proceed;
        }

        //in Beta2, we now seem to have a new animal - select new { ProductId=p.ProductID, Name=p.ProductName }
        //comes in not as MemberInit, but as NewExpr.
        //thus I cloned AnalyzeMemberInit from above
        private AnalysisResult AnalyzeNew(RecurData recurData, NewExpression expr)
        {
            //_result.AppendString("Init");
            int fieldCount = 0;
            //recurData.selectAllFields = true;
            foreach (Expression bind in expr.Arguments)
            {
                //if (bind.BindingType != MemberBindingType.Assignment)
                //    throw new ArgumentException("AnalyzeMemberInit - only prepared for MemberAssign, not " + bind.BindingType);

                if (fieldCount++ > 0) { this._result.AppendString(","); }
                //MemberAssignment memberAssign = (MemberAssignment)bind;

                //Expression MAExpr = memberAssign.Expression;
                Expression MAExpr = bind;

                string nick = null;
                ColumnAttribute[] colAttribs = null;

                switch (MAExpr.NodeType)
                {
                    case ExpressionType.MemberAccess:
                        {
                            MemberExpression memberExpr = MAExpr as MemberExpression;
                            colAttribs = AttribHelper.GetColumnAttribs(MAExpr.Type);
                            bool isAssoc = AttribHelper.IsAssociation(memberExpr);

                            if (colAttribs.Length == 0)
                            {
                                if (GroupHelper.IsGrouping(memberExpr))
                                {
                                    //eg. {g.Key}
                                    //replace {g.Key} with GroupByExpression={o.Customer}
                                    //(Expression replaceExpr = _inputs.GroupByExpression.Body; //Too simple!)
                                    Expression replaceExpr = _parent.SubstitueGroupKeyExpression(memberExpr);

                                    AnalyzeExpression(recurData, replaceExpr);
                                }
                                else
                                {
                                    //it's a primitive field (eg. p.ProductID), not a column type
                                    AnalyzeExpression(recurData, memberExpr);
                                }
                                continue;
                            }
                            else if (isAssoc)
                            {
                                //eg. select p.Supplier from Products
                                recurData.allowSelectAllFields = false;
                                JoinBuilder.AddJoin2(recurData, _parent, memberExpr, _result);
                            }

                            //try extracting 'c' from '<>h__TransparentIdentifier10.c' 
                            ParameterExpression expr2 = memberExpr.StripTransparentID().XParam();
                            if (expr2 != null)
                            {
                                nick = expr2.Name; // VarName.GetSqlName(expr2.Name);
                            }
                            else
                            {
                                nick = _parent.NicknameRequest(memberExpr);
                            }
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

                int loopIndex = 0;
                foreach (ColumnAttribute colAtt in colAttribs)
                {
                    string part = nick + "." + colAtt.Name; //eg. '$o.OrderID'
                    if (loopIndex++ > 0) { _result.EndField(); }
                    _result.AppendString(part);
                }

            }
            //recurData.selectAllFields = false;
            return AnalysisResult.Proceed;
        }


        private AnalysisResult AnalyzeParameter(RecurData recurData, ParameterExpression expr)
        {
            string sqlParamName;
            if (_isInTransparentIdBlock)
            {
                //don't use remembered var names during self-join
                if (recurData.allowSelectAllFields && AttribHelper.GetTableAttrib(expr.Type) != null)
                {
                    FromClauseBuilder.SelectAllFields(_parent._vars, this._parent._vars.SqlParts, expr.Type, expr.Name);
                    return AnalysisResult.Proceed;
                }
            }
            else if (_parent.currentVarNames.TryGetValue(expr.Type, out sqlParamName))
            {
                _result.AppendString(sqlParamName); //used from D11_Products_DoubleWhere()
                return AnalysisResult.Proceed;
            }

            sqlParamName = VarName.GetSqlName(expr.Name);
            _result.AppendString(sqlParamName); //"e$"
            _parent.currentVarNames[expr.Type] = sqlParamName;

            return AnalysisResult.Proceed;
        }

        /// <summary>
        /// process 'a.b' or 'a.b.c' expressions
        /// </summary>
        private AnalysisResult AnalyzeMember(RecurData recurData, MemberExpression expr)
        {
            FunctionReturningObject funcReturningObj;
            if (LocalExpressionChecker.TryMatchLocalExpression(expr, out funcReturningObj))
            {
                //handle 'someObject.SomeField' constants
                string paramName = _parent.storeFunctionParam(funcReturningObj);
                _result.AppendString(paramName);
                return AnalysisResult.Proceed;
            }

            if (GroupHelper.IsGrouping(expr))
            {
                //eg. {g.Key.Length}
                //replace {g.Key.Length} with GroupByExpression={o.Customer.Length}
                Expression replaceExpr = _parent._vars.GroupByExpression.Body;
                if (replaceExpr.NodeType == ExpressionType.MemberInit)
                {
                    //we are grouping by multiple columns
                    //eg. new ComboGroupBy {col1 = o.Product.ProductID, col2 = o.Product.ProductName}
                    return AnalyzeExpression(recurData, replaceExpr);
                }
                System.Reflection.PropertyInfo pinfo = null;
                Expression replaceMemberExpr = Expression.Property(replaceExpr, pinfo);
                expr = replaceMemberExpr as MemberExpression;
            }

            string exprStr = expr.Expression.ToString();
            if (exprStr.StartsWith("<>h__TransparentIdentifier"))
            {
                switch (expr.Expression.NodeType)
                {
                    case ExpressionType.MemberAccess:
                    case ExpressionType.Parameter:
                        //ignore the first bit in '<>h__TransparentIdentifier10$.c.City
                        _isInTransparentIdBlock = true;
                        Expression stripped = expr.StripTransparentID();
                        var ar=AnalyzeExpression(recurData, stripped);
                        _isInTransparentIdBlock = false;
                        return ar;
                    default:
                        break;
                }
            }

            MemberExpression memberInner = expr.Expression.XMember();
            if (memberInner != null)
            {
                //check for 'e.ReportsTo.Value'
                bool isNullableValue = ExprExtensions.IsTypeNullable(expr.Expression)
                    && expr.Member.Name == "Value";
                if (isNullableValue)
                {
                    //process as 'e.ReportsTo' - don't go into JoinBuilder
                    expr = memberInner;
                }
                else
                {
                    memberInner = memberInner.StripTransparentID() as MemberExpression;
                    if (memberInner != null)
                    {
                        if (memberInner.Type == typeof(string) || memberInner.Type == typeof(DateTime))
                        {
                            //process {p.ProductName.Length}
                            AnalyzeBuiltinMember(recurData, expr);
                        }
                        else
                        {
                            //process 'o.Customer.City'
                            JoinBuilder.AddJoin2(recurData, _parent, expr, _result);
                        }
                        return AnalysisResult.Proceed;
                    }
                }
            }

            AttribAndProp attribAndProp;
            //AssociationAttribute assoc;
            if (AttribHelper.IsAssociation(expr, out attribAndProp))
            {
                //process 'o.Customer'
                JoinBuilder.AddJoin2(recurData, _parent, expr, _result);
                return AnalysisResult.Proceed;
            }

            Expression inner = expr.Expression;
            if (inner.NodeType == ExpressionType.Constant && inner.Type.Name.Contains("__DisplayClass"))
            {
                //in OrcasBeta1, ConstantExpression comes as MemberEXpression
                //(they don't pass you an integer, they pass you c__DisplayClass1.myID)
                System.Reflection.MemberInfo field = expr.Member;
                ConstantExpression wrapperObj = (ConstantExpression)inner;
                object constObj = FieldUtils.GetValue(field, wrapperObj.Value);
                ConstantExpression constExpr = Expression.Constant(constObj);
                AnalyzeConstant(recurData, constExpr);
                return AnalysisResult.Proceed;
            }


            int pos1 = _result.MarkSbPosition();

            recurData.allowSelectAllFields = false;
            AnalyzeExpression(recurData, expr.Expression);

            string varName = _result.Substring(pos1);
            //_result.Revert(pos1);

            _result.tablesUsed[expr.Expression.Type] = varName;
            _result.AppendString(".");

            //TODO: this appends "Alltypes.int_" whereas it should append "Alltypes.`int`"
            ColumnAttribute columnAttrib = expr.Member.GetCustomAttributes(false).OfType<ColumnAttribute>().FirstOrDefault();
            string sqlColumnName = expr.Member.Name;
            if (columnAttrib != null)
                sqlColumnName = _parent._vars.Context.Vendor.GetSqlFieldSafeName(columnAttrib.Name);
            _result.AppendString(sqlColumnName);

            return AnalysisResult.Proceed;
        }

        /// <summary>
        /// handle 'p.ProductName.Length' etc
        /// </summary>
        /// <param name="recurData"></param>
        /// <param name="memberExpr"></param>
        private AnalysisResult AnalyzeBuiltinMember(RecurData recurData, MemberExpression memberOuter)
        {
            MemberExpression memberInner = memberOuter.Expression.XMember();
            if (memberInner.Type == typeof(string) && memberOuter.Member.Name == "Length")
            {
                //process string length function here. 
                //"LENGTH()" function seems to be available on Oracle,Mysql,PostgreSql
                //Ha! it's called LEN() on MssqlServer
                string length_func = _parent._vars.Context.Vendor.GetSqlStringLengthFunction();
                _result.AppendString(length_func + "(");
                AnalyzeExpression(recurData, memberInner);
                _result.AppendString(")");
            }
            return AnalysisResult.Proceed;
        }

        //Dictionary<string,string> csharpOperatorToSqlMap = new Dictionary<string,string>
        //{
        //    {"op_Equality", " = "},
        //    {"op_Inequality", " != "},
        //    {"op_GreaterThan", " > "},
        //    {"op_LessThan", " > "},
        //};


        private AnalysisResult AnalyzeUnary(RecurData recurData, UnaryExpression expr)
        {
            if (expr.NodeType == ExpressionType.Convert)
            {
                AnalyzeExpression(recurData, expr.Operand);
                return AnalysisResult.Proceed;
            }

            bool isNot = expr.NodeType == ExpressionType.Not;
            if (isNot)
            {
                MemberExpression operandMember = expr.Operand.XMember(); //eg. {e.ReportsTo.HasValue}
                if (operandMember != null)
                {
                    //check for 'IS NULL' 
                    Type opType = operandMember.Expression.Type;
                    bool isNullExpr = opType.IsGenericType
                        && opType.GetGenericTypeDefinition() == typeof(Nullable<>)
                        && operandMember.Member.Name == "HasValue";
                    if (isNullExpr)
                    {
                        //found special case 'X IS NOT NULL'
                        var ar=AnalyzeExpression(recurData, operandMember.Expression); //process {e.ReportsTo}
                        _result.AppendString(" IS NOT NULL ");
                        return ar; //end special case
                    }
                }
                _result.AppendString(" NOT ");
            }

            AnalyzeExpression(recurData, expr.Operand);

            if (isNot)
                return AnalysisResult.Proceed;

            string operatorStr = "UNOP:" + expr.NodeType.ToString(); //formatBinaryOperator(expr.NodeType);
            _result.AppendString(" " + operatorStr + " ");

            return AnalysisResult.Proceed;
        }

        public class NameAndType
        {
            public string name;
            public Type type;
        }

        private AnalysisResult AnalyzeBinary(RecurData recurData, BinaryExpression expr)
        {
            if (expr.NodeType == ExpressionType.Add && expr.Type == typeof(string))
            {
                //in LinqPreview2006, this used to be MethodCall "Concat"
                List<ExpressionAndType> strings = new List<ExpressionAndType>();
                int posInitial = _result.MarkSbPosition();
                List<Expression> operands = new List<Expression>() { expr.Left, expr.Right };
                foreach (Expression concatPart in operands)
                {
                    int pos2A = _result.MarkSbPosition();

                    //strip bogus UnaryExpression containing MemberExpression, if any
                    Expression concatPart2 = concatPart.XCastOperand() ?? concatPart;

                    AnalyzeExpression(recurData, concatPart2);
                    string substr = _result.Substring(pos2A);
                    strings.Add(new ExpressionAndType { expression = substr, type = concatPart2.Type });
                }
                _result.Revert(posInitial);
                string sqlConcatStr = _parent._vars.Context.Vendor.GetSqlConcat(strings);
                _result.AppendString(sqlConcatStr);
                return AnalysisResult.Proceed;
            }

            bool isNE = expr.NodeType == ExpressionType.NotEqual;
            bool isEQ = expr.NodeType == ExpressionType.Equal;
            if (isEQ || isNE)
            {
                Expression exprBeingComparedWithNull;
                if (ExprExtensions.IsNullTest(expr, out exprBeingComparedWithNull))
                {
                    //special case 'e.ReportsTo==null'
                    AnalyzeExpression(recurData, exprBeingComparedWithNull);
                    string opString = isEQ
                        ? " IS NULL"
                        : " IS NOT NULL";
                    _result.AppendString(opString);
                    return AnalysisResult.Proceed;
                }
            }

            if (expr.NodeType == ExpressionType.Coalesce)
            {
                _result.AppendString("COALESCE(");
                this.AnalyzeExpression(recurData, expr.Left);
                _result.AppendString(" , ");
                this.AnalyzeExpression(recurData, expr.Right);
                _result.AppendString(")");
                return AnalysisResult.Proceed;
            }

            int precedence = Operators.GetPrecedence(expr.NodeType);
            bool needsBrackets = (recurData.operatorPrecedence > precedence);
            recurData.operatorPrecedence = precedence; //nested methods will see different precedence

            if (needsBrackets)
            {
                _result.AppendString("(");
            }

            var ar=AnalyzeExpression(recurData, expr.Left);

            // some method may require to skip the test result (VB string compare for example)
            if ((ar & AnalysisResult.SkipRight) == 0)
            {
                string operatorStr = Operators.FormatBinaryOperator(expr.NodeType);
                _result.AppendString(" " + operatorStr + " ");

                AnalyzeExpression(recurData, expr.Right);
                if (needsBrackets)
                {
                    _result.AppendString(")");
                }
            }
            return AnalysisResult.Proceed;
        }

        public struct RecurData
        {
            public int depth;
            public int operatorPrecedence;
            public bool allowSelectAllFields;
        }

        [Flags]
        public enum AnalysisResult
        {
            Proceed = 0,
            SkipRight = 0x2,
        }
    }
}
