#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using DBLinq.util;
using DBLinq.Linq.Mapping;
using DBLinq.vendor;

namespace DBLinq.Linq.clause
{
    /// <summary>
    /// ExpressionTreeParser parses expressions such as 
    /// 'c.Product.ProductID', 'c==x' or 'c.ToString()' into a SQL string.
    /// This is used for both Where and Select clauses.
    /// Output: sql clause, sql params, and sql joins
    /// </summary>
    public class ExpressionTreeParser
    {
        static Dictionary<string, string> s_csharpOperatorToSqlMap = new Dictionary<string, string>()
        {
            {"op_Equality", " = "},
            {"op_Inequality", " != "},
            {"op_GreaterThan", " > "},
            {"op_GreaterThanOrEqual", " >= "},
            {"op_LessThan", " > "},
            {"op_LessThanOrEqual", " <= "},
        };

        static IVendor s_vendor = VendorFactory.Make();

        QueryProcessor _parent;
        ParseResult _result;
        //ParseInputs _inputs;
        bool _isInTransparentIdBlock;


        /// <summary>
        /// main entry point for recursive analysis of an expression tree.
        /// </summary>
        /// <returns>ParseResult containing params, sql string</returns>
        public static ParseResult Parse(QueryProcessor parent, Expression ex)
        {
            RecurData recur = new RecurData();
            ExpressionTreeParser parser = new ExpressionTreeParser();
            parser._parent = parent;
            parser._result = new ParseResult();

            parser.AnalyzeExpression(recur, ex); //recursion here

            parser._result.EndField();
            return parser._result;
        }

        private void AnalyzeExpression(RecurData recurData, Expression expr)
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
                case ExpressionType.Not:
                    //case ExpressionType.Cast: //Cast disappeared in Bet2?!
                    AnalyzeUnary(recurData, (UnaryExpression)expr);
                    return;
                case ExpressionType.New:
                    {
                        //new case in Beta2 - route into MemberInit
                        NewExpression newExpr = (NewExpression)expr;
                        AnalyzeNew(recurData, newExpr);

                        //MemberBinding[] fakeBindings = new MemberBinding[0]; //newExpr.Arguments
                        //MemberInitExpression fakeMemberInit = Expression.MemberInit(newExpr, fakeBindings);
                        //AnalyzeMemberInit(recurData, fakeMemberInit);
                        return;
                    }
                default:
                    throw new ApplicationException("L105 TODO add parsing of expression: " + expr.NodeType);
            }
        }


        private void AnalyzeConstant(RecurData recurData, ConstantExpression expr)
        {
            object val = expr.Value;

            if (expr.Type == typeof(string))
            {
                //pass as named parameter:
                //string paramName = _result.storeParam((string)val);
                string paramName = _parent.storeParam((string)val);
                _result.AppendString(paramName);
                return;
            }
            else if (val == null)
            {
                _result.AppendString("NULL"); //for int? or DateTime? only
                return;
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
                return;
            }
            _result.AppendString(val.ToString());
        }

        private void AnalyzeMemberInit(RecurData recurData, MemberInitExpression expr)
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
                                    //replace {g.Key} with groupByExpr={o.Customer}
                                    Expression replaceExpr = _parent._vars.groupByExpr.Body;
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
        }

        //in Beta2, we now seem to have a new animal - select new { ProductId=p.ProductID, Name=p.ProductName }
        //comes in not as MemberInit, but as NewExpr.
        //thus I cloned AnalyzeMemberInit from above
        private void AnalyzeNew(RecurData recurData, NewExpression expr)
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
                            if (colAttribs.Length == 0)
                            {
                                if (GroupHelper.IsGrouping(memberExpr))
                                {
                                    //eg. {g.Key}
                                    //replace {g.Key} with groupByExpr={o.Customer}
                                    //(Expression replaceExpr = _inputs.groupByExpr.Body; //Too simple!)
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
        }


        private void AnalyzeParameter(RecurData recurData, ParameterExpression expr)
        {
            string sqlParamName;
            if (_isInTransparentIdBlock)
            {
                //don't use remembered var names during self-join
            }
            else if (_parent.currentVarNames.TryGetValue(expr.Type, out sqlParamName))
            {
                _result.AppendString(sqlParamName); //used from D11_Products_DoubleWhere()
                return;
            }

            sqlParamName = VarName.GetSqlName(expr.Name);
            _result.AppendString(sqlParamName); //"e$"
            _parent.currentVarNames[expr.Type] = sqlParamName;
        }

        /// <summary>
        /// process 'a.b' or 'a.b.c' expressions
        /// </summary>
        private void AnalyzeMember(RecurData recurData, MemberExpression expr)
        {
            if (GroupHelper.IsGrouping(expr))
            {
                //eg. {g.Key.Length}
                //replace {g.Key.Length} with groupByExpr={o.Customer.Length}
                Expression replaceExpr = _parent._vars.groupByExpr.Body;
                if (replaceExpr.NodeType == ExpressionType.MemberInit)
                {
                    //we are grouping by multiple columns
                    //eg. new ComboGroupBy {col1 = o.Product.ProductID, col2 = o.Product.ProductName}
                    AnalyzeExpression(recurData, replaceExpr);
                    return;
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
                        AnalyzeExpression(recurData, stripped);
                        _isInTransparentIdBlock = false;
                        return;
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
                            JoinBuilder.AddJoin2(_parent, expr, _result);
                        }
                        return;
                    }
                }
            }

            AttribAndProp attribAndProp;
            //AssociationAttribute assoc;
            if (AttribHelper.IsAssociation(expr, out attribAndProp))
            {
                //process 'o.Customer'
                JoinBuilder.AddJoin2(_parent, expr, _result);
                return;
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
                return;
            }


            int pos1 = _result.MarkSbPosition();

            AnalyzeExpression(recurData, expr.Expression);

            string varName = _result.Substring(pos1);
            //_result.Revert(pos1);

            _result.tablesUsed[expr.Expression.Type] = varName;
            _result.AppendString(".");

            //TODO: this appends "Alltypes.int_" whereas int should append "Alltypes.`int`"
            ColumnAttribute columnAttrib = expr.Member.GetCustomAttributes(false).OfType<ColumnAttribute>().FirstOrDefault();
            string sqlColumnName = expr.Member.Name;
            if (columnAttrib != null)
                sqlColumnName = s_vendor.FieldName_Safe(columnAttrib.Name);
            _result.AppendString(sqlColumnName);
        }

        /// <summary>
        /// handle 'p.ProductName.Length' etc
        /// </summary>
        /// <param name="recurData"></param>
        /// <param name="memberExpr"></param>
        void AnalyzeBuiltinMember(RecurData recurData, MemberExpression memberOuter)
        {
            MemberExpression memberInner = memberOuter.Expression.XMember();
            if (memberInner.Type == typeof(string) && memberOuter.Member.Name=="Length")
            {
                //process string length function here. 
                //"LENGTH()" function seems to be available on Oracle,Mysql,PostgreSql
                //Ha! it's called LEN() on MssqlServer
                string length_func = s_vendor.String_Length_Function();
                _result.AppendString(length_func + "(");
                AnalyzeExpression(recurData, memberInner);
                _result.AppendString(")");
            }

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
            switch (expr.Method.Name)
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
                        string paramName = _parent.lastParamName;
                        string lastParam = _parent.paramMap[paramName] as string;
                        if (lastParam != null)
                        {
                            //modify parameter from X to X%
                            string modParam = "";
                            switch (expr.Method.Name)
                            {
                                case "StartsWith": modParam = lastParam + "%"; break;
                                case "EndWith": modParam = "%" + lastParam; break;
                                case "Contains": modParam = "%" + lastParam + "%"; break;
                            }
                            _parent.paramMap[paramName] = modParam;
                        }
                    }
                    return;
                case "Sum":
                    {
                        //extract 'OrderID' from '{g.Sum(o => Convert(o.OrderID))}'
                        Expression sumExpr1 = expr.Arguments[1].XLambda().Body;
                        MemberExpression sumExpr2 = null;
                        switch (sumExpr1.NodeType)
                        {
                            //case ExpressionType.Cast: //Cast disappeared in Beta2?!
                            //    sumExpr2 = sumExpr1.XCastOperand().XMember(); break;
                            case ExpressionType.MemberAccess:
                                sumExpr2 = sumExpr1.XMember(); break;
                            case ExpressionType.Convert:
                                //eg. {g.Sum(o => Convert(o.OrderID))} (from G08_OrderSumByCustomerID)
                                sumExpr2 = sumExpr1.XUnary().Operand.XMember();
                                break;
                            default:
                                throw new ArgumentException("L277 Sum(lambda): unprepared for lambda expr " + sumExpr1.NodeType);
                        }
                        _result.AppendString("SUM(" + sumExpr2.Member.Name + ")");
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
                        //this was discontinued after Linq 20006 preview
                        throw new ApplicationException("L581 Discontinued operand: Concat");
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

            //check if the function is a stored proc:
            object[] oCustomAttribs = expr.Method.GetCustomAttributes(false);
            FunctionExAttribute functionAttrib = oCustomAttribs.OfType<FunctionExAttribute>().FirstOrDefault();
            if (functionAttrib != null)
            {
                //it's a stored proc in the database
                _result.AppendString(functionAttrib.Name + "(");
                string comma = "";
                foreach (Expression functionArg in expr.Arguments)
                {
                    _result.AppendString(comma); comma = ",";
                    AnalyzeExpression(recurData, functionArg);
                }
                _result.AppendString(")");
                return;
            }

            //TODO: throw for any other method - database probably cannot handle such call
            string msg2 = "L274: Unprepared to map method " + methodName + " (" + expr + ") to SQL";
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
                        AnalyzeExpression(recurData, operandMember.Expression); //process {e.ReportsTo}
                        _result.AppendString(" IS NOT NULL ");
                        return; //end special case
                    }
                }
                _result.AppendString(" NOT ");
            }

            AnalyzeExpression(recurData, expr.Operand);

            if (isNot)
                return;

            string operatorStr = "UNOP:" + expr.NodeType.ToString(); //formatBinaryOperator(expr.NodeType);
            _result.AppendString(" " + operatorStr + " ");
        }

        public class NameAndType
        {
            public string name;
            public Type type;
        }

        private void AnalyzeBinary(RecurData recurData, BinaryExpression expr)
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
                string sqlConcatStr = s_vendor.Concat(strings);
                _result.AppendString(sqlConcatStr);
                return;
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
                    return;
                }
            }

            int precedence = Operators.GetPrecedence(expr.NodeType);
            bool needsBrackets = (recurData.operatorPrecedence > precedence);
            recurData.operatorPrecedence = precedence; //nested methods will see different precedence

            if (needsBrackets)
            {
                _result.AppendString("(");
            }

            AnalyzeExpression(recurData, expr.Left);

            string operatorStr = Operators.FormatBinaryOperator(expr.NodeType);
            _result.AppendString(" " + operatorStr + " ");

            AnalyzeExpression(recurData, expr.Right);
            if (needsBrackets)
            {
                _result.AppendString(")");
            }
        }

        public struct RecurData
        {
            public int depth;
            public int operatorPrecedence;
            //public bool selectAllFields;
        }
    }
}
